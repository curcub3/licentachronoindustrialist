using System.Collections.Generic;
using System.Linq;
using Core.Simulation.Data;

namespace Core.Simulation.Logic
{
    public sealed class ProgressionManager
    {
        private readonly HashSet<string> _unlockedFeatures = new();

        public IReadOnlyCollection<string> UnlockedFeatures => _unlockedFeatures.ToList().AsReadOnly();

        public bool IsUnlocked(string key) => _unlockedFeatures.Contains(key);

        public void Unlock(string key)
        {
            _unlockedFeatures.Add(key);
        }

        public void LoadUnlocks(IEnumerable<string> unlockKeys)
        {
            _unlockedFeatures.Clear();
            foreach (var key in unlockKeys)
                _unlockedFeatures.Add(key);
        }

        public IEnumerable<string> GetUnlockKeys() => _unlockedFeatures;

        public void Evaluate(GameManager game)
        {
            if (game.Economy.TotalProfit >= Money.FromUnits(5000) && !IsUnlocked("AdvancedSupply"))
                Unlock("AdvancedSupply");

            if (game.Economy.TotalProfit >= Money.FromUnits(15000) && !IsUnlocked("MarketingCampaign"))
                Unlock("MarketingCampaign");

            if (game.Customers.Reputation >= 80 && !IsUnlocked("PremiumBrand"))
                Unlock("PremiumBrand");
        }

        public int GetDemandBoostBasisPoints() => IsUnlocked("MarketingCampaign") ? 11_500 : 10_000;

        public int GetSupplyReliabilityBonusBasisPoints() => IsUnlocked("AdvancedSupply") ? 11_000 : 10_000;

        public int GetEmployeeMoraleBonusBasisPoints() => IsUnlocked("PremiumBrand") ? 10_500 : 10_000;
    }
}
