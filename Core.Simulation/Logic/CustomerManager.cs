using System.Collections.Generic;
using Core.Simulation.Data;

namespace Core.Simulation.Logic
{
    public sealed class CustomerManager
    {
        private readonly DeterministicRandom _rng;

        public int Reputation { get; private set; }
        public int Satisfaction { get; private set; }
        public int DailyDemandMultiplier { get; private set; }
        public List<int> RecentPurchases { get; } = new();

        public CustomerManager(int seed)
        {
            _rng = new DeterministicRandom(seed);
            Reputation = 50;
            Satisfaction = 50;
            DailyDemandMultiplier = 1;
        }

        public void UpdateDemand(GameManager game)
        {
            int reputationFactor = Reputation / 25;
            int moraleFactor = game.Employees.AverageMorale / 25;
            int satisfactionFactor = Satisfaction / 30;
            int progressionBoost = game.Progression.GetDemandBoostBasisPoints();
            int eventModifier = game.Events.GetDemandModifierBasisPoints();
            int moodModifier = game.GetGlobalDemandBasisPoints();

            long scaled = 1 + reputationFactor + moraleFactor + satisfactionFactor;
            scaled = scaled * progressionBoost / 10_000;
            scaled = scaled * eventModifier / 10_000;
            scaled = scaled * moodModifier / 10_000;
            DailyDemandMultiplier = System.Math.Max(1, (int)scaled);
        }

        public int CalculateProductDemand(Product product, GameManager game)
        {
            int priceRatioBasisPoints = product.SalePrice.ToMicros() == 0
                ? 10_000
                : (int)System.Math.Clamp(product.CostPrice.ToMicros() * 10_000 / product.SalePrice.ToMicros(), 3_500, 15_000);

            int baseDemandHundredths = 220 + product.Popularity * 12 + DailyDemandMultiplier * 55;
            int priceSensitivityBasisPoints = System.Math.Max(1_000, 15_000 - priceRatioBasisPoints);
            int categoryDemandBasisPoints = GetCategoryDemandBasisPoints(product);
            int categoryPriceBasisPoints = GetCategoryPriceSensitivityBasisPoints(product);
            int employeeBonusBasisPoints = 10_000 + game.Employees.AverageMorale * 50;
            int customerServiceBasisPoints = game.Employees.CustomerServiceDemandBasisPoints;
            int decorationBasisPoints = 10_000 + game.DecorationDemandBonusBasisPoints;
            int eventModifier = game.Events.GetDemandModifierBasisPoints();

            long demandHundredths = baseDemandHundredths;
            demandHundredths = demandHundredths * priceSensitivityBasisPoints / 10_000;
            demandHundredths = demandHundredths * categoryDemandBasisPoints / 10_000;
            demandHundredths = demandHundredths * categoryPriceBasisPoints / 10_000;
            demandHundredths = demandHundredths * employeeBonusBasisPoints / 10_000;
            demandHundredths = demandHundredths * customerServiceBasisPoints / 10_000;
            demandHundredths = demandHundredths * decorationBasisPoints / 10_000;
            demandHundredths = demandHundredths * eventModifier / 10_000;

            int demand = (int)((demandHundredths + 50) / 100) + _rng.Next(-3, 4);
            return System.Math.Max(0, demand);
        }

        private static int GetCategoryDemandBasisPoints(Product product)
        {
            return product.Id switch
            {
                1 => 11_500,
                2 => 7_000,
                3 => 4_500,
                4 => 9_500,
                5 => 6_500,
                6 => 2_500,
                7 => 10_500,
                8 => 1_800,
                _ => 10_000
            };
        }

        private static int GetCategoryPriceSensitivityBasisPoints(Product product)
        {
            return product.Id switch
            {
                1 => 10_500,
                2 => 8_000,
                3 => 7_000,
                4 => 9_500,
                5 => 7_500,
                6 => 5_500,
                7 => 11_000,
                8 => 4_500,
                _ => 10_000
            };
        }

        public void RecordPurchase(GameManager game, Product product, int quantity)
        {
            RecentPurchases.Add(quantity);
            Satisfaction = System.Math.Min(100, Satisfaction + quantity);
            ApplyReputationEvent(game, quantity / 5, ReputationChangeSource.CustomerService);
        }

        public void RecordStockout(GameManager game, Product product)
        {
            Satisfaction = System.Math.Max(0, Satisfaction - 5);
            ApplyReputationEvent(game, -2, ReputationChangeSource.Stockout);
        }

        public void RecordQueuePressure(GameManager game, int lostCustomers, int mitigationBasisPoints = 10_000)
        {
            if (lostCustomers <= 0)
                return;

            int mitigatedLoss = (int)(lostCustomers * (long)mitigationBasisPoints / 10_000);
            Satisfaction = System.Math.Max(0, Satisfaction - System.Math.Min(8, mitigatedLoss));
            ApplyReputationEvent(game, mitigatedLoss >= 5 ? -1 : 0, ReputationChangeSource.QueuePressure);
        }

        public void ApplyReputationEvent(GameManager game, int delta, ReputationChangeSource source = ReputationChangeSource.Generic)
        {
            int adjustedDelta = game.GetAdjustedReputationDelta(delta, source);
            Reputation = Math.Clamp(Reputation + adjustedDelta, 0, 100);
        }

        public void SetState(int reputation, int satisfaction, int demandMultiplier)
        {
            Reputation = Math.Clamp(reputation, 0, 100);
            Satisfaction = Math.Clamp(satisfaction, 0, 100);
            DailyDemandMultiplier = System.Math.Max(1, demandMultiplier);
        }

        public bool HasDemandEvent() => _rng.ChanceBasisPoints(1_000);
    }
}
