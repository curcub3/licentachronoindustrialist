using Core.Simulation.Data;

namespace Core.Simulation.Logic
{
    public sealed class EconomyManager
    {
        public Money Cash { get; set; }
        public Money DailyRevenue { get; private set; }
        public Money DailyExpenses { get; private set; }
        public Money DailyProfit { get; private set; }
        public Money TotalProfit { get; private set; }
        public Money DailyVatDue { get; private set; }
        public Money DailyVatAccrued { get; private set; }
        public Money DailyVatPaid { get; private set; }
        public Money DailyBusinessTax { get; private set; }
        public Money DailyFixedCosts { get; private set; }
        public Money DailyPayroll { get; private set; }
        public Money DailyRent { get; private set; }
        public Money DailyUtilities { get; private set; }
        public Money DailySupplierCosts { get; private set; }
        public Money VatOwed { get; private set; }
        public int NextTaxDueDay { get; private set; }
        public int NextRentDueDay { get; private set; }
        public Money ProjectedCash => Cash + DailyRevenue - DailyExpenses;

        public EconomicConfig Config { get; } = new();

        public EconomyManager()
        {
            Cash = Money.Zero;
            DailyRevenue = Money.Zero;
            DailyExpenses = Money.Zero;
            DailyProfit = Money.Zero;
            TotalProfit = Money.Zero;
            DailyVatDue = Money.Zero;
            DailyVatAccrued = Money.Zero;
            DailyVatPaid = Money.Zero;
            DailyBusinessTax = Money.Zero;
            DailyFixedCosts = Money.Zero;
            DailyPayroll = Money.Zero;
            DailyRent = Money.Zero;
            DailyUtilities = Money.Zero;
            DailySupplierCosts = Money.Zero;
            VatOwed = Money.Zero;
            NextTaxDueDay = Config.TaxIntervalDays;
            NextRentDueDay = Config.RentIntervalDays;
        }

        public void RecordRevenue(Money revenue)
        {
            DailyRevenue += revenue;
            DailyVatAccrued += CalculateBasisPoints(revenue, Config.VatBasisPoints);
            VatOwed += CalculateBasisPoints(revenue, Config.VatBasisPoints);
        }

        public void RecordExpense(Money expense)
        {
            DailyExpenses += expense;
        }

        public void RecordSupplierExpense(Money expense)
        {
            DailySupplierCosts += expense;
            RecordExpense(expense);
        }

        public void RecordPayrollExpense(Money expense)
        {
            DailyPayroll += expense;
            RecordExpense(expense);
        }

        public bool CanAfford(Money expense)
        {
            return ProjectedCash >= expense;
        }

        public void ApplyRomanianEconomicPressure(int day, int inflationBasisPoints, int decorationLevel, int hardwareLevel)
        {
            DailyUtilities = ApplyBasisPoints(Config.EffectiveDailyUtilities + Money.FromUnits(hardwareLevel * 8), inflationBasisPoints);
            DailyRent = day >= NextRentDueDay
                ? ApplyBasisPoints(Config.EffectiveWeeklyRent + Money.FromUnits(decorationLevel * 45 + hardwareLevel * 25), inflationBasisPoints)
                : Money.Zero;

            DailyBusinessTax = CalculateBasisPoints(DailyRevenue, Config.MicroenterpriseTaxBasisPoints);
            DailyVatDue = VatOwed;
            DailyVatPaid = day >= NextTaxDueDay ? VatOwed : Money.Zero;
            DailyFixedCosts = DailyRent + DailyUtilities;
            DailyExpenses += DailyFixedCosts + DailyBusinessTax + DailyVatPaid;

            if (day >= NextTaxDueDay)
            {
                if (Cash + DailyRevenue - DailyExpenses < Money.Zero)
                    DailyExpenses += Money.FromUnits(35);

                VatOwed = Money.Zero;
                NextTaxDueDay += Config.TaxIntervalDays;
            }

            if (day >= NextRentDueDay)
                NextRentDueDay += Config.RentIntervalDays;
        }

        public void SettleDaily()
        {
            DailyProfit = DailyRevenue - DailyExpenses;
            Cash += DailyProfit;
            TotalProfit += DailyProfit;
            DailyRevenue = Money.Zero;
            DailyExpenses = Money.Zero;
            DailyVatDue = Money.Zero;
            DailyVatAccrued = Money.Zero;
            DailyVatPaid = Money.Zero;
            DailyBusinessTax = Money.Zero;
            DailyFixedCosts = Money.Zero;
            DailyPayroll = Money.Zero;
            DailyRent = Money.Zero;
            DailyUtilities = Money.Zero;
            DailySupplierCosts = Money.Zero;
        }

        public DailyReport CreateReport(
            int day,
            Money startingCash,
            int unitsSold,
            int stockouts,
            int restockedUnits,
            int storageOverflowUnits,
            int lostCheckoutCustomers,
            int lostStockoutCustomers,
            int customersServed,
            int queuePressure,
            int storageUsed,
            int storageCapacity,
            int reputation,
            int satisfaction,
            string eventDescription,
            EconomicMood economicMood,
            string supportEventEffects,
            int bestSellerProductId = 0,
            int worstSellerProductId = 0)
        {
            Money revenue = DailyRevenue;
            Money expenses = DailyExpenses;
            Money profit = revenue - expenses;
            Money endingCash = Cash + profit;

            return new DailyReport(
                day,
                startingCash,
                revenue,
                expenses,
                profit,
                endingCash,
                DailyVatDue,
                DailyBusinessTax,
                DailyFixedCosts,
                revenue,
                DailyVatAccrued,
                DailyVatPaid,
                DailyPayroll,
                DailyRent,
                DailyUtilities,
                DailySupplierCosts,
                profit,
                economicMood,
                supportEventEffects,
                unitsSold,
                stockouts,
                restockedUnits,
                storageOverflowUnits,
                lostCheckoutCustomers,
                lostStockoutCustomers,
                customersServed,
                queuePressure,
                storageUsed,
                storageCapacity,
                reputation,
                satisfaction,
                eventDescription,
                bestSellerProductId,
                worstSellerProductId
            );
        }

        public void SetTotalProfit(Money totalProfit)
        {
            TotalProfit = totalProfit;
        }

        public void RestoreDailyState(Money dailyRevenue, Money dailyExpenses)
        {
            DailyRevenue = dailyRevenue;
            DailyExpenses = dailyExpenses;
            DailyProfit = dailyRevenue - dailyExpenses;
        }

        public void RestoreEconomicState(Money vatOwed, int nextTaxDueDay, int nextRentDueDay)
        {
            VatOwed = vatOwed;
            NextTaxDueDay = Math.Max(1, nextTaxDueDay);
            NextRentDueDay = Math.Max(1, nextRentDueDay);
        }

        private static Money CalculateBasisPoints(Money value, int basisPoints)
        {
            return Money.FromMicros(value.ToMicros() * basisPoints / 10_000);
        }

        private static Money ApplyBasisPoints(Money value, int basisPoints)
        {
            return Money.FromMicros(value.ToMicros() * basisPoints / 10_000);
        }
    }
}
