using System;
using System.Collections.Generic;
using System.Linq;
using Core.Simulation.Data;

namespace Core.Simulation.Logic
{
    public sealed class InventoryManager
    {
        public List<Product> Products { get; } = new();
        public List<ShelfStock> Shelves { get; } = new();
        public List<InventoryOrder> PendingOrders { get; } = new();
        public int StorageCapacity { get; set; } = 160;
        public int TotalStorageUnits => Products.Sum(p => p.Quantity);
        public int FreeStorageUnits => Math.Max(0, StorageCapacity - TotalStorageUnits);
        public int NextShelfId() => Shelves.Count == 0 ? 1 : Shelves.Max(s => s.Id) + 1;

        public void AddProduct(Product product)
        {
            Products.Add(product);
        }

        public Product? GetProduct(int productId)
        {
            return Products.Find(p => p.Id == productId);
        }

        public ShelfStock? GetShelf(int shelfId)
        {
            return Shelves.Find(s => s.Id == shelfId);
        }

        public IEnumerable<ShelfStock> GetShelvesForProduct(int productId)
        {
            return Shelves.Where(s => s.ProductId == productId);
        }

        public void ClearProducts()
        {
            Products.Clear();
        }

        public void LoadProducts(IEnumerable<Product> products)
        {
            Products.Clear();
            Products.AddRange(products);
        }

        public void AddShelf(ShelfStock shelf)
        {
            Shelves.Add(shelf);
        }

        public void ClearShelves()
        {
            Shelves.Clear();
        }

        public void LoadShelves(IEnumerable<ShelfStock> shelves)
        {
            Shelves.Clear();
            Shelves.AddRange(shelves);
        }

        public int RefillShelvesFromStorage(int maxUnits = int.MaxValue)
        {
            int movedTotal = 0;
            foreach (var shelf in Shelves)
            {
                int remainingCapacity = maxUnits - movedTotal;
                if (remainingCapacity <= 0)
                    break;

                int requested = Math.Min(shelf.Capacity - shelf.CurrentStock, remainingCapacity);
                movedTotal += RefillShelf(shelf.Id, requested);
            }

            return movedTotal;
        }

        public bool AssignShelfProduct(int shelfId, int productId)
        {
            var shelf = GetShelf(shelfId);
            var newProduct = GetProduct(productId);
            if (shelf == null || newProduct == null)
                return false;

            var oldProduct = GetProduct(shelf.ProductId);
            if (oldProduct != null)
                oldProduct.Quantity += shelf.CurrentStock;

            shelf.ProductId = productId;
            shelf.CurrentStock = 0;
            return true;
        }

        public int RefillShelf(int shelfId, int quantity)
        {
            if (quantity <= 0)
                return 0;

            var shelf = GetShelf(shelfId);
            if (shelf == null)
                return 0;

            var product = GetProduct(shelf.ProductId);
            if (product == null)
                return 0;

            int availableSpace = Math.Max(0, shelf.Capacity - shelf.CurrentStock);
            int moved = Math.Min(quantity, Math.Min(availableSpace, product.Quantity));
            if (moved <= 0)
                return 0;

            product.Quantity -= moved;
            shelf.CurrentStock += moved;
            return moved;
        }

        public int RefillProductShelves(int productId, int quantity)
        {
            if (quantity <= 0 || GetProduct(productId) == null)
                return 0;

            int remaining = quantity;
            int movedTotal = 0;
            foreach (var shelf in GetShelvesForProduct(productId))
            {
                int moved = RefillShelf(shelf.Id, remaining);
                movedTotal += moved;
                remaining -= moved;

                if (remaining == 0)
                    break;
            }

            return movedTotal;
        }

        public bool PlaceRestockOrder(int productId, int quantity, SupplierProfile supplier, GameManager game)
        {
            if (quantity <= 0) return false;
            var product = Products.Find(p => p.Id == productId);
            if (product == null) return false;

            int costBasisPoints = game.GetSupplierCostBasisPoints(product, supplier);
            long totalMicros = product.CostPrice.ToMicros() * quantity * costBasisPoints / 10_000;
            Money orderCost = new Money(totalMicros);
            if (!game.Economy.CanAfford(orderCost))
                return false;

            int deliveryDelay = supplier.DeliveryDays + Math.Max(0, 100 - supplier.Reliability) / 25;
            PendingOrders.Add(new InventoryOrder(productId, quantity, deliveryDelay, orderCost));
            game.Economy.RecordSupplierExpense(orderCost);
            return true;
        }

        public int ReceiveDeliveries(GameManager game)
        {
            int overflowUnits = 0;
            for (int i = PendingOrders.Count - 1; i >= 0; i--)
            {
                var order = PendingOrders[i];
                order.RemainingDays -= 1;
                if (order.RemainingDays <= 0)
                {
                    var product = Products.Find(p => p.Id == order.ProductId);
                    if (product != null)
                    {
                        int accepted = Math.Min(order.Quantity, FreeStorageUnits);
                        int overflow = order.Quantity - accepted;
                        product.Quantity += accepted;
                        overflowUnits += overflow;
                    }

                    PendingOrders.RemoveAt(i);
                }
            }

            if (overflowUnits > 0)
                game.Customers.ApplyReputationEvent(game, -Math.Min(5, overflowUnits / 10 + 1), ReputationChangeSource.Overflow);

            return overflowUnits;
        }

        public SalesSummary ProcessSales(GameManager game, int demandBasisPoints = 10_000)
        {
            int unitsSold = 0;
            int stockouts = 0;
            int lostCheckoutCustomers = 0;
            int lostStockoutCustomers = 0;
            int queuePressure = 0;
            int checkoutCapacityRemaining = game.CheckoutCapacity;

            foreach (var product in Products)
            {
                int demand = ApplyShelfDemandModifier(product.Id, game.Customers.CalculateProductDemand(product, game));
                demand = (int)(demand * (long)game.GetProductDemandBasisPoints(product) / 10_000);
                demand = (int)(demand * (long)demandBasisPoints / 10_000);
                int shelfStock = GetShelvesForProduct(product.Id).Sum(s => s.CurrentStock);
                int sold = Math.Min(demand, Math.Min(shelfStock, checkoutCapacityRemaining));
                if (sold == 0 && demand > 0)
                {
                    if (shelfStock <= 0)
                    {
                        stockouts++;
                        lostStockoutCustomers += demand;
                        game.Customers.RecordStockout(game, product);
                        product.Popularity = System.Math.Max(0, product.Popularity - 2);
                    }
                    else if (checkoutCapacityRemaining <= 0)
                    {
                        lostCheckoutCustomers += demand;
                        queuePressure += demand;
                        game.Customers.RecordQueuePressure(game, demand, game.GetAdjustedQueuePressureMitigationBasisPoints(game.Employees.QueuePressureMitigationBasisPoints));
                    }

                    continue;
                }

                if (sold > 0)
                {
                    unitsSold += sold;
                    checkoutCapacityRemaining -= sold;
                    RemoveShelfStock(product.Id, sold);
                    Money revenue = product.SalePrice * sold;
                    game.Economy.RecordRevenue(revenue);
                    game.RecordCheckoutFeedback(sold, revenue);
                    game.RecordProductSales(product.Id, sold);
                    game.Customers.RecordPurchase(game, product, sold);
                    product.Popularity = System.Math.Min(100, product.Popularity + sold / 2);

                    int unmetDemand = Math.Max(0, demand - sold);
                    if (unmetDemand > 0 && checkoutCapacityRemaining <= 0)
                    {
                        lostCheckoutCustomers += unmetDemand;
                        queuePressure += unmetDemand;
                        game.Customers.RecordQueuePressure(game, unmetDemand, game.GetAdjustedQueuePressureMitigationBasisPoints(game.Employees.QueuePressureMitigationBasisPoints));
                    }
                    else if (unmetDemand > 0 && GetShelvesForProduct(product.Id).Sum(s => s.CurrentStock) <= 0)
                    {
                        stockouts++;
                        lostStockoutCustomers += unmetDemand;
                        game.Customers.RecordStockout(game, product);
                    }
                }
            }

            return new SalesSummary(unitsSold, stockouts, lostCheckoutCustomers, lostStockoutCustomers, unitsSold, queuePressure);
        }

        private void RemoveShelfStock(int productId, int quantity)
        {
            int remaining = quantity;
            foreach (var shelf in GetShelvesForProduct(productId))
            {
                int removed = Math.Min(remaining, shelf.CurrentStock);
                shelf.CurrentStock -= removed;
                remaining -= removed;

                if (remaining == 0)
                    return;
            }
        }

        private int ApplyShelfDemandModifier(int productId, int demand)
        {
            int modifierBasisPoints = 10_000;
            foreach (var shelf in GetShelvesForProduct(productId))
            {
                modifierBasisPoints = Math.Max(modifierBasisPoints, shelf.DisplayType switch
                {
                    ShelfDisplayType.Featured => 12_500,
                    ShelfDisplayType.Premium => 11_500,
                    _ => 10_000
                });
            }

            return (int)(demand * (long)modifierBasisPoints / 10_000);
        }

        public void ClearOrders()
        {
            PendingOrders.Clear();
        }

        public void LoadPendingOrders(IEnumerable<InventoryOrder> orders)
        {
            PendingOrders.Clear();
            PendingOrders.AddRange(orders);
        }

        public Money CalculatePendingOrderValue()
        {
            return PendingOrders.Aggregate(Money.Zero, (sum, order) => sum + order.TotalCost);
        }
    }
}
