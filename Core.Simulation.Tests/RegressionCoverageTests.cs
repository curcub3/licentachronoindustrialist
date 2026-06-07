using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Globalization;
using Core.Simulation.Data;
using Core.Simulation.Logic;
using Core.Simulation.Persistence;
using Xunit;

namespace Core.Simulation.Tests;

public sealed class RegressionCoverageTests
{
    [Fact]
    public void NewGameSettingsCreateExpectedInitialState()
    {
        using var game = new GameManager(10000, settings: new GameStartSettings("La Bunica", GameDifficulty.Relaxed, 14));

        Assert.Equal("La Bunica", game.StoreName);
        Assert.Equal(GameDifficulty.Relaxed, game.Difficulty);
        Assert.Equal(GameManager.CampaignDurationDays, game.LoopDayLimit);
        Assert.Equal(DayPhase.Management, game.CurrentPhase);
        Assert.Equal(Money.FromUnits(3200), game.Economy.Cash);
        Assert.Equal(7, game.Inventory.Products.Count);
        Assert.Equal(8, game.Inventory.Shelves.Count);
        Assert.All(game.Inventory.Shelves, shelf => Assert.Equal(0, shelf.CurrentStock));
        Assert.Equal(3, game.Employees.Employees.Count);
        Assert.Contains(game.Employees.Employees, employee => employee.RoleType == EmployeeRole.Cashier);
        Assert.Equal(65, game.Customers.Reputation);
        Assert.Equal(60, game.Customers.Satisfaction);
        Assert.True(game.IsEarlyGracePeriodActive);
    }

    [Fact]
    public void DefaultNewGameStartsInRelaxedGuidedMode()
    {
        using var game = new GameManager(10000);

        Assert.Equal(GameDifficulty.Relaxed, game.Difficulty);
        Assert.All(game.Inventory.Shelves, shelf => Assert.Equal(0, shelf.CurrentStock));
        Assert.False(game.GetOnboardingObjectives().Single(item => item.Objective.Id == "stock_first_shelf").IsCompleted);
    }

    [Fact]
    public void NormalModeKeepsStockedStartingShelves()
    {
        using var game = new GameManager(10000, settings: new GameStartSettings("Normal", GameDifficulty.Normal, 14));

        Assert.Contains(game.Inventory.Shelves, shelf => shelf.CurrentStock > 0);
        Assert.True(game.GetOnboardingObjectives().Single(item => item.Objective.Id == "stock_first_shelf").IsCompleted);
    }

    [Fact]
    public void SaveLoadRestoresStoreCustomerEmployeeFurnitureAndRuntimeState()
    {
        string path = Path.Combine(Path.GetTempPath(), $"chrono-roundtrip-{Guid.NewGuid():N}.json");

        try
        {
            using var game = new GameManager(10000, settings: new GameStartSettings("Sclipici Retro", GameDifficulty.Relaxed, 14));
            Assert.True(game.PurchaseShopCatalogItem(1, 4));
            Assert.True(game.PurchaseShopCatalogItem(5, 1));
            Assert.True(game.PurchaseShopCatalogItem(6, 1));
            Assert.True(game.HireEmployee("Ioana", EmployeeRole.SalesAssociate));
            Assert.True(game.PlaceRestockOrder(1, 3, 1));
            game.Customers.ApplyReputationEvent(game, -4, ReputationChangeSource.Stockout);
            game.Customers.RecordPurchase(game, game.Inventory.GetProduct(1)!, 5);
            game.SetProductPrice(1, Money.FromUnits(49));
            Assert.True(game.StartBusiness());
            game.TickBusiness();
            game.SaveGame(path);

            using var loaded = new GameManager(10000);
            loaded.LoadGame(path);

            Assert.Equal("Sclipici Retro", loaded.StoreName);
            Assert.Equal(GameDifficulty.Relaxed, loaded.Difficulty);
            Assert.Equal(game.CurrentPhase, loaded.CurrentPhase);
            Assert.Equal(game.BusinessTicksRemaining, loaded.BusinessTicksRemaining);
            Assert.Equal(game.Economy.Cash, loaded.Economy.Cash);
            Assert.Equal(game.Customers.Reputation, loaded.Customers.Reputation);
            Assert.Equal(game.Customers.Satisfaction, loaded.Customers.Satisfaction);
            Assert.Equal(game.DecorationLevel, loaded.DecorationLevel);
            Assert.Equal(game.HardwareLevel, loaded.HardwareLevel);
            Assert.Equal(game.PurchasedCatalogItemIds, loaded.PurchasedCatalogItemIds);
            Assert.Equal(game.Inventory.Shelves.Count, loaded.Inventory.Shelves.Count);
            Assert.Contains(loaded.Inventory.Shelves, shelf => shelf.Id > 8 && shelf.ProductId == 4 && shelf.DisplayType == ShelfDisplayType.Basic);
            Assert.Contains(loaded.Employees.Employees, employee => employee.Name == "Ioana" && employee.RoleType == EmployeeRole.SalesAssociate);
            Assert.Single(loaded.Inventory.PendingOrders);
        }
        finally
        {
            DeleteIfExists(path);
            DeleteIfExists(path + ".bak");
        }
    }

    [Fact]
    public void SaveFileCanBeEditedAndReloaded()
    {
        string path = Path.Combine(Path.GetTempPath(), $"chrono-edit-{Guid.NewGuid():N}.json");

        try
        {
            using var game = new GameManager(10000);
            game.SaveGame(path);

            var json = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            json["CashMicros"] = Money.FromUnits(777).ToMicros();
            json["StoreName"] = "Editat Din Save";
            File.WriteAllText(path, json.ToJsonString());

            using var loaded = new GameManager(10000);
            loaded.LoadGame(path);

            Assert.Equal(Money.FromUnits(777), loaded.Economy.Cash);
            Assert.Equal("Editat Din Save", loaded.StoreName);
        }
        finally
        {
            DeleteIfExists(path);
            DeleteIfExists(path + ".bak");
        }
    }

    [Fact]
    public void MissingAndCorruptSavesFailBeforeMutatingCurrentGame()
    {
        string missingPath = Path.Combine(Path.GetTempPath(), $"chrono-missing-{Guid.NewGuid():N}.json");
        string corruptPath = Path.Combine(Path.GetTempPath(), $"chrono-corrupt-{Guid.NewGuid():N}.json");

        try
        {
            using var game = new GameManager(10000, settings: new GameStartSettings("Stabil", GameDifficulty.Hard, 14));
            Money originalCash = game.Economy.Cash;
            int originalReputation = game.Customers.Reputation;

            Assert.Throws<FileNotFoundException>(() => game.LoadGame(missingPath));

            File.WriteAllText(corruptPath, "{ not valid json");
            Assert.ThrowsAny<Exception>(() => game.LoadGame(corruptPath));

            Assert.Equal("Stabil", game.StoreName);
            Assert.Equal(GameDifficulty.Hard, game.Difficulty);
            Assert.Equal(originalCash, game.Economy.Cash);
            Assert.Equal(originalReputation, game.Customers.Reputation);
        }
        finally
        {
            DeleteIfExists(corruptPath);
        }
    }

    [Fact]
    public void InvalidSaveDataIsNotConsideredLoadable()
    {
        var persistence = new DataPersistenceManager();
        var invalidSave = new GameSaveData(
            Money.FromUnits(10).ToMicros(),
            Money.Zero.ToMicros(),
            0,
            50,
            50,
            1,
            new List<ProductSaveData>(),
            new List<ShelfSaveData>(),
            new List<EmployeeSaveData>(),
            new List<EmployeeCandidateSaveData>(),
            new List<SupplierSaveData>(),
            new List<InventoryOrderSaveData>(),
            new List<string>());

        Assert.Throws<InvalidDataException>(() => persistence.Validate(invalidSave));
    }

    [Fact]
    public void PathfinderAvoidsSolidFurnitureTiles()
    {
        var map = new GridMap(7, 5);
        for (int y = 0; y < map.Height; y++)
            map.Tiles[map.GetIndex(3, y)] = 99;
        map.Tiles[map.GetIndex(3, 2)] = -1;

        var path = Pathfinder.FindPath(map, new IntVector2(1, 2), new IntVector2(5, 2));

        Assert.NotEmpty(path);
        Assert.All(path, index =>
        {
            var point = map.GetCoord(index);
            Assert.True(map.IsWalkable(point) || point == new IntVector2(1, 2));
        });
        Assert.Contains(map.GetIndex(3, 2), path);
        Assert.DoesNotContain(map.GetIndex(3, 1), path);
    }

    [Fact]
    public void GodotSceneReferencesExistingAssets()
    {
        string projectRoot = FindRepoRoot();
        foreach (string scenePath in Directory.EnumerateFiles(Path.Combine(projectRoot, "client.godot", "Scenes"), "*.tscn", SearchOption.AllDirectories))
        {
            string scene = File.ReadAllText(scenePath);
            foreach (Match match in Regex.Matches(scene, "path=\"res://([^\"]+)\""))
            {
                string assetPath = Path.Combine(projectRoot, "client.godot", match.Groups[1].Value.Replace('/', Path.DirectorySeparatorChar));
                Assert.True(File.Exists(assetPath), $"{Path.GetRelativePath(projectRoot, scenePath)} references missing asset {match.Groups[1].Value}");
            }
        }
    }

    [Fact]
    public void StartupNewGameAndLoadUiUseRomanianSlotBasedFlow()
    {
        string projectRoot = FindRepoRoot();
        string uiRoot = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Scenes", "UIRoot.tscn"));
        string newGame = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Scenes", "NewGameSetupPanel.tscn"));
        string uiManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "UIManager.cs"));

        Assert.Contains("StartupNewGameButton", uiRoot);
        Assert.Contains("text = \"Joc Nou\"", uiRoot);
        Assert.Contains("StartupLoadFileButton", uiRoot);
        Assert.Contains("text = \"Încarcă Joc\"", uiRoot);
        Assert.Contains("NewGameStoreNameInput", newGame);
        Assert.Contains("NewGameDifficultyPicker", newGame);
        Assert.Contains("NewGameDurationPicker", newGame);
        Assert.DoesNotContain("FileDialog", uiRoot + uiManager);
        Assert.Contains("SaveSlotPopup", uiManager);
        Assert.Contains("slot_{slot}.json", uiManager);
        Assert.Contains("Salvare ilizibilă", uiManager);
        Assert.Contains("Nu există salvări.", uiManager);
        Assert.DoesNotContain("New Game", uiRoot + newGame);
        Assert.DoesNotContain("Load Game", uiRoot + newGame);
    }

    [Fact]
    public void PurchasedShelfVisualsReuseOriginalSceneControls()
    {
        string projectRoot = FindRepoRoot();
        string layoutManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "Store", "StoreLayoutManager.cs"));
        string furnitureController = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "Store", "StoreFurnitureVisualController.cs"));

        Assert.Contains("CreateShelfVisualFromOriginal", layoutManager);
        Assert.Contains("template?.Duplicate()", layoutManager);
        Assert.Contains("CreateShelfVisual(key", furnitureController);
        Assert.DoesNotContain("GeneratedShelf", layoutManager + furnitureController);
        Assert.DoesNotContain("res://Assets/UI/generated", layoutManager + furnitureController);
    }

    [Fact]
    public void PurchasedShelfCatalogItemsCreateSimulationShelves()
    {
        using var game = new GameManager(10000, settings: new GameStartSettings("Rafturi", GameDifficulty.Relaxed, 14));
        int initialShelfCount = game.Inventory.Shelves.Count;

        Assert.True(game.PurchaseShopCatalogItem(1, 1));
        Assert.True(game.PurchaseShopCatalogItem(2, 2));
        Assert.True(game.PurchaseShopCatalogItem(3, 3));

        var purchasedShelves = game.Inventory.Shelves
            .Where(shelf => shelf.Id > initialShelfCount)
            .OrderBy(shelf => shelf.Id)
            .ToList();

        Assert.Collection(purchasedShelves,
            shelf =>
            {
                Assert.Equal(1, shelf.ProductId);
                Assert.Equal(24, shelf.Capacity);
                Assert.Equal(0, shelf.CurrentStock);
                Assert.Equal(ShelfDisplayType.Basic, shelf.DisplayType);
            },
            shelf =>
            {
                Assert.Equal(2, shelf.ProductId);
                Assert.Equal(12, shelf.Capacity);
                Assert.Equal(0, shelf.CurrentStock);
                Assert.Equal(ShelfDisplayType.Premium, shelf.DisplayType);
            },
            shelf =>
            {
                Assert.Equal(3, shelf.ProductId);
                Assert.Equal(16, shelf.Capacity);
                Assert.Equal(0, shelf.CurrentStock);
                Assert.Equal(ShelfDisplayType.Featured, shelf.DisplayType);
            });
        Assert.Contains(1, game.PurchasedCatalogItemIds);
        Assert.Contains(2, game.PurchasedCatalogItemIds);
        Assert.Contains(3, game.PurchasedCatalogItemIds);
    }

    [Fact]
    public void NewGameSetupFlowHasRomanianChoicesAndValidation()
    {
        string projectRoot = FindRepoRoot();
        string newGame = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Scenes", "NewGameSetupPanel.tscn"));
        string uiManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "UIManager.cs"));

        Assert.Contains("NewGameStoreNameInput", newGame);
        Assert.Contains("placeholder_text = \"Magazin Retro\"", newGame);
        Assert.Contains("AddItem(\"Relaxat\", (int)GameDifficulty.Relaxed)", uiManager);
        Assert.Contains("AddItem(\"Normal\", (int)GameDifficulty.Normal)", uiManager);
        Assert.Contains("AddItem(\"Greu\", (int)GameDifficulty.Hard)", uiManager);
        Assert.Contains("AddItem(\"14 zile\", GameManager.CampaignDurationDays)", uiManager);
        Assert.Contains("Numele magazinului nu poate fi gol.", uiManager);
        Assert.Contains("new GameStartSettings(storeName, difficulty, duration)", uiManager);
        Assert.DoesNotContain("New Game", newGame);
        Assert.DoesNotContain("Start Game", uiManager);
    }

    [Fact]
    public void SceneUiTextAvoidsEnglishRegressionLabelsAndEmbeddedHelperText()
    {
        string projectRoot = FindRepoRoot();
        string uiRoot = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Scenes", "UIRoot.tscn"));
        string newGame = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Scenes", "NewGameSetupPanel.tscn"));
        string uiManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "UIManager.cs"));
        string sceneText = uiRoot + "\n" + newGame;

        Assert.DoesNotContain("Cash ", sceneText);
        Assert.DoesNotContain("Profit run", sceneText);
        Assert.DoesNotContain("| Rep ", sceneText);
        Assert.DoesNotContain("{item.Type}", uiManager);
        Assert.Contains("CatalogTypeRo(item.Type)", uiManager);
        Assert.DoesNotContain("hardware +", uiManager);
        Assert.DoesNotMatch("(?i)\\bsearch\\b|căutare|caută", sceneText);

        foreach (string buttonText in ExtractSceneButtonTexts(sceneText))
        {
            Assert.DoesNotMatch("(?i)tooltip|hover|apasă pentru|ține apăsat|click", buttonText);
        }
    }

    [Fact]
    public void OptionButtonsUseEstablishedMenuTheme()
    {
        string projectRoot = FindRepoRoot();
        string theme = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Themes", "ChronoTheme.tres"));

        Assert.Contains("Button/colors/font_color = Color(0.97, 0.91, 0.87, 1)", theme);
        Assert.Contains("OptionButton/colors/font_color = Color(0.97, 0.91, 0.87, 1)", theme);
        Assert.Contains("Button/font_sizes/font_size = 14", theme);
        Assert.Contains("OptionButton/font_sizes/font_size = 14", theme);
        Assert.Contains("OptionButton/styles/disabled = SubResource(\"StyleBoxFlat_button_disabled\")", theme);
        Assert.Contains("OptionButton/styles/hover = SubResource(\"StyleBoxFlat_button_hover\")", theme);
        Assert.Contains("OptionButton/styles/normal = SubResource(\"StyleBoxFlat_button_normal\")", theme);
        Assert.Contains("OptionButton/styles/pressed = SubResource(\"StyleBoxFlat_button_pressed\")", theme);
    }

    [Fact]
    public void UiPopupsAndMenusKeepViewportBoundsContracts()
    {
        string projectRoot = FindRepoRoot();
        string uiRoot = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Scenes", "UIRoot.tscn"));
        string newGame = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Scenes", "NewGameSetupPanel.tscn"));
        string uiManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "UIManager.cs"));

        Assert.Contains("anchors_preset = 15", uiRoot);
        Assert.Contains("anchors_preset = 15", newGame);
        Assert.Contains("FitPopupToViewport", uiManager);
        Assert.Contains("RefreshVisiblePopupBounds", uiManager);
        Assert.Contains("Math.Min(desiredSize.X, availableSize.X)", uiManager);
        Assert.Contains("Math.Min(desiredSize.Y, availableSize.Y)", uiManager);

        foreach (Match match in Regex.Matches(uiRoot + "\n" + newGame, "custom_minimum_size = Vector2\\((?<width>[0-9.]+), (?<height>[0-9.]+)\\)"))
        {
            float width = float.Parse(match.Groups["width"].Value, CultureInfo.InvariantCulture);
            float height = float.Parse(match.Groups["height"].Value, CultureInfo.InvariantCulture);
            Assert.True(width <= 1280f, $"Control minimum width exceeds target viewport: {match.Value}");
            Assert.True(height <= 720f, $"Control minimum height exceeds target viewport: {match.Value}");
        }
    }

    [Fact]
    public void RuntimeUiOpensVisibleFirstRunTutorialOverlay()
    {
        string projectRoot = FindRepoRoot();
        string uiManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "UIManager.cs"));

        Assert.Contains("private bool _tutorialEnabled = true;", uiManager);
        Assert.Contains("SetPrice = 1", uiManager);
        Assert.Contains("Name = \"TutorialBodyScroll\"", uiManager);
        Assert.Contains("HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled", uiManager);
        Assert.Contains("StartGuidedTutorial();", uiManager);
        Assert.Contains("ShowTutorialForNewGame();", uiManager);
        Assert.Contains("RefreshModalOverlayVisibility();", uiManager);
    }

    [Fact]
    public void LoopManagerSelfWiresSiblingTickManager()
    {
        string projectRoot = FindRepoRoot();
        string loopManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Loop", "LoopManager.cs"));

        Assert.Contains("TickManager ??= GetNodeOrNull<TickManager>(\"../TickManager\");", loopManager);
        Assert.Contains("LoopManager: TickManager was not assigned and could not be found.", loopManager);
    }

    [Fact]
    public void SaveSlotMenuUsesRepoSavesAndSafeRomanianFailureFeedback()
    {
        string projectRoot = FindRepoRoot();
        string uiManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "UIManager.cs"));
        string localization = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Localization", "ro.csv"));

        Assert.Contains("ProjectSettings.GlobalizePath(\"res://\")", uiManager);
        Assert.Contains("Directory.GetParent(normalizedProjectFolder)", uiManager);
        Assert.Contains("Directory.CreateDirectory(saveFolder)", uiManager);
        Assert.Contains("slot_{slot}.json", uiManager);
        Assert.Contains("IsSaveSlotLoadable", uiManager);
        Assert.Contains("persistence.Validate(save)", uiManager);
        Assert.Contains("Salvare ilizibilă", uiManager);
        Assert.Contains("Nu există salvări.", uiManager);
        Assert.Contains("NOTIFY_LOAD_FAILED", uiManager);
        Assert.Contains("NOTIFY_LOAD_FAILED,Încărcarea a eșuat", localization);
        Assert.DoesNotContain("FileDialog", uiManager);
    }

    [Fact]
    public void StoreVisualControllersKeepPathingEmployeeAndFurnitureContracts()
    {
        string projectRoot = FindRepoRoot();
        string layoutManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "Store", "StoreLayoutManager.cs"));
        string customerController = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "Store", "CustomerVisualController.cs"));
        string employeeController = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "Store", "EmployeeVisualController.cs"));
        string furnitureController = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "Store", "StoreFurnitureVisualController.cs"));

        Assert.Contains("GetSpawnPoint", customerController);
        Assert.Contains("GetBrowsePoint", customerController);
        Assert.Contains("GetQueuePoint", customerController);
        Assert.Contains("GetRegisterServicePoint", customerController);
        Assert.Contains("BuildPath(actor.Position, finalTarget, _game)", customerController);
        Assert.Contains("_customerFocusPoints.Clear()", customerController);
        Assert.DoesNotContain("OrderBy(candidate => candidate.QueueSequence)", customerController);
        Assert.DoesNotContain("Position = finalTarget", customerController);

        Assert.Contains("NameLabel", employeeController);
        Assert.Contains("RoleLabel", employeeController);
        Assert.Contains("RoleToRomanian", employeeController);
        Assert.Contains("Position = new Vector2(45f, 0f)", employeeController);
        Assert.Contains("Position = new Vector2(6f, 22f)", employeeController);
        Assert.Contains("Position = new Vector2(6f, 34f)", employeeController);
        Assert.Contains("ShortenLabel(employee.Name, 14)", employeeController);
        Assert.Contains("BuildPath(actor.Position, finalTarget, _game)", employeeController);

        Assert.Contains("GetSolidShelfObstacles(GameManager? game)", layoutManager);
        Assert.Contains("GetAllShelfPlacements(game)", layoutManager);
        Assert.Contains("GetPurchasedShelfPlacements(game)", layoutManager);
        Assert.Contains("BuildDynamicShelfSlots", layoutManager);
        Assert.Contains("SegmentIntersectsRect", layoutManager);
        Assert.Contains("return new List<Vector2> { start };", layoutManager);
        Assert.Contains("CreateShelfVisual(key", furnitureController);
        Assert.Contains("GetPurchasedShelfPlacements(game)", furnitureController);
        Assert.Contains("CreateShelfVisualFromOriginal", furnitureController);
        Assert.Contains("CreateMissingAssetVisual", furnitureController);
        Assert.Contains("ASSET LIPSĂ: raft", furnitureController);
        Assert.Contains("CreateTemporaryFurnitureStyle", furnitureController);
    }

    [Fact]
    public void OnboardingObjectivesAppearInExpectedRomanianOrder()
    {
        Assert.Collection(OnboardingObjectiveCatalog.Objectives,
            objective => Assert.Equal(("stock_first_shelf", "Aprovizionează primul raft"), (objective.Id, objective.TitleRo)),
            objective => Assert.Equal(("set_first_price", "Setează prețul unui produs"), (objective.Id, objective.TitleRo)),
            objective => Assert.Equal(("serve_first_customer", "Servește primul client"), (objective.Id, objective.TitleRo)),
            objective => Assert.Equal(("buy_first_shelf", "Cumpără sau plasează un raft nou"), (objective.Id, objective.TitleRo)),
            objective => Assert.Equal(("hire_first_worker", "Angajează un lucrător"), (objective.Id, objective.TitleRo)),
            objective => Assert.Equal(("keep_reputation_60", "Menține reputația peste 60%"), (objective.Id, objective.TitleRo)),
            objective => Assert.Equal(("serve_five_customers", "Servește 5 clienți"), (objective.Id, objective.TitleRo)),
            objective => Assert.Equal(("finish_first_day", "Finalizează prima zi"), (objective.Id, objective.TitleRo)));
    }

    [Fact]
    public void RelaxedGracePeriodStartsActiveAndRampsCustomerPressure()
    {
        using var relaxed = new GameManager(10000, settings: new GameStartSettings("Relaxat", GameDifficulty.Relaxed, 14));
        using var normal = new GameManager(10000, settings: new GameStartSettings("Normal", GameDifficulty.Normal, 14));

        Assert.True(relaxed.IsEarlyGracePeriodActive);
        Assert.False(normal.IsEarlyGracePeriodActive);
        int firstWaveDemand = relaxed.CurrentSalesWaveDemandBasisPoints;
        Assert.True(firstWaveDemand < normal.CurrentSalesWaveDemandBasisPoints);
        Assert.Equal(2, relaxed.GetGuidedCustomerVisualCount(6));
        Assert.Equal(6, normal.GetGuidedCustomerVisualCount(6));

        Assert.True(relaxed.StartBusiness());
        for (int tick = 0; tick < 361; tick++)
            relaxed.TickBusiness();

        Assert.True(relaxed.IsEarlyGracePeriodActive);
        Assert.True(relaxed.CurrentSalesWaveDemandBasisPoints > firstWaveDemand);
        Assert.True(relaxed.CurrentSalesWaveDemandBasisPoints < normal.CurrentSalesWaveDemandBasisPoints);

        relaxed.SetDay(4);
        Assert.False(relaxed.IsEarlyGracePeriodActive);
        Assert.Equal(6, relaxed.GetGuidedCustomerVisualCount(6));
    }

    [Fact]
    public void OnboardingObjectiveCompletionFollowsGameState()
    {
        using var game = new GameManager(10000, settings: new GameStartSettings("Ghidat", GameDifficulty.Relaxed, 14));

        Assert.False(game.GetOnboardingObjectives().Single(item => item.Objective.Id == "stock_first_shelf").IsCompleted);
        Assert.False(game.GetOnboardingObjectives().Single(item => item.Objective.Id == "set_first_price").IsCompleted);
        Assert.False(game.GetOnboardingObjectives().Single(item => item.Objective.Id == "serve_first_customer").IsCompleted);

        Assert.True(game.RefillShelf(1, 10) > 0);
        Assert.True(game.GetOnboardingObjectives().Single(item => item.Objective.Id == "stock_first_shelf").IsCompleted);
        Assert.True(game.SetProductPrice(1, Money.FromUnits(46)));
        Assert.True(game.GetOnboardingObjectives().Single(item => item.Objective.Id == "set_first_price").IsCompleted);
        Assert.True(game.StartBusiness());
        game.TickBusiness();

        Assert.True(game.GetOnboardingObjectives().Single(item => item.Objective.Id == "serve_first_customer").IsCompleted);
        Assert.True(game.HireEmployee("Ana", EmployeeRole.SalesAssociate));
        Assert.True(game.GetOnboardingObjectives().Single(item => item.Objective.Id == "hire_first_worker").IsCompleted);
    }

    [Fact]
    public void FirstCustomerSaleProducesMoneyAndVisibleFeedback()
    {
        using var game = new GameManager(10000, settings: new GameStartSettings("Prima zi", GameDifficulty.Relaxed, 14));
        Money startingProjectedCash = game.Economy.ProjectedCash;

        Assert.True(game.RefillShelf(1, 10) > 0);
        Assert.True(game.StartBusiness());
        game.TickBusiness();

        Assert.True(game.CurrentCustomersServedToday > 0);
        Assert.True(game.Economy.ProjectedCash > startingProjectedCash);
        Assert.Contains("Casă:", game.LastCheckoutFeedbackRo);
        Assert.Contains("Venit +", game.LastCheckoutFeedbackRo);
    }

    [Fact]
    public void ReputationLossFeedbackIncludesCause()
    {
        using var game = new GameManager(10000, settings: new GameStartSettings("Feedback", GameDifficulty.Normal, 14));

        game.Customers.ApplyReputationEvent(game, -3, ReputationChangeSource.QueuePressure);

        Assert.Contains("Reputație -", game.LastReputationFeedbackRo);
        Assert.Contains("clienții au așteptat prea mult la casă", game.LastReputationFeedbackRo);
    }

    [Fact]
    public void ProductPriceValidationRejectsNegativeAndUnreasonablePrices()
    {
        using var game = new GameManager(10000, settings: new GameStartSettings("Prețuri", GameDifficulty.Relaxed, 14));
        var product = game.Inventory.GetProduct(1)!;
        Money originalPrice = product.SalePrice;
        Money otherProductPrice = game.Inventory.GetProduct(2)!.SalePrice;

        Assert.False(game.SetProductPrice(1, Money.FromUnits(-1)));
        Assert.False(game.SetProductPrice(1, GameManager.MaximumProductSalePrice + Money.FromUnits(1)));
        Assert.Equal(originalPrice, product.SalePrice);

        Assert.True(game.SetProductPrice(1, Money.FromUnits(50)));
        Assert.Equal(Money.FromUnits(50), product.SalePrice);
        Assert.Equal(otherProductPrice, game.Inventory.GetProduct(2)!.SalePrice);
    }

    [Fact]
    public void PriceMenuUsesSingleExplicitTargetAndRomanianFeedback()
    {
        string projectRoot = FindRepoRoot();
        string uiManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "UIManager.cs"));
        string uiRoot = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Scenes", "UIRoot.tscn"));
        string ro = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Localization", "ro.csv"));

        Assert.Contains("_activePriceProductId", uiManager);
        Assert.Contains("BeginPriceEdit", uiManager);
        Assert.Contains("ClearPriceEditTarget", uiManager);
        Assert.Contains("BeginPriceEdit(productId.Value, focusInput: false)", uiManager);
        Assert.Contains("CompleteTutorialStep(TutorialStep.SetPrice);\n\t\t\t\tHidePricesPopup();", uiManager);
        Assert.Contains("RuntimePriceTargetLabel", uiRoot);
        Assert.Contains("Preț actualizat pentru {0}.", ro);
        Assert.Contains("Nu există produs selectat.", ro);
        Assert.Contains("Introdu un preț valid.", ro);
        Assert.Contains("ui_cancel", uiManager);
    }

    [Fact]
    public void ManagementMenusStayOpenAndUseTaskBoxProgress()
    {
        string projectRoot = FindRepoRoot();
        string uiManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "UIManager.cs"));
        string uiRoot = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Scenes", "UIRoot.tscn"));

        Assert.DoesNotContain("RuntimeTaskChecklist", uiRoot);
        Assert.DoesNotContain("TaskChecklist.tscn", uiRoot);
        Assert.False(File.Exists(Path.Combine(projectRoot, "client.godot", "Assets", "UI", "components", "TaskChecklist.tscn")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "client.godot", "Assets", "UI", "components", "TaskChecklistItem.tscn")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "client.godot", "Assets", "UI", "scripts", "task_checklist.gd")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "client.godot", "Assets", "UI", "scripts", "task_checklist_item.gd")));
        Assert.Contains("BuildTaskBoxText", uiManager);
        Assert.Contains("BuildOpeningTaskList", uiManager);
        Assert.Contains("GetTaskForObjective", uiManager);
        Assert.Contains("RefreshMenuProgressIndicators", uiManager);
        Assert.Contains("Setează prețurile", uiManager);
        Assert.Contains("Plasează o comandă", uiManager);
        Assert.Contains("Aprovizionează rafturile", uiManager);
        Assert.Contains("Verifică personalul", uiManager);
        Assert.Contains("Verifică rapoartele", uiManager);
        Assert.Contains("_reportsViewed = true;", uiManager);
        Assert.Contains("_staffChanged = true;", uiManager);
        Assert.Contains("ShowOpenShopWarning(warnings);", uiManager);
        Assert.DoesNotContain("HideEventPopup();\n\t\t\t}", uiManager);
    }

    [Fact]
    public void HotbarStatsAndWarmPixelThemeStayConsistent()
    {
        string projectRoot = FindRepoRoot();
        string uiManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "UIManager.cs"));
        string uiRoot = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Scenes", "UIRoot.tscn"));
        string theme = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Themes", "ChronoTheme.tres"));

        Assert.Contains("RuntimeHudLabel", uiRoot);
        Assert.Contains("Lei {_game.Economy.Cash}", uiManager);
        Assert.Contains("Rep {_game.Customers.Reputation}", uiManager);
        Assert.Contains("Stoc {_game.Inventory.TotalStorageUnits}/{_game.Inventory.StorageCapacity}", uiManager);
        Assert.Contains("_shop2DStatusLabel.Visible = false;", uiManager);
        Assert.Contains("0.52f, 0.88f", uiManager);
        Assert.Contains("custom_minimum_size = Vector2(260, 100)", uiRoot);
        Assert.Contains("bg_color = Color(0.54, 0.27, 0.1, 1)", theme);
        Assert.Contains("Chrono/colors/success = Color(0.84, 0.46, 0.18, 1)", theme);
        Assert.Contains("corner_radius_top_left = 1", theme);
        Assert.Contains("OptionButton/styles/normal = SubResource(\"StyleBoxFlat_button_normal\")", theme);
    }

    [Fact]
    public void UiCleanupKeepsMenusCompactClickableAndCalm()
    {
        string projectRoot = FindRepoRoot();
        string uiManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "UIManager.cs"));
        string uiRoot = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Scenes", "UIRoot.tscn"));
        string newGame = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Scenes", "NewGameSetupPanel.tscn"));
        string theme = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Themes", "ChronoTheme.tres"));
        string sceneText = uiRoot + "\n" + newGame;

        Assert.Contains("ConfigureButtonSize", uiManager);
        Assert.Contains("OrdersPopupSize = new(460f, 520f)", uiManager);
        Assert.Contains("PricesPopupSize = new(390f, 300f)", uiManager);
        Assert.Contains("StaffPopupSize = new(440f, 480f)", uiManager);
        Assert.Contains("ReportPopupSize = new(540f, 540f)", uiManager);
        Assert.Contains("StartupOptionsPopupSize = new(320f, 220f)", uiManager);
        Assert.Contains("Math.Max(300f, preferredSize.X)", uiManager);
        Assert.Contains("Magazin retro în buclă de timp", uiRoot);
        Assert.Contains("custom_minimum_size = Vector2(260, 52)", uiRoot);
        Assert.Contains("[node name=\"MarginContainer\" type=\"MarginContainer\" parent=\".\"", uiRoot);
        Assert.Contains("[node name=\"RootLayout\" type=\"VBoxContainer\" parent=\"MarginContainer\"", uiRoot);
        Assert.Contains("[node name=\"BodyLayout\" type=\"HBoxContainer\" parent=\"MarginContainer/RootLayout\"", uiRoot);
        Assert.Contains("[node name=\"TopMenu\" type=\"PanelContainer\" parent=\"MarginContainer/RootLayout\"", uiRoot);
        Assert.Contains("[node name=\"LeftMenu\" type=\"PanelContainer\" parent=\"MarginContainer/RootLayout/BodyLayout\"", uiRoot);
        Assert.Contains("[node name=\"MainGameArea\" type=\"HBoxContainer\" parent=\"MarginContainer/RootLayout/BodyLayout\"", uiRoot);
        Assert.Contains("custom_minimum_size = Vector2(0, 64)", uiRoot);
        Assert.DoesNotContain("clip_text = true", sceneText);
        Assert.DoesNotContain("offset_bottom = 18.767822", uiRoot);
        Assert.DoesNotContain("theme_override_font_sizes/font_size = 20", sceneText);
        Assert.DoesNotContain("RuntimeTaskChecklist", uiRoot);
        Assert.DoesNotContain("TaskChecklist.tscn", uiRoot);
        Assert.Contains("RuntimeTaskBoxLabel", uiRoot);
        Assert.Contains("content_margin_left = 14.0", theme);
        Assert.Contains("content_margin_top = 9.0", theme);
        Assert.Contains("Button/colors/font_hover_color = Color(1, 0.96, 0.86, 1)", theme);
        Assert.DoesNotContain("Button/colors/font_hover_color = Color(1, 1, 1, 1)", theme);
        Assert.Contains("OptionButton/styles/hover = SubResource(\"StyleBoxFlat_button_hover\")", theme);
        Assert.Contains("RunDebugOverlapCheck", uiManager);
        Assert.Contains("UI layout overlap:", uiManager);
        Assert.Contains("UI popup bounds:", uiManager);
        Assert.DoesNotContain("GameplayRoot", uiRoot + uiManager);
        Assert.DoesNotContain("AppVBox", uiRoot + uiManager);
        Assert.DoesNotContain("BodyHBox", uiRoot + uiManager);
    }

    [Fact]
    public void RuntimeUiRefreshIsThrottledDuringBusinessTicks()
    {
        string projectRoot = FindRepoRoot();
        string tickManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Systems", "TickManager.cs"));
        string uiManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "UIManager.cs"));

        Assert.Contains("BusinessUiRefreshInterval", tickManager);
        Assert.Contains("UIManager?.Refresh(forceFullUiRefresh)", tickManager);
        Assert.Contains("public void Refresh(bool forceFullRefresh = true)", uiManager);
        Assert.Contains("RefreshRuntimePanel(bool fullRefresh)", uiManager);
        Assert.Contains("RefreshRuntimeHudOnly", uiManager);
        Assert.Contains("BuildFullRefreshSignature", uiManager);
    }

    [Fact]
    public void LayoutChangesInvalidateCachedPathsAndNavigation()
    {
        string projectRoot = FindRepoRoot();
        string uiManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "UIManager.cs"));
        string layoutManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "Store", "StoreLayoutManager.cs"));
        string customerController = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "Store", "CustomerVisualController.cs"));
        string employeeController = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "Store", "EmployeeVisualController.cs"));

        Assert.Contains("InvalidateShopNavigation", uiManager);
        Assert.Contains("_storeLayoutManager?.InvalidateNavigationCache();", uiManager);
        Assert.Contains("_customerVisualController?.InvalidatePaths();", uiManager);
        Assert.Contains("_employeeVisualController?.InvalidatePaths();", uiManager);
        Assert.Contains("public void InvalidateNavigationCache()", layoutManager);
        Assert.Contains("BuildObstacleSignature", layoutManager);
        Assert.Contains("public void InvalidatePaths()", customerController);
        Assert.Contains("public void InvalidatePaths()", employeeController);
    }

    [Fact]
    public void RuntimeFlowAvoidsNoisyDebugPrintsAndClearsStaleTargetsOnLoad()
    {
        string projectRoot = FindRepoRoot();
        string uiManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "UIManager.cs"));

        Assert.Contains("HideAllPopups();\n\t\t\t\tClearPriceEditTarget();\n\t\t\t\tInvalidateShopNavigation();", uiManager);
        Assert.Contains("_runtimeTriggerEventButton.Visible = EnableLayoutDebug;", uiManager);
        Assert.Contains("if (_game == null || !EnableLayoutDebug)", uiManager);
        Assert.DoesNotContain("GD.Print(\"UIManager: Open Shop pressed.", uiManager);
        Assert.DoesNotContain("GD.Print(\"UIManager: Apply Price pressed.", uiManager);
        Assert.DoesNotContain("GD.Print(\"UIManager: Place Order pressed.", uiManager);
    }

    [Fact]
    public void RelaxedGraceSoftensPayrollAndQueuePenaltiesOnlyForRelaxed()
    {
        using var relaxed = new GameManager(10000, settings: new GameStartSettings("Relaxat", GameDifficulty.Relaxed, 14));
        using var normal = new GameManager(10000, settings: new GameStartSettings("Normal", GameDifficulty.Normal, 14));
        Money payroll = Money.FromUnits(100);

        Assert.Equal(Money.FromUnits(50), relaxed.GetAdjustedPayrollCost(payroll));
        Assert.Equal(payroll, normal.GetAdjustedPayrollCost(payroll));
        Assert.True(relaxed.GetAdjustedQueuePressureMitigationBasisPoints(10_000) < normal.GetAdjustedQueuePressureMitigationBasisPoints(10_000));
    }

    [Fact]
    public void RelaxedEarlyReputationLossIsMuchSlowerThanNormal()
    {
        using var relaxed = new GameManager(10000, settings: new GameStartSettings("Relaxat", GameDifficulty.Relaxed, 14));
        using var normal = new GameManager(10000, settings: new GameStartSettings("Normal", GameDifficulty.Normal, 14));
        int relaxedStart = relaxed.Customers.Reputation;
        int normalStart = normal.Customers.Reputation;

        for (int i = 0; i < 5; i++)
        {
            relaxed.Customers.ApplyReputationEvent(relaxed, -2, ReputationChangeSource.Stockout);
            normal.Customers.ApplyReputationEvent(normal, -2, ReputationChangeSource.Stockout);
        }

        Assert.True(relaxedStart - relaxed.Customers.Reputation < normalStart - normal.Customers.Reputation);
        Assert.True(relaxed.Customers.Reputation >= 60);
    }

    [Fact]
    public void RelaxedEarlyReputationFloorPreventsImmediateCollapse()
    {
        using var relaxed = new GameManager(10000, settings: new GameStartSettings("Relaxat", GameDifficulty.Relaxed, 14));
        using var normal = new GameManager(10000, settings: new GameStartSettings("Normal", GameDifficulty.Normal, 14));

        relaxed.Customers.SetState(61, 60, 1);
        normal.Customers.SetState(61, 60, 1);

        relaxed.Customers.ApplyReputationEvent(relaxed, -50, ReputationChangeSource.Stockout);
        normal.Customers.ApplyReputationEvent(normal, -50, ReputationChangeSource.Stockout);

        Assert.Equal(60, relaxed.Customers.Reputation);
        Assert.True(normal.Customers.Reputation < relaxed.Customers.Reputation);

        relaxed.SetDay(3);
        relaxed.Customers.ApplyReputationEvent(relaxed, -50, ReputationChangeSource.QueuePressure);
        Assert.True(relaxed.Customers.Reputation >= 54);
    }

    [Fact]
    public void RelaxedEarlyRecoverySupportPreventsStarterCashTrap()
    {
        using var relaxed = new GameManager(10000, settings: new GameStartSettings("Relaxat", GameDifficulty.Relaxed, 14));

        relaxed.Economy.Cash = Money.FromUnits(100);
        relaxed.SetDay(2);
        relaxed.StartMorning();

        Assert.Contains("Sprijin de început", relaxed.LastSupportEventEffect);
        Assert.True(relaxed.Economy.ProjectedCash >= Money.FromUnits(250));
    }

    [Fact]
    public void TutorialGuidanceUsesShortRomanianFirstRunInstructions()
    {
        string projectRoot = FindRepoRoot();
        string uiManager = File.ReadAllText(Path.Combine(projectRoot, "client.godot", "Visuals", "UIManager.cs"));

        Assert.Contains("Bun venit! Primul pas: aprovizionează un raft.", uiManager);
        Assert.Contains("Acum setează prețul unui produs.", uiManager);
        Assert.Contains("Acum deschide magazinul.", uiManager);
        Assert.Contains("Clienții intră prin ușă și caută produse pe rafturi.", uiManager);
        Assert.Contains("După cumpărături, clienții așteaptă la casă.", uiManager);
        Assert.Contains("Numerarul crește când vinzi", uiManager);
        Assert.Contains("Reputația scade dacă rafturile sunt goale", uiManager);
        Assert.Contains("Pas {(int)_tutorialStep + 1}/{TutorialStepCount}", uiManager);
    }

    private static IEnumerable<string> ExtractSceneButtonTexts(string sceneText)
    {
        foreach (Match block in Regex.Matches(sceneText, "\\[node name=\"[^\"]+\" type=\"Button\"[^\\]]*\\](?<body>.*?)(?=\\n\\[node |\\z)", RegexOptions.Singleline))
        {
            Match text = Regex.Match(block.Groups["body"].Value, "text = \"(?<text>[^\"]*)\"");
            if (text.Success)
                yield return text.Groups["text"].Value;
        }
    }

    private static string FindRepoRoot()
    {
        string current = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(current))
        {
            if (File.Exists(Path.Combine(current, "ChronoIndustrialist.sln")))
                return current;

            var parent = Directory.GetParent(current);
            if (parent == null)
                break;
            current = parent.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate ChronoIndustrialist.sln from test output directory.");
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}
