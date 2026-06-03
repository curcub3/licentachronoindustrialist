using System.Collections.Generic;
using System.Linq;
using Core.Simulation.Data;

namespace Core.Simulation.Logic
{
    public sealed class SupplierManager
    {
        public List<SupplierProfile> Suppliers { get; } = new();
        public IReadOnlyList<InventoryOrder> ActiveOrders => PendingOrders;

        private readonly List<InventoryOrder> PendingOrders = new();

        public void AddSupplier(SupplierProfile supplier)
        {
            Suppliers.Add(supplier);
        }

        public void ClearSuppliers()
        {
            Suppliers.Clear();
        }

        public void LoadSuppliers(IEnumerable<SupplierProfile> suppliers)
        {
            Suppliers.Clear();
            Suppliers.AddRange(suppliers);
        }

        public bool PlaceOrder(int productId, int quantity, int supplierId, GameManager game)
        {
            var supplier = GetSupplierById(supplierId);
            if (supplier == null) return false;
            return game.Inventory.PlaceRestockOrder(productId, quantity, supplier, game);
        }

        public SupplierProfile? GetSupplierById(int id)
        {
            return Suppliers.Find(s => s.Id == id);
        }

        public int ProcessDeliveries(GameManager game)
        {
            return game.Inventory.ReceiveDeliveries(game);
        }

        public void ApplySupplyShock(GameManager game, int intensity)
        {
            if (Suppliers.Count == 0) return;
            Suppliers[0].Reliability = System.Math.Max(0, Suppliers[0].Reliability - intensity);
            game.Economy.RecordExpense(Money.FromUnits(intensity * 5));
        }
    }
}
