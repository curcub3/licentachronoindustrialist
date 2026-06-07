using System;
using System.Collections.Generic;
using System.Linq;
using Core.Simulation.Data;
using Core.Simulation.Persistence;
using Core.Simulation.Serialization;

namespace Core.Simulation.Logic
{
    public sealed class GameManager : IDisposable
    {
        public SimulationManager Simulation { get; }
        public EconomyManager Economy { get; }
        public InventoryManager Inventory { get; }
        public EmployeeManager Employees { get; }
        public SupplierManager Suppliers { get; }
        public CustomerManager Customers { get; }
        public EventManager Events { get; }
        public ProgressionManager Progression { get; }
        public DataPersistenceManager Persistence { get; }

        public int CurrentDay { get; private set; }
        public int CurrentWeek => (CurrentDay - 1) / 7 + 1;
        public int CurrentMonth => (CurrentDay - 1) / 30 + 1;
        public DayPhase CurrentPhase { get; private set; }
        public int BusinessTicksRemaining { get; private set; }
        public DailyReport LastDailyReport { get; private set; }
        public LoopResult CurrentLoopResult { get; private set; }
        public IReadOnlyList<ShopCatalogItem> ShopCatalog { get; }
        public const int CampaignDurationDays = 14;
        public int LoopDayLimit { get; private set; }
        public Money LoopTargetCash { get; }
        public Money BronzeProfitTarget { get; }
        public Money SilverProfitTarget { get; }
        public Money GoldProfitTarget { get; }
        public Money InsolvencyThreshold { get; }
        public int ReputationFailureThreshold { get; }
        public string StoreName { get; private set; }
        public GameDifficulty Difficulty { get; private set; }
        public int DecorationLevel { get; private set; }
        public int HardwareLevel { get; private set; }
        public EconomicMood ActiveEconomicMood { get; private set; }
        public int InflationBasisPoints { get; private set; }
        public int ImportCostPressureBasisPoints { get; private set; } = 10_000;
        public string LastSupportEventEffect { get; private set; } = "None";
        public int DecorationDemandBonusBasisPoints => DecorationLevel * 250;
        public int CheckoutCapacity => Employees.CheckoutCapacity + HardwareLevel * 2;
        public int DailyRestockCapacity => Employees.DailyRestockCapacity + HardwareLevel * 6;
        public int MinimumReputation => ReputationFailureThreshold;
        public int DaysRemaining => Math.Max(0, LoopDayLimit - CurrentDay + 1);
        public Money RunProfit => Economy.TotalProfit + Economy.DailyRevenue - Economy.DailyExpenses;
        public int BusinessTicksPerDayTotal => BusinessTicksPerDay;
        public bool IsLoopActive => CurrentLoopResult.Status == LoopStatus.Active;
        public GameEvent? CurrentEvent => Events.CurrentEvent;
        public EventDecision? CurrentDecision => Events.CurrentDecision;
        public int CurrentQueueLength { get; private set; }
        public int CurrentCustomersServedToday => _businessSales.CustomersServed;
        public int CurrentCustomersLostToday => _businessSales.LostCheckoutCustomers + _businessSales.LostStockoutCustomers;
        public int CurrentQueuePressureToday => _businessSales.QueuePressure;
        public IReadOnlyList<int> PurchasedCatalogItemIds => _purchasedCatalogItemIds;
        public int BusinessTicksElapsedToday => CurrentPhase == DayPhase.Business
            ? Math.Max(0, BusinessTicksPerDay - BusinessTicksRemaining)
            : CurrentPhase == DayPhase.Management || CurrentPhase == DayPhase.Morning ? 0 : BusinessTicksPerDay;
        public bool IsEarlyGracePeriodActive => Difficulty == GameDifficulty.Relaxed
            && CurrentDay <= RelaxedGraceDays
            && (CurrentPhase != DayPhase.Business || BusinessTicksElapsedToday < RelaxedGraceBusinessTicks);
        public int CurrentSalesWaveDemandBasisPoints => GetCurrentSalesWaveDemandBasisPoints();
        public string LastReputationFeedbackRo { get; private set; } = "";
        public int LastReputationFeedbackSequence { get; private set; }
        public string LastCheckoutFeedbackRo { get; private set; } = "";
        public int LastCheckoutFeedbackSequence { get; private set; }
        public static readonly Money MaximumProductSalePrice = Money.FromUnits(9_999);

        private const int BusinessTicksPerDay = 1_800;
        private const int SalesWavesPerDay = 5;
        private const int SalesWaveIntervalTicks = BusinessTicksPerDay / SalesWavesPerDay;
        private const int SalesWaveDemandBasisPoints = 10_000 / SalesWavesPerDay;
        private const int RelaxedStartingCashUnits = 3_200;
        private const int RelaxedStartingReputation = 65;
        private const int RelaxedStartingSatisfaction = 60;
        private const int RelaxedGraceDays = 2;
        private const int RelaxedGraceBusinessTicks = 1_200;
        private Money _dayStartingCash;
        private SalesSummary _businessSales;
        private readonly Dictionary<int, int> _dayProductSales = new();
        private readonly List<int> _purchasedCatalogItemIds = new();
        private static readonly IReadOnlyDictionary<int, Money> DefaultProductSalePrices = new Dictionary<int, Money>
        {
            [1] = Money.FromUnits(45),
            [2] = Money.FromUnits(180),
            [3] = Money.FromUnits(85),
            [4] = Money.FromUnits(75),
            [5] = Money.FromUnits(120),
            [6] = Money.FromUnits(340),
            [7] = Money.FromUnits(28),
            [8] = Money.FromUnits(520)
        };
        private int _businessRestockBudgetRemaining;
        private int _dayRestockedUnits;
        private int _dayStorageOverflowUnits;

        public GameManager(int maxEntities, int ghostCount = 0, GameStartSettings? settings = null)
        {
            var startSettings = settings ?? GameStartSettings.Default;
            Simulation = new SimulationManager(maxEntities, ghostCount);
            Economy = new EconomyManager();
            Inventory = new InventoryManager();
            Employees = new EmployeeManager();
            Suppliers = new SupplierManager();
            Customers = new CustomerManager(seed: 12345);
            Events = new EventManager(seed: 12345);
            Progression = new ProgressionManager();
            Persistence = new DataPersistenceManager();
            ShopCatalog = CreateShopCatalog();
            StoreName = string.IsNullOrWhiteSpace(startSettings.StoreName) ? GameStartSettings.Default.StoreName : startSettings.StoreName.Trim();
            Difficulty = startSettings.Difficulty;
            LoopDayLimit = NormalizeCampaignDuration(startSettings.RunDurationDays);
            BronzeProfitTarget = Money.FromUnits(2_500);
            SilverProfitTarget = Money.FromUnits(4_500);
            GoldProfitTarget = Money.FromUnits(6_500);
            LoopTargetCash = GoldProfitTarget;
            InsolvencyThreshold = Money.Zero;
            ReputationFailureThreshold = 1;
            CurrentPhase = DayPhase.Morning;
            LastDailyReport = DailyReport.Empty(1, Money.Zero);
            CurrentLoopResult = LoopResult.Active(1, Money.Zero, LoopTargetCash, 50, ReputationFailureThreshold);
            ActiveEconomicMood = PickEconomicMood(maxEntities + ghostCount);
            InflationBasisPoints = Economy.Config.BaseInflationBasisPoints;

            InitializeDefaultState();
            StartMorning();
        }

        private void InitializeDefaultState()
        {
            CurrentDay = 1;
            DecorationLevel = 0;
            HardwareLevel = 0;
            _purchasedCatalogItemIds.Clear();
            Economy.Cash = Difficulty switch
            {
                GameDifficulty.Relaxed => Money.FromUnits(RelaxedStartingCashUnits),
                GameDifficulty.Hard => Money.FromUnits(1050),
                _ => Money.FromUnits(1500)
            };
            if (Difficulty == GameDifficulty.Relaxed)
                Customers.SetState(RelaxedStartingReputation, RelaxedStartingSatisfaction, 1);
            ImportCostPressureBasisPoints = ActiveEconomicMood == EconomicMood.ImportSqueeze ? 11_500 : 10_000;
            LastSupportEventEffect = "None";
            _dayStartingCash = Economy.Cash;
            LastDailyReport = DailyReport.Empty(CurrentDay, Economy.Cash);
            CurrentLoopResult = LoopResult.Active(CurrentDay, Economy.Cash, LoopTargetCash, Customers.Reputation, ReputationFailureThreshold);
            Inventory.AddProduct(new Product(1, "Retro Cartridge", 50, Money.FromUnits(20), Money.FromUnits(45), 80, 1_500));
            Inventory.AddProduct(new Product(2, "Classic Console", 10, Money.FromUnits(100), Money.FromUnits(180), 60, 6_500));
            Inventory.AddProduct(new Product(3, "Collector's Box", 8, Money.FromUnits(40), Money.FromUnits(85), 45, 8_000));
            Inventory.AddProduct(new Product(4, "Handheld Game", 24, Money.FromUnits(35), Money.FromUnits(75), 72, 4_000));
            Inventory.AddProduct(new Product(5, "Import RPG", 12, Money.FromUnits(55), Money.FromUnits(120), 58, 8_500));
            Inventory.AddProduct(new Product(6, "Arcade Board", 4, Money.FromUnits(180), Money.FromUnits(340), 35, 7_500));
            Inventory.AddProduct(new Product(7, "Repair Kit", 30, Money.FromUnits(12), Money.FromUnits(28), 65, 500));
            InitializeDefaultShelves(autoRefill: Difficulty != GameDifficulty.Relaxed);
            Employees.AddEmployee(new EmployeeProfile("Alex", "Manager", 80, Money.FromUnits(85), 80));
            Employees.AddEmployee(new EmployeeProfile("Mara", "Cashier", 70, Money.FromUnits(55), 75));
            Employees.AddEmployee(new EmployeeProfile("Niko", "Stocker", 65, Money.FromUnits(50), 70));
            Employees.RefreshCandidates(CurrentDay);
            Suppliers.AddSupplier(new SupplierProfile(1, "Retro Supply Co.", 9_500, 2, 95));
            Suppliers.AddSupplier(new SupplierProfile(2, "Budget Importer", 8_500, 4, 72));
            Suppliers.AddSupplier(new SupplierProfile(3, "Express Collector Hub", 12_500, 1, 88));

            SeedVisibleSimulationItems();
        }

        private void SeedVisibleSimulationItems()
        {
            var items = Simulation.Active.Items;
            int id = 0;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    items.Add(new Item
                    {
                        ID = id,
                        Type = 1,
                        Position = new IntVector2(x - 4, y - 4),
                        Progress = (byte)((x + y) % 64)
                    });
                    id++;
                }
            }
        }

        public void Tick()
        {
            switch (CurrentPhase)
            {
                case DayPhase.Morning:
                    StartMorning();
                    break;
                case DayPhase.Management:
                    StartBusiness();
                    break;
                case DayPhase.Business:
                    TickBusiness();
                    break;
                case DayPhase.Closing:
                    AdvanceToNextDay();
                    break;
            }
        }

        public void StartMorning()
        {
            if (!IsLoopActive)
                return;

            _dayStartingCash = Economy.Cash;
            _businessSales = new SalesSummary(0, 0);
            _dayProductSales.Clear();
            _businessRestockBudgetRemaining = 0;
            _dayRestockedUnits = 0;
            _dayStorageOverflowUnits = 0;
            CurrentQueueLength = 0;
            BusinessTicksRemaining = 0;
            InflationBasisPoints = Economy.Config.BaseInflationBasisPoints + (CurrentDay - 1) * Economy.Config.DailyInflationStepBasisPoints;
            if (ActiveEconomicMood == EconomicMood.UtilityShock)
                InflationBasisPoints += 250;

            Events.InjectEvents(this);
            Employees.UpdateDaily(this);
            Employees.RemoveExpiredCandidates(CurrentDay);
            if ((CurrentDay - 1) % 7 == 0 && Employees.Candidates.Count == 0)
                Employees.RefreshCandidates(CurrentDay);
            _dayStorageOverflowUnits += Suppliers.ProcessDeliveries(this);
            if (Difficulty != GameDifficulty.Relaxed || CurrentDay > 1)
                _dayRestockedUnits += Inventory.RefillShelvesFromStorage(DailyRestockCapacity);
            Customers.UpdateDemand(this);
            ApplyRelaxedEarlyRecoverySupport();

            CurrentPhase = DayPhase.Management;
        }

        public bool StartBusiness()
        {
            if (!IsLoopActive || CurrentPhase != DayPhase.Management)
                return false;

            BusinessTicksRemaining = BusinessTicksPerDay;
            _businessRestockBudgetRemaining = DailyRestockCapacity;
            CurrentPhase = DayPhase.Business;
            return true;
        }

        public void TickBusiness()
        {
            if (!IsLoopActive || CurrentPhase != DayPhase.Business)
                return;

            if (BusinessTicksRemaining % SalesWaveIntervalTicks == 0)
                ProcessBusinessWave();

            Simulation.Tick();
            BusinessTicksRemaining = Math.Max(0, BusinessTicksRemaining - 1);

            if (BusinessTicksRemaining == 0)
                CloseDay();
        }

        public void CloseDay()
        {
            if (CurrentPhase == DayPhase.Closing)
                return;

            Economy.ApplyRomanianEconomicPressure(CurrentDay, InflationBasisPoints, DecorationLevel, HardwareLevel);
            string eventDescription = CurrentEvent?.Description ?? "None";
            int bestSellerProductId = GetBestSellerProductId();
            int worstSellerProductId = GetWorstSellerProductId();
            LastDailyReport = Economy.CreateReport(
                CurrentDay,
                _dayStartingCash,
                _businessSales.UnitsSold,
                _businessSales.Stockouts,
                _dayRestockedUnits,
                _dayStorageOverflowUnits,
                _businessSales.LostCheckoutCustomers,
                _businessSales.LostStockoutCustomers,
                _businessSales.CustomersServed,
                _businessSales.QueuePressure,
                Inventory.TotalStorageUnits,
                Inventory.StorageCapacity,
                Customers.Reputation,
                Customers.Satisfaction,
                eventDescription,
                ActiveEconomicMood,
                LastSupportEventEffect,
                bestSellerProductId,
                worstSellerProductId
            );

            Economy.SettleDaily();
            Progression.Evaluate(this);
            EvaluateLoopResult();
            CurrentPhase = DayPhase.Closing;
            CurrentQueueLength = 0;
        }

        public void AdvanceToNextDay()
        {
            if (!IsLoopActive || CurrentPhase != DayPhase.Closing)
                return;

            CurrentDay += 1;
            StartMorning();
        }

        private void EvaluateLoopResult()
        {
            if (Economy.Cash < InsolvencyThreshold)
            {
                CurrentLoopResult = new LoopResult(
                    LoopStatus.Failed,
                    CurrentDay,
                    Economy.Cash,
                    LoopTargetCash,
                    Customers.Reputation,
                    ReputationFailureThreshold,
                    $"Business failed: cash dropped below {InsolvencyThreshold}."
                );
                return;
            }

            if (Customers.Reputation < ReputationFailureThreshold)
            {
                CurrentLoopResult = new LoopResult(
                    LoopStatus.Failed,
                    CurrentDay,
                    Economy.Cash,
                    LoopTargetCash,
                    Customers.Reputation,
                    ReputationFailureThreshold,
                    $"Business failed: reputation collapsed below {ReputationFailureThreshold}."
                );
                return;
            }

            if (CurrentDay >= LoopDayLimit)
            {
                Money finalProfit = Economy.TotalProfit;
                bool reachedBronze = finalProfit >= BronzeProfitTarget;
                CurrentLoopResult = new LoopResult(
                    reachedBronze ? LoopStatus.Won : LoopStatus.Expired,
                    CurrentDay,
                    Economy.Cash,
                    LoopTargetCash,
                    Customers.Reputation,
                    ReputationFailureThreshold,
                    reachedBronze
                        ? $"14-day campaign complete: {GetProfitMedalName(finalProfit)} target reached with {finalProfit} profit."
                        : $"14-day campaign complete: profit {finalProfit} stayed below bronze target {BronzeProfitTarget}."
                );
                return;
            }

            CurrentLoopResult = LoopResult.Active(CurrentDay, Economy.Cash, LoopTargetCash, Customers.Reputation, ReputationFailureThreshold);
        }

        private void ProcessBusinessWave()
        {
            if (_businessRestockBudgetRemaining > 0)
            {
                int restockQuota = Math.Max(1, DailyRestockCapacity / SalesWavesPerDay);
                int moved = Inventory.RefillShelvesFromStorage(Math.Min(restockQuota, _businessRestockBudgetRemaining));
                _businessRestockBudgetRemaining -= moved;
                _dayRestockedUnits += moved;
            }

            var wave = Inventory.ProcessSales(this, CurrentSalesWaveDemandBasisPoints);
            _businessSales += wave;
            CurrentQueueLength = Math.Clamp(wave.LostCheckoutCustomers, 0, 12);
        }

        public void SaveGame(string path)
        {
            var save = Persistence.Capture(this);
            Persistence.SaveWithBackup(path, save);
        }

        public void LoadGame(string path)
        {
            var save = Persistence.Load(path);
            Persistence.Validate(save);
            Persistence.Restore(this, save);
        }

        public bool PlaceRestockOrder(int productId, int quantity, int supplierId)
        {
            var supplier = Suppliers.GetSupplierById(supplierId);
            if (supplier == null) return false;
            return Inventory.PlaceRestockOrder(productId, quantity, supplier, this);
        }

        public int GetSupplierCostBasisPoints(Product product, SupplierProfile supplier)
        {
            long basisPoints = supplier.PriceMultiplierBasisPoints;
            basisPoints = basisPoints * InflationBasisPoints / 10_000;
            int importPressure = 10_000 + ((ImportCostPressureBasisPoints - 10_000) * product.ImportSensitivityBasisPoints / 10_000);
            basisPoints = basisPoints * importPressure / 10_000;
            basisPoints = basisPoints * GetMoodCostBasisPoints(product) / 10_000;
            return (int)Math.Clamp(basisPoints, 1_000, 50_000);
        }

        public int GetGlobalDemandBasisPoints()
        {
            int moodBasisPoints = ActiveEconomicMood switch
            {
                EconomicMood.NostalgiaBoom => 11_500,
                EconomicMood.CollectorCraze => 10_500,
                EconomicMood.UtilityShock => 9_200,
                _ => 10_000
            };

            int difficultyBasisPoints = Difficulty switch
            {
                GameDifficulty.Relaxed => 10_800,
                GameDifficulty.Hard => 9_200,
                _ => 10_000
            };

            return (int)(moodBasisPoints * (long)difficultyBasisPoints / 10_000);
        }

        public int GetProductDemandBasisPoints(Product product)
        {
            return ActiveEconomicMood switch
            {
                EconomicMood.BudgetMonth when product.SalePrice <= Money.FromUnits(80) => 12_500,
                EconomicMood.BudgetMonth => 7_500,
                EconomicMood.NostalgiaBoom when product.Id is 1 or 2 or 4 => 12_500,
                EconomicMood.CollectorCraze when product.Id is 3 or 8 => 14_000,
                EconomicMood.CollectorCraze when product.SalePrice > Money.FromUnits(150) => 11_000,
                EconomicMood.ImportSqueeze when product.ImportSensitivityBasisPoints >= 6_000 => 8_500,
                EconomicMood.UtilityShock when product.SalePrice <= Money.FromUnits(80) => 11_000,
                EconomicMood.UtilityShock => 8_800,
                _ => 10_000
            };
        }

        private int GetMoodCostBasisPoints(Product product)
        {
            int moodBasisPoints = ActiveEconomicMood switch
            {
                EconomicMood.ImportSqueeze when product.ImportSensitivityBasisPoints >= 4_000 => 11_500,
                EconomicMood.CollectorCraze when product.Id is 3 or 8 => 10_800,
                EconomicMood.BudgetMonth when product.SalePrice <= Money.FromUnits(80) => 9_700,
                _ => 10_000
            };

            int difficultyBasisPoints = Difficulty switch
            {
                GameDifficulty.Relaxed => 9_500,
                GameDifficulty.Hard => 10_800,
                _ => 10_000
            };

            return (int)(moodBasisPoints * (long)difficultyBasisPoints / 10_000);
        }

        public bool PurchaseShopCatalogItem(int catalogItemId, int selectedProductId)
        {
            if (CurrentPhase != DayPhase.Management)
                return false;

            var item = ShopCatalog.FirstOrDefault(i => i.Id == catalogItemId);
            if (item == null)
                return false;
            if (!Economy.CanAfford(item.Cost))
                return false;

            Product? shelfProduct = null;
            if (item.Type == ShopCatalogItemType.Shelf)
            {
                shelfProduct = Inventory.GetProduct(selectedProductId) ?? Inventory.Products.FirstOrDefault();
                if (shelfProduct == null)
                    return false;
            }

            Economy.RecordExpense(item.Cost);
            bool purchased = false;
            switch (item.Type)
            {
                case ShopCatalogItemType.Shelf:
                    Inventory.AddShelf(new ShelfStock(
                        Inventory.NextShelfId(),
                        shelfProduct!.Id,
                        item.ShelfCapacity,
                        0,
                        item.ShelfDisplayType));
                    purchased = true;
                    break;

                case ShopCatalogItemType.Storage:
                    Inventory.StorageCapacity += item.StorageCapacityBonus;
                    purchased = true;
                    break;

                case ShopCatalogItemType.Decoration:
                    DecorationLevel += item.DecorationBonus;
                    Customers.ApplyReputationEvent(this, 1, ReputationChangeSource.Upgrade);
                    purchased = true;
                    break;

                case ShopCatalogItemType.Hardware:
                    HardwareLevel += item.HardwareBonus;
                    purchased = true;
                    break;

                default:
                    return false;
            }

            if (purchased)
                _purchasedCatalogItemIds.Add(item.Id);

            return purchased;
        }

        public IReadOnlyList<OnboardingObjectiveState> GetOnboardingObjectives()
        {
            return OnboardingObjectiveCatalog.Objectives
                .Select(objective => new OnboardingObjectiveState(objective, IsOnboardingObjectiveCompleted(objective.Id)))
                .ToList();
        }

        public OnboardingObjectiveState? GetCurrentOnboardingObjective()
        {
            return GetOnboardingObjectives().FirstOrDefault(objective => !objective.IsCompleted);
        }

        public bool SetProductPrice(int productId, Money salePrice)
        {
            var product = Inventory.GetProduct(productId);
            if (product == null || salePrice < Money.Zero || salePrice > MaximumProductSalePrice) return false;
            product.SalePrice = salePrice;
            return true;
        }

        public void RecordProductSales(int productId, int unitsSold)
        {
            if (unitsSold <= 0)
                return;

            if (_dayProductSales.ContainsKey(productId))
                _dayProductSales[productId] += unitsSold;
            else
                _dayProductSales[productId] = unitsSold;
        }

        public string GetProfitMedalName(Money? profitOverride = null)
        {
            Money profit = profitOverride ?? RunProfit;
            if (profit >= GoldProfitTarget)
                return "Gold";
            if (profit >= SilverProfitTarget)
                return "Silver";
            if (profit >= BronzeProfitTarget)
                return "Bronze";

            return "No medal";
        }

        public Money GetNextProfitTarget(Money? profitOverride = null)
        {
            Money profit = profitOverride ?? RunProfit;
            if (profit < BronzeProfitTarget)
                return BronzeProfitTarget;
            if (profit < SilverProfitTarget)
                return SilverProfitTarget;
            if (profit < GoldProfitTarget)
                return GoldProfitTarget;

            return GoldProfitTarget;
        }

        public bool AssignShelfProduct(int shelfId, int productId)
        {
            if (CurrentPhase != DayPhase.Management)
                return false;

            return Inventory.AssignShelfProduct(shelfId, productId);
        }

        public int RefillShelf(int shelfId, int quantity)
        {
            if (CurrentPhase != DayPhase.Management)
                return 0;

            return Inventory.RefillShelf(shelfId, quantity);
        }

        public int RefillProductShelves(int productId, int quantity)
        {
            if (CurrentPhase != DayPhase.Management)
                return 0;

            return Inventory.RefillProductShelves(productId, quantity);
        }

        public bool HireEmployee(EmployeeProfile employee)
        {
            if (!Economy.CanAfford(employee.Salary * 2)) return false;
            Employees.AddEmployee(employee);
            return true;
        }

        public bool HireEmployee(string name, EmployeeRole role)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return HireEmployee(Employees.CreateEmployee(name.Trim(), role));
        }

        public bool HireCandidate(int candidateId)
        {
            if (CurrentPhase != DayPhase.Management)
                return false;

            return Employees.HireCandidate(candidateId, this);
        }

        public bool FireEmployee(string employeeName)
        {
            return Employees.RemoveEmployeeByName(employeeName);
        }

        public bool ResolveEventDecision(int option)
        {
            if (CurrentPhase != DayPhase.Management)
                return false;

            if (option < 0 || option > 1)
                return false;

            return Events.ResolveDecision(this, option);
        }

        public void ApplyImportPressure(int basisPoints)
        {
            ImportCostPressureBasisPoints = Math.Clamp(basisPoints, 8_000, 15_000);
        }

        public void RecordSupportEventEffect(string effect)
        {
            LastSupportEventEffect = string.IsNullOrWhiteSpace(effect) ? "None" : effect;
        }

        public void RecordReputationFeedback(int adjustedDelta, ReputationChangeSource source)
        {
            if (adjustedDelta == 0)
                return;

            string sign = adjustedDelta > 0 ? $"+{adjustedDelta}" : adjustedDelta.ToString();
            string cause = source switch
            {
                ReputationChangeSource.Stockout => "rafturile sunt goale",
                ReputationChangeSource.QueuePressure => "clienții au așteptat prea mult la casă",
                ReputationChangeSource.CustomerService => "client servit cu succes",
                ReputationChangeSource.Upgrade => "magazin îmbunătățit",
                ReputationChangeSource.Overflow => "depozitul este supraîncărcat",
                ReputationChangeSource.MinorEvent => "eveniment minor",
                ReputationChangeSource.MajorEvent => "eveniment major",
                _ => "schimbare în magazin"
            };
            LastReputationFeedbackRo = $"Reputație {sign}: {cause}.";
            LastReputationFeedbackSequence += 1;
        }

        public void RecordCheckoutFeedback(int unitsSold, Money revenue)
        {
            if (unitsSold <= 0 || revenue <= Money.Zero)
                return;

            string unitText = unitsSold == 1 ? "client servit" : $"{unitsSold} clienți serviți";
            LastCheckoutFeedbackRo = $"Casă: {unitText}. Venit +{revenue}.";
            LastCheckoutFeedbackSequence += 1;
        }

        public Money GetAdjustedPayrollCost(Money payroll)
        {
            if (Difficulty != GameDifficulty.Relaxed)
                return payroll;

            int basisPoints = IsEarlyGracePeriodActive ? 5_000 : 8_000;
            return Money.FromMicros(payroll.ToMicros() * basisPoints / 10_000);
        }

        public int GetAdjustedQueuePressureMitigationBasisPoints(int mitigationBasisPoints)
        {
            if (Difficulty != GameDifficulty.Relaxed)
                return mitigationBasisPoints;

            int basisPoints = IsEarlyGracePeriodActive ? 4_500 : 7_500;
            return Math.Max(1_000, mitigationBasisPoints * basisPoints / 10_000);
        }

        public int GetGuidedCustomerVisualCount(int desiredCount)
        {
            if (Difficulty != GameDifficulty.Relaxed)
                return desiredCount;

            if (IsEarlyGracePeriodActive)
                return Math.Min(desiredCount, CurrentCustomersServedToday == 0 ? 2 : 3);

            if (CurrentDay <= 3)
                return Math.Min(desiredCount, 4);

            return desiredCount;
        }

        public bool TriggerDebugEvent()
        {
            if (CurrentPhase != DayPhase.Management)
                return false;

            return Events.TriggerDebugEvent(this);
        }

        public BusinessRuntimeSaveData CaptureBusinessRuntime()
        {
            return new BusinessRuntimeSaveData(
                _dayStartingCash.ToMicros(),
                _businessSales.UnitsSold,
                _businessSales.Stockouts,
                _businessSales.LostCheckoutCustomers,
                _businessSales.LostStockoutCustomers,
                _businessRestockBudgetRemaining,
                _dayRestockedUnits,
                _dayStorageOverflowUnits,
                Economy.DailyRevenue.ToMicros(),
                Economy.DailyExpenses.ToMicros(),
                _businessSales.CustomersServed,
                _businessSales.QueuePressure
            );
        }

        public void ResetTick()
        {
            Simulation.ResetTick();
            CurrentDay = 1;
            CurrentPhase = DayPhase.Management;
            BusinessTicksRemaining = 0;
            _businessSales = new SalesSummary(0, 0);
            CurrentQueueLength = 0;
            _businessRestockBudgetRemaining = 0;
            _dayRestockedUnits = 0;
            _dayStorageOverflowUnits = 0;
            _dayStartingCash = Economy.Cash;
            _dayProductSales.Clear();
            LastDailyReport = DailyReport.Empty(CurrentDay, Economy.Cash);
            CurrentLoopResult = LoopResult.Active(CurrentDay, Economy.Cash, LoopTargetCash, Customers.Reputation, ReputationFailureThreshold);
            LastSupportEventEffect = "None";
        }

        public void SetDay(int day)
        {
            CurrentDay = Math.Max(1, day);
        }

        public void RestoreRuntimeState(DayPhase phase, int businessTicksRemaining, DailyReport lastReport, LoopResult loopResult, GameEvent? currentEvent, bool hasPendingDecision, BusinessRuntimeSaveData? businessRuntime = null)
        {
            CurrentPhase = phase;
            BusinessTicksRemaining = Math.Max(0, businessTicksRemaining);
            LastDailyReport = lastReport;
            CurrentLoopResult = loopResult;
            Events.RestoreEvent(currentEvent, hasPendingDecision);
            if (businessRuntime != null)
            {
                _dayStartingCash = Money.FromMicros(businessRuntime.DayStartingCashMicros);
                _businessSales = new SalesSummary(
                    businessRuntime.UnitsSold,
                    businessRuntime.Stockouts,
                    businessRuntime.LostCheckoutCustomers,
                    businessRuntime.LostStockoutCustomers,
                    businessRuntime.CustomersServed,
                    businessRuntime.QueuePressure);
                CurrentQueueLength = phase == DayPhase.Business ? Math.Clamp(businessRuntime.QueuePressure, 0, 12) : 0;
                _businessRestockBudgetRemaining = businessRuntime.RestockBudgetRemaining;
                _dayRestockedUnits = businessRuntime.RestockedUnits;
                _dayStorageOverflowUnits = businessRuntime.StorageOverflowUnits;
                Economy.RestoreDailyState(Money.FromMicros(businessRuntime.DailyRevenueMicros), Money.FromMicros(businessRuntime.DailyExpensesMicros));
            }
            else
            {
                _dayStartingCash = phase == DayPhase.Closing ? lastReport.StartingCash : Economy.Cash;
            }
        }

        public void RestoreShopUpgrades(int decorationLevel, int hardwareLevel)
        {
            DecorationLevel = Math.Max(0, decorationLevel);
            HardwareLevel = Math.Max(0, hardwareLevel);
        }

        public void RestorePurchasedCatalogItemIds(IEnumerable<int>? purchasedCatalogItemIds)
        {
            _purchasedCatalogItemIds.Clear();
            if (purchasedCatalogItemIds == null)
                return;

            var validCatalogIds = ShopCatalog.Select(item => item.Id).ToHashSet();
            foreach (int catalogItemId in purchasedCatalogItemIds)
            {
                if (validCatalogIds.Contains(catalogItemId))
                    _purchasedCatalogItemIds.Add(catalogItemId);
            }
        }

        public void RestoreEconomicState(EconomicMood mood, Money vatOwed, int nextTaxDueDay, int nextRentDueDay, int inflationBasisPoints, int importCostPressureBasisPoints, string? supportEffect)
        {
            ActiveEconomicMood = mood;
            InflationBasisPoints = Math.Max(1_000, inflationBasisPoints);
            ImportCostPressureBasisPoints = Math.Max(1_000, importCostPressureBasisPoints);
            LastSupportEventEffect = string.IsNullOrWhiteSpace(supportEffect) ? "None" : supportEffect;
            Economy.RestoreEconomicState(vatOwed, nextTaxDueDay, nextRentDueDay);
        }

        public void RestoreStartSettings(string? storeName, GameDifficulty? difficulty, int? runDurationDays)
        {
            StoreName = string.IsNullOrWhiteSpace(storeName) ? GameStartSettings.Default.StoreName : storeName.Trim();
            Difficulty = difficulty ?? GameStartSettings.Default.Difficulty;
            LoopDayLimit = NormalizeCampaignDuration(runDurationDays ?? GameStartSettings.Default.RunDurationDays);
        }

        private static int NormalizeCampaignDuration(int days)
        {
            return CampaignDurationDays;
        }

        public int GetAdjustedReputationDelta(int delta, ReputationChangeSource source)
        {
            if (delta == 0 || Difficulty != GameDifficulty.Relaxed)
                return delta;

            double multiplier = delta > 0
                ? GetRelaxedGainMultiplier(source)
                : GetRelaxedLossMultiplier(source) * (CurrentDay <= 3 ? 0.75d : 1d);

            int adjustedDelta = ScaleReputationDelta(delta, multiplier);
            if (adjustedDelta < 0)
            {
                int floor = GetRelaxedEarlyReputationFloor();
                if (floor > 0 && Customers.Reputation + adjustedDelta < floor)
                    adjustedDelta = floor - Customers.Reputation;
            }

            return adjustedDelta;
        }

        private int GetBestSellerProductId()
        {
            if (_dayProductSales.Count > 0)
                return _dayProductSales.OrderByDescending(entry => entry.Value).ThenBy(entry => entry.Key).First().Key;

            return Inventory.Products.OrderByDescending(product => product.Popularity).ThenBy(product => product.Id).FirstOrDefault()?.Id ?? 0;
        }

        private int GetWorstSellerProductId()
        {
            var products = Inventory.Products.ToList();
            if (products.Count == 0)
                return 0;

            return products
                .OrderBy(product => _dayProductSales.TryGetValue(product.Id, out int sold) ? sold : 0)
                .ThenBy(product => product.Popularity)
                .ThenBy(product => product.Id)
                .First()
                .Id;
        }

        private static int ScaleReputationDelta(int delta, double multiplier)
        {
            if (delta == 0)
                return 0;

            int magnitude = (int)Math.Round(Math.Abs(delta) * multiplier, MidpointRounding.AwayFromZero);
            if (magnitude == 0 && Math.Abs(delta) >= 2 && multiplier > 0d)
                magnitude = 1;

            return delta > 0 ? magnitude : -magnitude;
        }

        private double GetRelaxedLossMultiplier(ReputationChangeSource source)
        {
            if (IsEarlyGracePeriodActive)
            {
                return source switch
                {
                    ReputationChangeSource.Stockout => 0.18d,
                    ReputationChangeSource.QueuePressure => 0.22d,
                    ReputationChangeSource.Overflow => 0.25d,
                    ReputationChangeSource.MinorEvent => 0.25d,
                    ReputationChangeSource.MajorEvent => 0.35d,
                    _ => 0.25d
                };
            }

            return source switch
            {
                ReputationChangeSource.Stockout => 0.40d,
                ReputationChangeSource.QueuePressure => 0.45d,
                ReputationChangeSource.Overflow => 0.40d,
                ReputationChangeSource.MinorEvent => 0.35d,
                ReputationChangeSource.MajorEvent => 0.50d,
                _ => 0.45d
            };
        }

        private double GetRelaxedGainMultiplier(ReputationChangeSource source)
        {
            return source switch
            {
                ReputationChangeSource.CustomerService => IsEarlyGracePeriodActive ? 1.80d : 1.55d,
                ReputationChangeSource.Upgrade => 1.25d,
                _ => 1.35d
            };
        }

        private int GetRelaxedEarlyReputationFloor()
        {
            if (Difficulty != GameDifficulty.Relaxed || CurrentDay > 3)
                return 0;

            return Math.Max(ReputationFailureThreshold, 60 - ((CurrentDay - 1) * 3));
        }

        private static EconomicMood PickEconomicMood(int seed)
        {
            var moods = Enum.GetValues<EconomicMood>();
            return moods[Math.Abs(seed) % moods.Length];
        }

        private static IReadOnlyList<ShopCatalogItem> CreateShopCatalog()
        {
            return new List<ShopCatalogItem>
            {
                new(1, "Budget Rack", ShopCatalogItemType.Shelf, Money.FromUnits(120), ShelfCapacity: 24, ShelfDisplayType: ShelfDisplayType.Basic),
                new(2, "Premium Display", ShopCatalogItemType.Shelf, Money.FromUnits(320), ShelfCapacity: 12, ShelfDisplayType: ShelfDisplayType.Premium),
                new(3, "Featured Endcap", ShopCatalogItemType.Shelf, Money.FromUnits(260), ShelfCapacity: 16, ShelfDisplayType: ShelfDisplayType.Featured),
                new(4, "Storage Shelving", ShopCatalogItemType.Storage, Money.FromUnits(450), StorageCapacityBonus: 60),
                new(5, "Neon Decor Set", ShopCatalogItemType.Decoration, Money.FromUnits(300), DecorationBonus: 1),
                new(6, "Checkout Scanner", ShopCatalogItemType.Hardware, Money.FromUnits(550), HardwareBonus: 1),
                new(7, "Stock Cart", ShopCatalogItemType.Hardware, Money.FromUnits(420), HardwareBonus: 1)
            };
        }

        public void InitializeDefaultShelves(bool autoRefill = true)
        {
            Inventory.ClearShelves();
            Inventory.AddShelf(new ShelfStock(1, 1, 35, 0, ShelfDisplayType.Basic));
            Inventory.AddShelf(new ShelfStock(2, 1, 25, 0, ShelfDisplayType.Featured));
            Inventory.AddShelf(new ShelfStock(3, 2, 10, 0, ShelfDisplayType.Premium));
            Inventory.AddShelf(new ShelfStock(4, 3, 8, 0, ShelfDisplayType.Premium));
            Inventory.AddShelf(new ShelfStock(5, 4, 18, 0, ShelfDisplayType.Basic));
            Inventory.AddShelf(new ShelfStock(6, 5, 10, 0, ShelfDisplayType.Featured));
            Inventory.AddShelf(new ShelfStock(7, 6, 3, 0, ShelfDisplayType.Premium));
            Inventory.AddShelf(new ShelfStock(8, 7, 24, 0, ShelfDisplayType.Basic));
            if (autoRefill)
                Inventory.RefillShelvesFromStorage();
        }

        private int GetCurrentSalesWaveDemandBasisPoints()
        {
            if (Difficulty != GameDifficulty.Relaxed)
                return SalesWaveDemandBasisPoints;

            if (IsEarlyGracePeriodActive)
                return SalesWaveDemandBasisPoints * (BusinessTicksElapsedToday < SalesWaveIntervalTicks ? 45 : 65) / 100;

            if (CurrentDay <= 3)
                return SalesWaveDemandBasisPoints * 80 / 100;

            return SalesWaveDemandBasisPoints;
        }

        private bool IsOnboardingObjectiveCompleted(string objectiveId)
        {
            return objectiveId switch
            {
                "stock_first_shelf" => Inventory.Shelves.Any(shelf => shelf.CurrentStock > 0),
                "set_first_price" => Inventory.Products.Any(product => DefaultProductSalePrices.TryGetValue(product.Id, out var defaultPrice) && product.SalePrice != defaultPrice),
                "serve_first_customer" => CurrentCustomersServedToday > 0 || LastDailyReport.CustomersServed > 0,
                "hire_first_worker" => Employees.Employees.Count > 3,
                "buy_first_shelf" => Inventory.Shelves.Count > 8,
                "keep_reputation_60" => Customers.Reputation >= 60,
                "serve_five_customers" => CurrentCustomersServedToday >= 5 || LastDailyReport.CustomersServed >= 5,
                "finish_first_day" => CurrentDay > 1 || (CurrentPhase == DayPhase.Closing && LastDailyReport.Day >= 1),
                _ => false
            };
        }

        private void ApplyRelaxedEarlyRecoverySupport()
        {
            if (Difficulty != GameDifficulty.Relaxed || CurrentDay > RelaxedGraceDays)
                return;

            if (Economy.ProjectedCash >= Money.FromUnits(250))
                return;

            Money support = Money.FromUnits(450);
            Economy.Cash += support;
            LastSupportEventEffect = $"Sprijin de început: +{support}";
            Customers.ApplyReputationEvent(this, 1, ReputationChangeSource.Upgrade);
        }

        public void Dispose()
        {
            Simulation.Dispose();
        }
    }
}
