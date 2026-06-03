using Core.Simulation.Data;
using Core.Simulation.Logic;
using Xunit;

namespace Core.Simulation.Tests;

public sealed class GameManagerSmokeTests
{
    [Fact]
    public void VatAccumulatesFromSales()
    {
        using var game = new GameManager(10000);
        game.Economy.RecordRevenue(Money.FromUnits(100));

        Assert.Equal(MoneyFromGrossWithVat(100), game.Economy.VatOwed);
    }

    [Fact]
    public void VatIsPaidOnDueDay()
    {
        using var game = new GameManager(10000);
        game.SetDay(game.Economy.NextTaxDueDay);
        game.Economy.RecordRevenue(Money.FromUnits(100));
        game.Economy.ApplyRomanianEconomicPressure(game.CurrentDay, game.InflationBasisPoints, game.DecorationLevel, game.HardwareLevel);

        Assert.Equal(MoneyFromGrossWithVat(100), game.Economy.DailyVatPaid);
        Assert.Equal(Money.Zero, game.Economy.VatOwed);
    }

    [Fact]
    public void InflationIncreasesSupplierCosts()
    {
        using var game = new GameManager(10000);
        var product = game.Inventory.GetProduct(1)!;
        var supplier = game.Suppliers.GetSupplierById(1)!;

        game.RestoreEconomicState(
            game.ActiveEconomicMood,
            game.Economy.VatOwed,
            game.Economy.NextTaxDueDay,
            game.Economy.NextRentDueDay,
            11_000,
            game.ImportCostPressureBasisPoints,
            game.LastSupportEventEffect);

        Assert.True(game.GetSupplierCostBasisPoints(product, supplier) > supplier.PriceMultiplierBasisPoints);
    }

    [Fact]
    public void EconomicMoodModifiesDemandAndCosts()
    {
        using var game = new GameManager(10000);
        var cheap = game.Inventory.GetProduct(1)!;
        game.RestoreEconomicState(EconomicMood.BudgetMonth, Money.Zero, 7, 7, 10_000, 10_000, "None");
        Assert.True(game.GetProductDemandBasisPoints(cheap) > 10_000);

        var imported = game.Inventory.GetProduct(5)!;
        var supplier = game.Suppliers.GetSupplierById(2)!;
        game.RestoreEconomicState(EconomicMood.ImportSqueeze, Money.Zero, 7, 7, 10_000, 11_500, "None");
        Assert.True(game.GetSupplierCostBasisPoints(imported, supplier) > supplier.PriceMultiplierBasisPoints);
    }

    [Fact]
    public void PayrollExpenseIsReported()
    {
        using var game = new GameManager(10000);
        game.Economy.ApplyRomanianEconomicPressure(game.CurrentDay, game.InflationBasisPoints, game.DecorationLevel, game.HardwareLevel);

        var report = game.Economy.CreateReport(
            game.CurrentDay,
            game.Economy.Cash,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            game.Inventory.TotalStorageUnits,
            game.Inventory.StorageCapacity,
            game.Customers.Reputation,
            game.Customers.Satisfaction,
            "None",
            game.ActiveEconomicMood,
            "None");

        Assert.Equal(game.Employees.TotalSalary, report.Payroll);
    }

    [Fact]
    public void SupportEventChoiceAffectsReputationAndCash()
    {
        using var game = new GameManager(10000);
        int reputationBefore = game.Customers.Reputation;
        Money expensesBefore = game.Economy.DailyExpenses;

        Assert.True(game.TriggerPlaceholderEvent());
        Assert.True(game.ResolveEventDecision(0));
        Assert.True(game.Customers.Reputation > reputationBefore);
        Assert.True(game.Economy.DailyExpenses > expensesBefore);
    }

    [Fact]
    public void RelaxedDifficultySoftensReputationLoss()
    {
        using var relaxed = new GameManager(10000, settings: new GameStartSettings("Relaxat", GameDifficulty.Relaxed, 14));
        using var normal = new GameManager(10000, settings: new GameStartSettings("Normal", GameDifficulty.Normal, 14));

        for (int i = 0; i < 5; i++)
        {
            relaxed.Customers.ApplyReputationEvent(relaxed, -5, ReputationChangeSource.MinorEvent);
            normal.Customers.ApplyReputationEvent(normal, -5, ReputationChangeSource.MinorEvent);
        }

        Assert.True(relaxed.Customers.Reputation > normal.Customers.Reputation);
        Assert.True(relaxed.Customers.Reputation >= 34);
    }

    [Fact]
    public void RelaxedDifficultyBoostsReputationRecovery()
    {
        using var relaxed = new GameManager(10000, settings: new GameStartSettings("Relaxat", GameDifficulty.Relaxed, 14));
        using var normal = new GameManager(10000, settings: new GameStartSettings("Normal", GameDifficulty.Normal, 14));
        var relaxedProduct = relaxed.Inventory.GetProduct(1)!;
        var normalProduct = normal.Inventory.GetProduct(1)!;

        relaxed.Customers.RecordPurchase(relaxed, relaxedProduct, 10);
        normal.Customers.RecordPurchase(normal, normalProduct, 10);

        Assert.True(relaxed.Customers.Reputation > normal.Customers.Reputation);
    }

    [Fact]
    public void SaveLoadRestoresEconomyState()
    {
        string path = Path.Combine(Path.GetTempPath(), $"chrono-test-{Guid.NewGuid():N}.json");

        try
        {
            using var game = new GameManager(10000);
            game.RestoreEconomicState(EconomicMood.ImportSqueeze, Money.FromUnits(123), 14, 21, 10_450, 11_700, "Saved support effect");
            game.SaveGame(path);

            using var loaded = new GameManager(10000);
            loaded.LoadGame(path);

            Assert.Equal(EconomicMood.ImportSqueeze, loaded.ActiveEconomicMood);
            Assert.Equal(Money.FromUnits(123), loaded.Economy.VatOwed);
            Assert.Equal(14, loaded.Economy.NextTaxDueDay);
            Assert.Equal(21, loaded.Economy.NextRentDueDay);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void SaveLoadRestoresPurchasedFurniture()
    {
        string path = Path.Combine(Path.GetTempPath(), $"chrono-furniture-{Guid.NewGuid():N}.json");

        try
        {
            using var game = new GameManager(10000);
            Assert.True(game.PurchaseShopCatalogItem(5, 1));
            Assert.True(game.PurchaseShopCatalogItem(6, 1));
            game.SaveGame(path);

            using var loaded = new GameManager(10000);
            loaded.LoadGame(path);

            Assert.Equal(new[] { 5, 6 }, loaded.PurchasedCatalogItemIds);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static Money MoneyFromGrossWithVat(long grossUnits)
    {
        return Money.FromMicros(Money.FromUnits(grossUnits).ToMicros() * 2_100 / 10_000);
    }
}
