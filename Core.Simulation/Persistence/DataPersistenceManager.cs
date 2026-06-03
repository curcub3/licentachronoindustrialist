using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Simulation.Data;
using Core.Simulation.Logic;

namespace Core.Simulation.Persistence
{
    public sealed class DataPersistenceManager
    {
        private readonly JsonSerializerOptions _options;

        public DataPersistenceManager()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
            _options.Converters.Add(new MoneyJsonConverter());
        }

        public void Save(string path, GameSaveData snapshot)
        {
            string payload = JsonSerializer.Serialize(snapshot, _options);
            AtomicWrite(path, payload);
        }

        public void SaveWithBackup(string path, GameSaveData snapshot)
        {
            string payload = JsonSerializer.Serialize(snapshot, _options);
            string backupPath = path + ".bak";
            if (File.Exists(path))
                File.Copy(path, backupPath, overwrite: true);

            AtomicWrite(path, payload);
        }

        public GameSaveData Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Save file not found.", path);

            string payload = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<GameSaveData>(payload, _options);
            return data ?? throw new InvalidOperationException("Failed to deserialize save data.");
        }

        public void Validate(GameSaveData data)
        {
            if (data.Products == null) throw new InvalidDataException("Save file contains no product data.");
            if (data.Employees == null) throw new InvalidDataException("Save file contains no employee data.");
            if (data.Suppliers == null) throw new InvalidDataException("Save file contains no supplier data.");
            if (data.PendingOrders == null) throw new InvalidDataException("Save file contains no pending orders.");
            if (data.CurrentDay < 1) throw new InvalidDataException("Save file contains invalid day index.");
        }

        public GameSaveData Capture(GameManager game)
        {
            var products = game.Inventory.Products
                .Select(p => new ProductSaveData(p.Id, p.Name, p.Quantity, p.CostPrice.ToMicros(), p.SalePrice.ToMicros(), p.Popularity, p.ImportSensitivityBasisPoints))
                .ToList();

            var shelves = game.Inventory.Shelves
                .Select(s => new ShelfSaveData(s.Id, s.ProductId, s.Capacity, s.CurrentStock, s.DisplayType))
                .ToList();

            var employees = game.Employees.Employees
                .Select(e => new EmployeeSaveData(e.Name, e.Role, e.Efficiency, e.Salary.ToMicros(), e.Morale))
                .ToList();

            var candidates = game.Employees.Candidates
                .Select(c => new EmployeeCandidateSaveData(
                    c.Id,
                    c.Profile.Name,
                    c.Profile.Role,
                    c.Profile.Efficiency,
                    c.Profile.Salary.ToMicros(),
                    c.Profile.Morale,
                    c.AvailableUntilDay))
                .ToList();

            var suppliers = game.Suppliers.Suppliers
                .Select(s => new SupplierSaveData(s.Id, s.Name, s.PriceMultiplierBasisPoints, s.DeliveryDays, s.Reliability))
                .ToList();

            var orders = game.Inventory.PendingOrders
                .Select(o => new InventoryOrderSaveData(o.ProductId, o.Quantity, o.RemainingDays, o.TotalCost.ToMicros()))
                .ToList();

            var report = game.LastDailyReport;
            var reportSave = new DailyReportSaveData(
                report.Day,
                report.StartingCash.ToMicros(),
                report.Revenue.ToMicros(),
                report.Expenses.ToMicros(),
                report.Profit.ToMicros(),
                report.EndingCash.ToMicros(),
                report.UnitsSold,
                report.Stockouts,
                report.RestockedUnits,
                report.StorageOverflowUnits,
                report.LostCheckoutCustomers,
                report.LostStockoutCustomers,
                report.Reputation,
                report.CustomerSatisfaction,
                report.EventDescription,
                report.VatDue.ToMicros(),
                report.BusinessTax.ToMicros(),
                report.FixedCosts.ToMicros(),
                report.GrossRevenue.ToMicros(),
                report.VatAccrued.ToMicros(),
                report.VatPaid.ToMicros(),
                report.Payroll.ToMicros(),
                report.Rent.ToMicros(),
                report.Utilities.ToMicros(),
                report.SupplierCosts.ToMicros(),
                report.NetProfit.ToMicros(),
                report.EconomicMood.ToString(),
                report.SupportEventEffects,
                report.CustomersServed,
                report.QueuePressure,
                report.StorageUsed,
                report.StorageCapacity,
                report.BestSellerProductId,
                report.WorstSellerProductId);

            var loop = game.CurrentLoopResult;
            var loopSave = new LoopResultSaveData(
                loop.Status,
                loop.Day,
                loop.Cash.ToMicros(),
                loop.TargetCash.ToMicros(),
                loop.Reputation,
                loop.ReputationFailureThreshold,
                loop.Reason);

            var currentEvent = game.CurrentEvent == null
                ? null
                : new GameEventSaveData(
                    game.CurrentEvent.Type,
                    game.CurrentEvent.Description,
                    game.CurrentEvent.Impact,
                    game.CurrentEvent.DurationDays,
                    game.CurrentEvent.RemainingDays,
                    game.CurrentDecision != null);

            return new GameSaveData(
                game.Economy.Cash.ToMicros(),
                game.Economy.TotalProfit.ToMicros(),
                game.CurrentDay,
                game.Customers.Reputation,
                game.Customers.Satisfaction,
                game.Customers.DailyDemandMultiplier,
                products,
                shelves,
                employees,
                candidates,
                suppliers,
                orders,
                game.Progression.GetUnlockKeys().ToList(),
                game.CurrentPhase,
                game.BusinessTicksRemaining,
                reportSave,
                loopSave,
                currentEvent,
                game.Inventory.StorageCapacity,
                game.CaptureBusinessRuntime(),
                game.DecorationLevel,
                game.HardwareLevel,
                DateTimeOffset.UtcNow.ToString("O"),
                game.ActiveEconomicMood,
                game.Economy.VatOwed.ToMicros(),
                game.Economy.NextTaxDueDay,
                game.Economy.NextRentDueDay,
                game.InflationBasisPoints,
                game.ImportCostPressureBasisPoints,
                game.LastSupportEventEffect,
                game.StoreName,
                game.Difficulty,
                game.LoopDayLimit,
                game.PurchasedCatalogItemIds.ToList()
            );
        }

        public void Restore(GameManager game, GameSaveData data)
        {
            game.Economy.Cash = Money.FromMicros(data.CashMicros);
            game.Economy.SetTotalProfit(Money.FromMicros(data.TotalProfitMicros));
            game.SetDay(data.CurrentDay);
            game.Customers.SetState(data.Reputation, data.CustomerSatisfaction, data.DemandMultiplier);

            var products = data.Products.Select(p => new Product(p.Id, p.Name, p.Quantity, Money.FromMicros(p.CostMicros), Money.FromMicros(p.SaleMicros), p.Popularity, p.ImportSensitivityBasisPoints));
            game.Inventory.LoadProducts(products);
            game.Inventory.StorageCapacity = data.StorageCapacity ?? game.Inventory.StorageCapacity;

            if (data.Shelves != null)
            {
                var shelves = data.Shelves.Select(s => new ShelfStock(s.Id, s.ProductId, s.Capacity, s.CurrentStock, s.DisplayType));
                game.Inventory.LoadShelves(shelves);
            }
            else
            {
                game.InitializeDefaultShelves();
            }

            var employees = data.Employees.Select(e => new EmployeeProfile(e.Name, e.Role, e.Efficiency, Money.FromMicros(e.SalaryMicros), e.Morale));
            game.Employees.LoadEmployees(employees);

            if (data.EmployeeCandidates != null)
            {
                var candidates = data.EmployeeCandidates.Select(c => new EmployeeCandidate(
                    c.Id,
                    new EmployeeProfile(c.Name, c.Role, c.Efficiency, Money.FromMicros(c.SalaryMicros), c.Morale),
                    c.AvailableUntilDay));
                game.Employees.LoadCandidates(candidates);
            }
            else
            {
                game.Employees.RefreshCandidates(data.CurrentDay);
            }

            var suppliers = data.Suppliers.Select(s => new SupplierProfile(s.Id, s.Name, s.PriceMultiplierBasisPoints, s.DeliveryDays, s.Reliability));
            game.Suppliers.LoadSuppliers(suppliers);

            var orders = data.PendingOrders.Select(o => new InventoryOrder(o.ProductId, o.Quantity, o.RemainingDays, Money.FromMicros(o.TotalCostMicros)));
            game.Inventory.LoadPendingOrders(orders);
            game.RestoreShopUpgrades(data.DecorationLevel ?? 0, data.HardwareLevel ?? 0);
            game.RestorePurchasedCatalogItemIds(data.PurchasedCatalogItemIds);

            if (data.UnlockedFeatures != null)
                game.Progression.LoadUnlocks(data.UnlockedFeatures);

            if (data.LastDailyReport != null || data.CurrentLoopResult != null || data.CurrentPhase != null || data.CurrentEvent != null)
            {
                var savedReport = data.LastDailyReport;
                var report = savedReport == null
                    ? DailyReport.Empty(data.CurrentDay, Money.FromMicros(data.CashMicros))
                    : new DailyReport(
                        savedReport.Day,
                        Money.FromMicros(savedReport.StartingCashMicros),
                        Money.FromMicros(savedReport.RevenueMicros),
                        Money.FromMicros(savedReport.ExpensesMicros),
                        Money.FromMicros(savedReport.ProfitMicros),
                        Money.FromMicros(savedReport.EndingCashMicros),
                        Money.FromMicros(savedReport.VatDueMicros),
                        Money.FromMicros(savedReport.BusinessTaxMicros),
                        Money.FromMicros(savedReport.FixedCostsMicros),
                        Money.FromMicros(savedReport.GrossRevenueMicros == 0 ? savedReport.RevenueMicros : savedReport.GrossRevenueMicros),
                        Money.FromMicros(savedReport.VatAccruedMicros),
                        Money.FromMicros(savedReport.VatPaidMicros),
                        Money.FromMicros(savedReport.PayrollMicros),
                        Money.FromMicros(savedReport.RentMicros),
                        Money.FromMicros(savedReport.UtilitiesMicros),
                        Money.FromMicros(savedReport.SupplierCostsMicros),
                        Money.FromMicros(savedReport.NetProfitMicros == 0 ? savedReport.ProfitMicros : savedReport.NetProfitMicros),
                        Enum.TryParse<EconomicMood>(savedReport.EconomicMood, out var reportMood) ? reportMood : data.ActiveEconomicMood ?? EconomicMood.BudgetMonth,
                        string.IsNullOrWhiteSpace(savedReport.SupportEventEffects) ? "None" : savedReport.SupportEventEffects,
                        savedReport.UnitsSold,
                        savedReport.Stockouts,
                        savedReport.RestockedUnits,
                        savedReport.StorageOverflowUnits,
                        savedReport.LostCheckoutCustomers,
                        savedReport.LostStockoutCustomers,
                        savedReport.CustomersServed == 0 ? savedReport.UnitsSold : savedReport.CustomersServed,
                        savedReport.QueuePressure,
                        savedReport.StorageUsed,
                        savedReport.StorageCapacity,
                        savedReport.Reputation,
                        savedReport.CustomerSatisfaction,
                        savedReport.EventDescription,
                        savedReport.BestSellerProductId,
                        savedReport.WorstSellerProductId);

                var savedLoop = data.CurrentLoopResult;
                var loop = savedLoop == null
                    ? LoopResult.Active(data.CurrentDay, Money.FromMicros(data.CashMicros), game.LoopTargetCash, data.Reputation, game.ReputationFailureThreshold)
                    : new LoopResult(
                        savedLoop.Status,
                        savedLoop.Day,
                        Money.FromMicros(savedLoop.CashMicros),
                        Money.FromMicros(savedLoop.TargetCashMicros),
                        savedLoop.Reputation,
                        savedLoop.ReputationFailureThreshold,
                        savedLoop.Reason);

                var savedEvent = data.CurrentEvent;
                GameEvent? currentEvent = null;
                if (savedEvent != null)
                {
                    currentEvent = new GameEvent(savedEvent.Type, savedEvent.Description, savedEvent.Impact, savedEvent.DurationDays);
                    while (currentEvent.RemainingDays > savedEvent.RemainingDays)
                        currentEvent.TickDay();
                }

                game.RestoreRuntimeState(
                    data.CurrentPhase ?? DayPhase.Management,
                    data.BusinessTicksRemaining ?? 0,
                    report,
                    loop,
                    currentEvent,
                    savedEvent?.HasPendingDecision ?? false,
                    data.BusinessRuntime);
            }

            game.RestoreEconomicState(
                data.ActiveEconomicMood ?? EconomicMood.BudgetMonth,
                Money.FromMicros(data.VatOwedMicros),
                data.NextTaxDueDay,
                data.NextRentDueDay,
                data.InflationBasisPoints,
                data.ImportCostPressureBasisPoints,
                data.LastSupportEventEffect);
            game.RestoreStartSettings(data.StoreName, data.Difficulty, data.RunDurationDays);
        }

        private static void AtomicWrite(string path, string payload)
        {
            string tempPath = path + ".tmp";
            File.WriteAllText(tempPath, payload);
            if (File.Exists(path))
            {
                File.Replace(tempPath, path, null);
            }
            else
            {
                File.Move(tempPath, path);
            }
        }

        private sealed class MoneyJsonConverter : JsonConverter<Money>
        {
            public override Money Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return Money.FromMicros(reader.GetInt64());
            }

            public override void Write(Utf8JsonWriter writer, Money value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value.ToMicros());
            }
        }
    }
}
