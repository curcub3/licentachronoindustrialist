using System;
using System.Collections.Generic;

namespace Core.Simulation.Data
{
    public sealed class Product
    {
        public int Id { get; }
        public string Name { get; }
        public int Quantity { get; set; }
        public Money CostPrice { get; }
        public Money SalePrice { get; set; }
        public int Popularity { get; set; }
        public int ImportSensitivityBasisPoints { get; set; }

        public Product(int id, string name, int quantity, Money costPrice, Money salePrice, int popularity, int importSensitivityBasisPoints = 0)
        {
            Id = id;
            Name = name;
            Quantity = quantity;
            CostPrice = costPrice;
            SalePrice = salePrice;
            Popularity = popularity;
            ImportSensitivityBasisPoints = Math.Clamp(importSensitivityBasisPoints, 0, 10_000);
        }
    }

    public enum EconomicMood
    {
        BudgetMonth,
        NostalgiaBoom,
        CollectorCraze,
        ImportSqueeze,
        UtilityShock
    }

    public enum GameDifficulty
    {
        Relaxed,
        Normal,
        Hard
    }

    public sealed record GameStartSettings(
        string StoreName,
        GameDifficulty Difficulty,
        int RunDurationDays
    )
    {
        public static GameStartSettings Default => new("Magazin Retro", GameDifficulty.Normal, 14);
    }

    public sealed record OnboardingObjective(
        string Id,
        string TitleRo,
        string SummaryRo
    );

    public sealed record OnboardingObjectiveState(
        OnboardingObjective Objective,
        bool IsCompleted
    );

    public static class OnboardingObjectiveCatalog
    {
        public static IReadOnlyList<OnboardingObjective> Objectives { get; } = new[]
        {
            new OnboardingObjective("stock_first_shelf", "Aprovizionează primul raft", "Mută produse din depozit pe raft ca să poată cumpăra clienții."),
            new OnboardingObjective("set_first_price", "Setează prețul unui produs", "Deschide Prețuri, alege un produs și confirmă un preț valid."),
            new OnboardingObjective("serve_first_customer", "Servește primul client", "Deschide magazinul și urmărește primul client până la casă."),
            new OnboardingObjective("buy_first_shelf", "Cumpără sau plasează un raft nou", "Investește într-un raft nou pentru mai multă capacitate."),
            new OnboardingObjective("hire_first_worker", "Angajează un lucrător", "Angajează un candidat când ai bani și vrei ajutor în magazin."),
            new OnboardingObjective("keep_reputation_60", "Menține reputația peste 60%", "Ține rafturile pline și coada scurtă."),
            new OnboardingObjective("serve_five_customers", "Servește 5 clienți", "Stabilizează prima zi de vânzare."),
            new OnboardingObjective("finish_first_day", "Finalizează prima zi", "Citește raportul și treci la ziua următoare.")
        };
    }

    public sealed record EconomicConfig(
        int VatBasisPoints = 2_100,
        int MicroenterpriseTaxBasisPoints = 100,
        int TaxIntervalDays = 7,
        int RentIntervalDays = 7,
        int BaseInflationBasisPoints = 10_000,
        int DailyInflationStepBasisPoints = 18,
        Money WeeklyRent = default,
        Money DailyUtilities = default
    )
    {
        public Money EffectiveWeeklyRent => WeeklyRent == default ? Money.FromUnits(520) : WeeklyRent;
        public Money EffectiveDailyUtilities => DailyUtilities == default ? Money.FromUnits(38) : DailyUtilities;
    }

    public sealed class EmployeeProfile
    {
        public string Name { get; }
        public string Role { get; }
        public EmployeeRole RoleType => EmployeeRoleCatalog.Parse(Role);
        public int Efficiency { get; set; }
        public Money Salary { get; }
        public int Morale { get; set; }

        public EmployeeProfile(string name, string role, int efficiency, Money salary, int morale)
        {
            Name = name;
            Role = role;
            Efficiency = efficiency;
            Salary = salary;
            Morale = morale;
        }
    }

    public enum EmployeeRole
    {
        Manager,
        Cashier,
        Stocker,
        SalesAssociate,
        Security
    }

    public static class EmployeeRoleCatalog
    {
        public static string ToDisplayName(EmployeeRole role)
        {
            return role switch
            {
                EmployeeRole.SalesAssociate => "Sales Associate",
                _ => role.ToString()
            };
        }

        public static EmployeeRole Parse(string role)
        {
            if (role.Contains("Stock", StringComparison.OrdinalIgnoreCase))
                return EmployeeRole.Stocker;
            if (role.Contains("Cash", StringComparison.OrdinalIgnoreCase))
                return EmployeeRole.Cashier;
            if (role.Contains("Sales", StringComparison.OrdinalIgnoreCase))
                return EmployeeRole.SalesAssociate;
            if (role.Contains("Security", StringComparison.OrdinalIgnoreCase))
                return EmployeeRole.Security;

            return EmployeeRole.Manager;
        }
    }

    public sealed record EmployeeOperations(
        int Managers,
        int Cashiers,
        int Stockers,
        int SalesAssociates,
        int Security,
        int DailyRestockCapacity,
        int CheckoutCapacity,
        int CustomerServiceDemandBasisPoints,
        int QueuePressureMitigationBasisPoints,
        int MoraleProtection
    );

    public sealed class EmployeeCandidate
    {
        public int Id { get; }
        public EmployeeProfile Profile { get; }
        public int AvailableUntilDay { get; }

        public EmployeeCandidate(int id, EmployeeProfile profile, int availableUntilDay)
        {
            Id = id;
            Profile = profile;
            AvailableUntilDay = availableUntilDay;
        }
    }

    public sealed class SupplierProfile
    {
        public int Id { get; }
        public string Name { get; }
        public int PriceMultiplierBasisPoints { get; }
        public int DeliveryDays { get; }
        public int Reliability { get; set; }

        public SupplierProfile(int id, string name, int priceMultiplierBasisPoints, int deliveryDays, int reliability)
        {
            Id = id;
            Name = name;
            PriceMultiplierBasisPoints = priceMultiplierBasisPoints;
            DeliveryDays = deliveryDays;
            Reliability = reliability;
        }
    }

    public sealed class InventoryOrder
    {
        public int ProductId { get; }
        public int Quantity { get; }
        public int RemainingDays { get; set; }
        public Money TotalCost { get; }

        public InventoryOrder(int productId, int quantity, int remainingDays, Money totalCost)
        {
            ProductId = productId;
            Quantity = quantity;
            RemainingDays = remainingDays;
            TotalCost = totalCost;
        }
    }

    public enum ShelfDisplayType
    {
        Basic,
        Premium,
        Featured
    }

    public enum ShelfState
    {
        Empty,
        LowStock,
        Normal,
        Full
    }

    public sealed class ShelfStock
    {
        public int Id { get; }
        public int ProductId { get; set; }
        public int Capacity { get; set; }
        public int CurrentStock { get; set; }
        public ShelfDisplayType DisplayType { get; set; }
        public ShelfState State
        {
            get
            {
                if (CurrentStock <= 0)
                    return ShelfState.Empty;
                if (CurrentStock >= Capacity)
                    return ShelfState.Full;
                if (CurrentStock <= Math.Max(1, Capacity / 4))
                    return ShelfState.LowStock;

                return ShelfState.Normal;
            }
        }

        public ShelfStock(int id, int productId, int capacity, int currentStock, ShelfDisplayType displayType)
        {
            Id = id;
            ProductId = productId;
            Capacity = capacity;
            CurrentStock = currentStock;
            DisplayType = displayType;
        }
    }

    public enum ShopCatalogItemType
    {
        Shelf,
        Storage,
        Decoration,
        Hardware
    }

    public sealed record ShopCatalogItem(
        int Id,
        string Name,
        ShopCatalogItemType Type,
        Money Cost,
        int ShelfCapacity = 0,
        ShelfDisplayType ShelfDisplayType = ShelfDisplayType.Basic,
        int StorageCapacityBonus = 0,
        int DecorationBonus = 0,
        int HardwareBonus = 0
    );

    public sealed class GameEvent
    {
        public GameEventType Type { get; }
        public string Description { get; }
        public int Impact { get; }
        public int DurationDays { get; }
        public int RemainingDays { get; private set; }

        public GameEvent(GameEventType type, string description, int impact, int durationDays)
        {
            Type = type;
            Description = description;
            Impact = impact;
            DurationDays = durationDays;
            RemainingDays = durationDays;
        }

        public void TickDay()
        {
            RemainingDays = Math.Max(0, RemainingDays - 1);
        }

        public bool IsActive => RemainingDays > 0;
    }

    public sealed class EventDecision
    {
        public GameEventType Type { get; }
        public string Prompt { get; }
        public string OptionA { get; }
        public string OptionB { get; }

        public EventDecision(GameEventType type, string prompt, string optionA, string optionB)
        {
            Type = type;
            Prompt = prompt;
            OptionA = optionA;
            OptionB = optionB;
        }
    }

    public enum GameEventType
    {
        DemandSpike,
        SupplierDelay,
        ReputationCrisis,
        EmployeeStrike,
        HolidayRush,
        PriceFluctuation,
        ImportPressure,
        DefectiveProductComplaint,
        RefundRequest,
        WarrantyDispute
    }

    public enum ReputationChangeSource
    {
        Generic,
        CustomerService,
        Stockout,
        QueuePressure,
        Overflow,
        Upgrade,
        MinorEvent,
        MajorEvent
    }

    public sealed record ProductSaveData(int Id, string Name, int Quantity, long CostMicros, long SaleMicros, int Popularity, int ImportSensitivityBasisPoints = 0);
    public sealed record ShelfSaveData(int Id, int ProductId, int Capacity, int CurrentStock, ShelfDisplayType DisplayType);
    public sealed record EmployeeSaveData(string Name, string Role, int Efficiency, long SalaryMicros, int Morale);
    public sealed record EmployeeCandidateSaveData(int Id, string Name, string Role, int Efficiency, long SalaryMicros, int Morale, int AvailableUntilDay);
    public sealed record SupplierSaveData(int Id, string Name, int PriceMultiplierBasisPoints, int DeliveryDays, int Reliability);
    public sealed record InventoryOrderSaveData(int ProductId, int Quantity, int RemainingDays, long TotalCostMicros);
    public sealed record DailyReportSaveData(int Day, long StartingCashMicros, long RevenueMicros, long ExpensesMicros, long ProfitMicros, long EndingCashMicros, int UnitsSold, int Stockouts, int RestockedUnits, int StorageOverflowUnits, int LostCheckoutCustomers, int LostStockoutCustomers, int Reputation, int CustomerSatisfaction, string EventDescription, long VatDueMicros = 0, long BusinessTaxMicros = 0, long FixedCostsMicros = 0, long GrossRevenueMicros = 0, long VatAccruedMicros = 0, long VatPaidMicros = 0, long PayrollMicros = 0, long RentMicros = 0, long UtilitiesMicros = 0, long SupplierCostsMicros = 0, long NetProfitMicros = 0, string EconomicMood = "", string SupportEventEffects = "", int CustomersServed = 0, int QueuePressure = 0, int StorageUsed = 0, int StorageCapacity = 0, int BestSellerProductId = 0, int WorstSellerProductId = 0);
    public sealed record LoopResultSaveData(LoopStatus Status, int Day, long CashMicros, long TargetCashMicros, int Reputation, int ReputationFailureThreshold, string Reason);
    public sealed record GameEventSaveData(GameEventType Type, string Description, int Impact, int DurationDays, int RemainingDays, bool HasPendingDecision);
    public sealed record BusinessRuntimeSaveData(long DayStartingCashMicros, int UnitsSold, int Stockouts, int LostCheckoutCustomers, int LostStockoutCustomers, int RestockBudgetRemaining, int RestockedUnits, int StorageOverflowUnits, long DailyRevenueMicros, long DailyExpensesMicros, int CustomersServed = 0, int QueuePressure = 0);
    public sealed record GameSaveData(
        long CashMicros,
        long TotalProfitMicros,
        int CurrentDay,
        int Reputation,
        int CustomerSatisfaction,
        int DemandMultiplier,
        List<ProductSaveData> Products,
        List<ShelfSaveData>? Shelves,
        List<EmployeeSaveData> Employees,
        List<EmployeeCandidateSaveData>? EmployeeCandidates,
        List<SupplierSaveData> Suppliers,
        List<InventoryOrderSaveData> PendingOrders,
        List<string> UnlockedFeatures,
        DayPhase? CurrentPhase = null,
        int? BusinessTicksRemaining = null,
        DailyReportSaveData? LastDailyReport = null,
        LoopResultSaveData? CurrentLoopResult = null,
        GameEventSaveData? CurrentEvent = null,
        int? StorageCapacity = null,
        BusinessRuntimeSaveData? BusinessRuntime = null,
        int? DecorationLevel = null,
        int? HardwareLevel = null,
        string? SavedAtUtc = null,
        EconomicMood? ActiveEconomicMood = null,
        long VatOwedMicros = 0,
        int NextTaxDueDay = 7,
        int NextRentDueDay = 7,
        int InflationBasisPoints = 10_000,
        int ImportCostPressureBasisPoints = 10_000,
        string? LastSupportEventEffect = null,
        string? StoreName = null,
        GameDifficulty? Difficulty = null,
        int? RunDurationDays = null,
        List<int>? PurchasedCatalogItemIds = null
    );
}
