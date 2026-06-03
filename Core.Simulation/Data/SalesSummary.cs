namespace Core.Simulation.Data
{
    public readonly struct SalesSummary
    {
        public readonly int UnitsSold;
        public readonly int Stockouts;
        public readonly int LostCheckoutCustomers;
        public readonly int LostStockoutCustomers;
        public readonly int CustomersServed;
        public readonly int QueuePressure;

        public SalesSummary(int unitsSold, int stockouts, int lostCheckoutCustomers = 0, int lostStockoutCustomers = 0, int customersServed = -1, int queuePressure = 0)
        {
            UnitsSold = unitsSold;
            Stockouts = stockouts;
            LostCheckoutCustomers = lostCheckoutCustomers;
            LostStockoutCustomers = lostStockoutCustomers;
            CustomersServed = customersServed < 0 ? unitsSold : customersServed;
            QueuePressure = queuePressure;
        }

        public static SalesSummary operator +(SalesSummary a, SalesSummary b)
        {
            return new SalesSummary(
                a.UnitsSold + b.UnitsSold,
                a.Stockouts + b.Stockouts,
                a.LostCheckoutCustomers + b.LostCheckoutCustomers,
                a.LostStockoutCustomers + b.LostStockoutCustomers,
                a.CustomersServed + b.CustomersServed,
                a.QueuePressure + b.QueuePressure
            );
        }
    }
}
