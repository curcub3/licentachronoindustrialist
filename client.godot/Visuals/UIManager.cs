using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using Godot;
using Core.Simulation.Data;
using Core.Simulation.Logic;
using Core.Simulation.Persistence;
using Client.Scripts.Systems;
using Client.Scripts.Visuals.Store;
using Client.Scripts.Visuals.UI;
using Client.Localization;

namespace Client.Scripts.Visuals
{
	public partial class UIManager : Control
	{
		private GameManager? _game;
		private TickManager? _tickManager;
		private Label? _runtimeHudLabel;
		private Label? _runtimeDailyRevenueLabel;
		private Label? _runtimeTimeOfDayLabel;
		private Label? _runtimeHintLabel;
		private Label? _runtimeTaskBoxLabel;
		private ProgressBar? _runtimeBusinessProgressBar;
		private Label? _runtimeSelectedShelfLabel;
		private Label? _runtimeSelectedEmployeeLabel;
		private Label? _runtimeSelectedCustomerLabel;
		private Label? _runtimeStockWarningsLabel;
		private Control? _storeHud;
		private Control? _startupMenu;
		private Control? _startupOptionsPopup;
		private Control? _newGameSetupPanel;
		private Button? _startupNewGameButton;
		private Button? _startupLoadFileButton;
		private Button? _startupOptionsButton;
		private Button? _startupExitButton;
		private Button? _startupCloseOptionsButton;
		private CheckBox? _startupTooltipsToggle;
		private LineEdit? _newGameStoreNameInput;
		private OptionButton? _newGameDifficultyPicker;
		private OptionButton? _newGameDurationPicker;
		private Label? _newGameValidationLabel;
		private Button? _newGameStartButton;
		private Button? _newGameBackButton;
		private PanelContainer? _saveSlotPopup;
		private Label? _saveSlotTitleLabel;
		private readonly Button?[] _saveSlotButtons = new Button?[3];
		private Button? _saveSlotCloseButton;
		private SaveSlotMenuMode _saveSlotMode = SaveSlotMenuMode.Load;
		private bool _saveSlotOpenedFromStartup;
		private Label? _runtimeReportLabel;
		private Label? _runtimeNotificationLabel;
		private Control? _notificationPanel;
		private Label? _runtimeAlertSummaryLabel;
		private Button? _runtimeOpenShopButton;
		private Button? _runtimeNextDayButton;
		private Button? _runtimeResetButton;
		private Button? _runtimeMainMenuButton;
		private Button? _runtimeSaveButton;
		private Button? _runtimeLoadButton;
		private Button? _runtimePauseSpeedButton;
		private Button? _runtimeNormalSpeedButton;
		private Button? _runtimeFastSpeedButton;
		private Button? _runtimeFasterSpeedButton;
		private Button? _runtimeManageTabButton;
		private Button? _runtimeStockTabButton;
		private Button? _runtimeStaffTabButton;
		private Button? _runtimeEventTabButton;
		private Button? _runtimeReportTabButton;
		private Button? _runtimeStatsTabButton;
		private Button? _runtimeCopyReportButton;
		private Control? _runtimeOrdersPopup;
		private Control? _runtimePricesPopup;
		private Control? _runtimeStaffPopup;
		private Control? _runtimeEventPopup;
		private Control? _runtimeReportPopup;
		private Button? _runtimeOpenOrdersPopupButton;
		private Button? _runtimeOpenPricesPopupButton;
		private Button? _runtimeOpenStaffPopupButton;
		private Button? _runtimeOpenEventPopupButton;
		private Button? _runtimeOpenReportPopupButton;
		private Button? _runtimeCloseOrdersPopupButton;
		private Button? _runtimeClosePricesPopupButton;
		private Button? _runtimeCloseStaffPopupButton;
		private Button? _runtimeCloseEventPopupButton;
		private Button? _runtimeCloseReportPopupButton;
		private OptionButton? _runtimePriceProductPicker;
		private Label? _runtimePriceTargetLabel;
		private LineEdit? _runtimePriceInput;
		private Button? _runtimeApplyPriceButton;
		private OptionButton? _runtimeOrderProductPicker;
		private OptionButton? _runtimeOrderSupplierPicker;
		private LineEdit? _runtimeOrderQuantityInput;
		private Button? _runtimePlaceOrderButton;
		private OptionButton? _runtimeShopCatalogPicker;
		private Button? _runtimeBuyShopCatalogButton;
		private Label? _runtimeShelfSummaryLabel;
		private OptionButton? _runtimeShelfPicker;
		private OptionButton? _runtimeShelfProductPicker;
		private Button? _runtimeAssignShelfButton;
		private LineEdit? _runtimeShelfRefillInput;
		private Button? _runtimeRefillShelfButton;
		private Button? _runtimeRefillAllShelvesButton;
		private Label? _runtimeStaffSummaryLabel;
		private OptionButton? _runtimeCandidatePicker;
		private Button? _runtimeHireCandidateButton;
		private OptionButton? _runtimeEmployeePicker;
		private Button? _runtimeFireEmployeeButton;
		private Label? _runtimeEventLabel;
		private Button? _runtimeEventOptionAButton;
		private Button? _runtimeEventOptionBButton;
		private Button? _runtimeTriggerEventButton;
		private Label? _shop2DStatusLabel;
		private Label? _shop2DShelfCartridgeLabel;
		private Label? _shop2DShelfConsoleLabel;
		private Label? _shop2DShelfCollectorLabel;
		private Label? _shop2DStorageLabel;
		private Label? _deliveryCountLabel;
		private Label? _shop2DRegisterLabel;
		private ColorRect? _shelfCartridgeZone;
		private ColorRect? _shelfConsoleZone;
		private ColorRect? _shelfCollectorZone;
		private ColorRect? _storageZone;
		private ColorRect? _deliveryZone;
		private ColorRect? _entranceZone;
		private ColorRect? _storageFillBar;
		private ColorRect? _registerZone;
		private ColorRect? _queueLane;
		private readonly ColorRect?[] _customerMarkers = new ColorRect?[4];
		private readonly ColorRect?[] _queueSpots = new ColorRect?[3];
		private ColorRect? _cashierMarker;
		private ColorRect? _stockerMarker;
		private ColorRect? _managerMarker;
		private ColorRect? _deliveryCrateMarker1;
		private ColorRect? _deliveryCrateMarker2;
		private Control? _runtimePriceSection;
		private Control? _runtimeStockSection;
		private Control? _runtimeStaffSection;
		private Control? _runtimeEventSection;
		private Control? _runtimeReportSection;
		private Control? _topHotbar;
		private Control? _shop2DView;
		private Control? _runtimeControlPanel;
		private Control? _rightContextPanel;
		private Label? _rightContextTitleLabel;
		private Label? _rightContextLabel;
		private Control? _gameplayRoot;
		private Control? _modalRoot;
		private StoreLayoutManager? _storeLayoutManager;
		private CustomerVisualController? _customerVisualController;
		private EmployeeVisualController? _employeeVisualController;
		private StoreFurnitureVisualController? _storeFurnitureVisualController;
		private bool _runtimeActionsWired;
		private bool _shopInteractionsWired;
		private bool _contextualHelpWired;
		private ContextualHelpTooltip? _contextualHelp;
		private DayPhase? _lastObservedPhase;
		private int _lastTaskResetDay = -1;
		private DailyReport? _lastCompletedReport;
		private DailyReport? _previousCompletedReport;
		private int _lastCompletedReportDay = -1;
		private bool _pricesChecked;
		private bool _stockChecked;
		private bool _ordersPlaced;
		private bool _staffChecked;
		private bool _staffChanged;
		private bool _eventsChecked;
		private bool _reportsViewed;
		private bool _showAdvancedReport;
		private readonly Dictionary<Control, ShopNodeLayout> _shopNodeLayouts = new();
		private PanelContainer? _openShopWarningPopup;
		private Label? _openShopWarningLabel;
		private Button? _openShopAnywayButton;
		private Button? _openShopBackButton;
		private PanelContainer? _tutorialPopup;
		private Label? _tutorialTitleLabel;
		private Label? _tutorialBodyLabel;
		private Label? _tutorialProgressLabel;
		private Button? _tutorialCloseButton;
		private Button? _tutorialDisableButton;
		private PanelContainer? _confirmationPopup;
		private Label? _confirmationTitleLabel;
		private Label? _confirmationBodyLabel;
		private Button? _confirmationCancelButton;
		private Button? _confirmationConfirmButton;
		private Action? _pendingConfirmationAction;
		private bool _tutorialSeenThisSession;
		private bool _tutorialEnabled = true;
		private TutorialStep _tutorialStep = TutorialStep.None;
		private bool _tutorialWaitingForAction;
		private int _lastReputationFeedbackSequence;
		private int _lastCheckoutFeedbackSequence;
		private int? _activePriceProductId;
		private bool _updatingPricePickerSelection;
		private string _lastFullRefreshSignature = "";
		private string _lastHudText = "";
		private string _lastHintText = "";
		private string _lastAlertSummaryText = "";
		private double _lastBusinessProgressValue = -1.0;
		private double _layoutDebugElapsed;
		[Export] public bool EnableLayoutDebug { get; set; } = false;

		[Export] public string SaveFolderName { get; set; } = "Saves";
		private const int VisibleProductCount = 7;
		private const int VisibleSupplierCount = 3;
		private const float ShopDesignWidth = 960f;
		private const float ShopDesignHeight = 620f;
		private const float LayoutDebugIntervalSeconds = 1.0f;
		private const int TutorialStepCount = 10;
		private static readonly Vector2 OrdersPopupSize = new(460f, 520f);
		private static readonly Vector2 PricesPopupSize = new(390f, 300f);
		private static readonly Vector2 StaffPopupSize = new(440f, 480f);
		private static readonly Vector2 EventPopupSize = new(420f, 330f);
		private static readonly Vector2 ReportPopupSize = new(540f, 540f);
		private static readonly Vector2 SaveSlotPopupSize = new(420f, 330f);
		private static readonly Vector2 TutorialPopupSize = new(460f, 280f);
		private static readonly Vector2 ConfirmationPopupSize = new(360f, 190f);
		private static readonly Vector2 StartupOptionsPopupSize = new(320f, 220f);
		private static readonly Color MenuChangedColor = new(0.84f, 0.46f, 0.18f, 1f);
		private static readonly Color MenuChangedBorderColor = new(1f, 0.74f, 0.38f, 1f);

		private enum SaveSlotMenuMode
		{
			Save,
			Load
		}

		private enum TutorialStep
		{
			None = -1,
			StockShelf = 0,
			SetPrice = 1,
			OpenStore = 2,
			ObserveCustomers = 3,
			CashierQueue = 4,
			MoneyEarned = 5,
			Reputation = 6,
			CurrentObjective = 7,
			ReviewReport = 8,
			Complete = 9
		}

		public override void _Ready()
		{
			Localizer.Initialize();
			AutoWireNodes();
			CallDeferred(nameof(FinalizeRuntimePanelSetup));
		}

		public override void _Notification(int what)
		{
			if (what == NotificationResized)
			{
				ApplyShopSceneScale();
				_storeLayoutManager?.RefreshLayout();
				RefreshVisiblePopupBounds();
			}
		}

		public override void _Process(double delta)
		{
			_customerVisualController?.Advance(delta);
			_employeeVisualController?.Advance(delta);
#if DEBUG
			if (EnableLayoutDebug)
			{
				_layoutDebugElapsed += delta;
				if (_layoutDebugElapsed >= LayoutDebugIntervalSeconds)
				{
					_layoutDebugElapsed = 0.0;
					RunDebugOverlapCheck();
				}
			}
#endif
		}

		public override void _UnhandledInput(InputEvent inputEvent)
		{
			if (inputEvent.IsActionPressed("ui_cancel") && CloseCurrentModalOrPanel())
				GetViewport().SetInputAsHandled();
		}

		private static string T(string key) => Localizer.Tr(key);
		private static string TF(string key, params object[] args) => Localizer.Format(key, args);
		private static string ProductName(Product product) => Localizer.ProductName(product);
		private static string ProductName(int productId) => Localizer.ProductName(productId);
		private static string PhaseName(DayPhase phase) => Localizer.Phase(phase);
		private static string MoodName(EconomicMood mood) => Localizer.Mood(mood);
		private static string EventName(GameEventType type) => Localizer.Event(type);
		private static string EventDescription(GameEvent? gameEvent) => gameEvent == null ? T("EVENT_NONE") : EventName(gameEvent.Type);
		private IEnumerable<Product> VisibleProducts() => _game?.Inventory.Products.OrderBy(product => product.Id).Take(VisibleProductCount) ?? Enumerable.Empty<Product>();
		private IEnumerable<SupplierProfile> VisibleSuppliers() => _game?.Suppliers.Suppliers.OrderBy(supplier => supplier.Id).Take(VisibleSupplierCount) ?? Enumerable.Empty<SupplierProfile>();

		public void Initialize(GameManager game)
		{
			_game = game;
			_tickManager ??= GetNodeOrNull<TickManager>("../../TickManager");
			CallDeferred(nameof(FinalizeRuntimePanelSetup));
		}

		public void FinalizeRuntimePanelSetup()
		{
			EnsureRuntimePanel();
			ApplyStaticRomanianText();
			ConfigureReadableControlSizing();
			ApplyShopSceneScale();
			PopulateRuntimeLists();
			WireRuntimeActions();
			Refresh();
			Notify(T("NOTIFY_RUNTIME_READY"));
		}

		private void ApplyStaticRomanianText()
		{
			if (FindNodeRecursive("StartupSubtitle", this) is Label startupSubtitle)
			{
				startupSubtitle.Visible = true;
				startupSubtitle.AutowrapMode = TextServer.AutowrapMode.WordSmart;
				startupSubtitle.Text = "Magazin retro în buclă de timp";
			}
			if (_startupNewGameButton != null)
				_startupNewGameButton.Text = "Joc Nou";
			if (_startupLoadFileButton != null)
				_startupLoadFileButton.Text = "Încarcă Joc";
			if (_startupOptionsButton != null)
				_startupOptionsButton.Text = "Setări";
			if (_startupExitButton != null)
				_startupExitButton.Text = "Ieșire";
			if (_startupCloseOptionsButton != null)
				_startupCloseOptionsButton.Text = "Înapoi";
			if (_startupTooltipsToggle != null)
				_startupTooltipsToggle.Text = "Informații la hover";
			if (_newGameStartButton != null)
				_newGameStartButton.Text = "Confirmă";
			if (_newGameBackButton != null)
				_newGameBackButton.Text = "Înapoi";
			if (_newGameStoreNameInput != null && string.IsNullOrWhiteSpace(_newGameStoreNameInput.Text))
				_newGameStoreNameInput.Text = "Magazin Retro";
			PopulateNewGameSetupOptions();
			if (_runtimeOpenShopButton != null)
				_runtimeOpenShopButton.Text = "Deschide";
			if (_runtimeNextDayButton != null)
				_runtimeNextDayButton.Text = "Ziua urm.";
			if (_runtimeResetButton != null)
				_runtimeResetButton.Text = "Reset";
			if (_runtimeMainMenuButton != null)
				_runtimeMainMenuButton.Text = "Meniu";
			if (_runtimeSaveButton != null)
				_runtimeSaveButton.Text = "Salv.";
			if (_runtimeLoadButton != null)
				_runtimeLoadButton.Text = "Încarcă";
			SetButtonText(_runtimePauseSpeedButton, "BTN_PAUSE");
			SetButtonText(_runtimeManageTabButton, "BTN_PRICES");
			SetButtonText(_runtimeStockTabButton, "BTN_ORDERS");
			SetButtonText(_runtimeStaffTabButton, "BTN_STAFF");
			SetButtonText(_runtimeEventTabButton, "BTN_EVENT");
			SetButtonText(_runtimeReportTabButton, "BTN_REPORT");
			if (_runtimeStatsTabButton != null)
				_runtimeStatsTabButton.Text = "Detalii";
			SetButtonText(_runtimeApplyPriceButton, "BTN_APPLY_PRICE");
			SetButtonText(_runtimePlaceOrderButton, "BTN_PLACE_ORDER");
			SetButtonText(_runtimeBuyShopCatalogButton, "BTN_BUY_SHOP_ITEM");
			SetButtonText(_runtimeAssignShelfButton, "BTN_ASSIGN_SHELF");
			SetButtonText(_runtimeRefillShelfButton, "BTN_REFILL");
			SetButtonText(_runtimeRefillAllShelvesButton, "BTN_REFILL_ALL");
			SetButtonText(_runtimeHireCandidateButton, "BTN_HIRE");
			SetButtonText(_runtimeFireEmployeeButton, "BTN_FIRE");
			SetButtonText(_runtimeCopyReportButton, "BTN_COPY_REPORT");
			SetButtonText(_runtimeCloseOrdersPopupButton, "BTN_CLOSE");
			SetButtonText(_runtimeClosePricesPopupButton, "BTN_CLOSE");
			SetButtonText(_runtimeCloseStaffPopupButton, "BTN_CLOSE");
			SetButtonText(_runtimeCloseEventPopupButton, "BTN_CLOSE");
			SetButtonText(_runtimeCloseReportPopupButton, "BTN_CLOSE");
		}

		private static void SetButtonText(Button? button, string key)
		{
			if (button != null)
				button.Text = T(key);
		}

		private void PopulateNewGameSetupOptions()
		{
			if (_newGameDifficultyPicker != null)
			{
				int selectedDifficulty = _newGameDifficultyPicker.GetItemCount() > 0 ? _newGameDifficultyPicker.Selected : 0;
				_newGameDifficultyPicker.Clear();
				_newGameDifficultyPicker.AddItem("Relaxat", (int)GameDifficulty.Relaxed);
				_newGameDifficultyPicker.AddItem("Normal", (int)GameDifficulty.Normal);
				_newGameDifficultyPicker.AddItem("Greu", (int)GameDifficulty.Hard);
				_newGameDifficultyPicker.Selected = Math.Clamp(selectedDifficulty, 0, _newGameDifficultyPicker.GetItemCount() - 1);
			}

			if (_newGameDurationPicker != null)
			{
				_newGameDurationPicker.Clear();
				_newGameDurationPicker.AddItem("14 zile", GameManager.CampaignDurationDays);
				_newGameDurationPicker.Selected = 0;
			}
		}

		private void ConfigureReadableControlSizing()
		{
			ConfigureFillControl(_newGameStoreNameInput);
			ConfigureFillControl(_newGameDifficultyPicker);
			ConfigureFillControl(_newGameDurationPicker);
			ConfigureFillControl(_runtimePriceProductPicker);
			ConfigureWrapLabel(_runtimePriceTargetLabel);
			ConfigureFillControl(_runtimeOrderProductPicker);
			ConfigureFillControl(_runtimeOrderSupplierPicker);
			ConfigureFillControl(_runtimeShopCatalogPicker);
			ConfigureFillControl(_runtimeShelfPicker);
			ConfigureFillControl(_runtimeShelfProductPicker);
			ConfigureFillControl(_runtimeCandidatePicker);
			ConfigureFillControl(_runtimeEmployeePicker);
			ConfigureFillControl(_runtimePriceInput);
			ConfigureFillControl(_runtimeOrderQuantityInput);
			ConfigureFillControl(_runtimeShelfRefillInput);

			ConfigureWrapLabel(_runtimeHudLabel);
			ConfigureWrapLabel(_runtimeNotificationLabel);
			ConfigureWrapLabel(_runtimeHintLabel);
			ConfigureWrapLabel(_runtimeTaskBoxLabel);
			ConfigureWrapLabel(_runtimeShelfSummaryLabel);
			ConfigureWrapLabel(_runtimeStaffSummaryLabel);
			ConfigureWrapLabel(_runtimeEventLabel);
			ConfigureWrapLabel(_runtimeReportLabel);
			ConfigureWrapLabel(_rightContextLabel);
			ConfigureWrapLabel(_newGameValidationLabel);

			ConfigureButtonSize(_startupNewGameButton, 52f, 260f);
			ConfigureButtonSize(_startupLoadFileButton, 52f, 260f);
			ConfigureButtonSize(_startupOptionsButton, 52f, 260f);
			ConfigureButtonSize(_startupExitButton, 52f, 260f);
			ConfigureButtonSize(_startupCloseOptionsButton);
			ConfigureButtonSize(_newGameStartButton);
			ConfigureButtonSize(_newGameBackButton);

			foreach (Button? button in new[]
			{
				_runtimeOpenShopButton, _runtimeNextDayButton, _runtimeResetButton,
				_runtimeMainMenuButton, _runtimeSaveButton, _runtimeLoadButton,
				_runtimePauseSpeedButton, _runtimeNormalSpeedButton, _runtimeFastSpeedButton,
				_runtimeFasterSpeedButton
			})
			{
				ConfigureButtonSize(button, 40f);
			}

			foreach (Button? button in new[]
			{
				_runtimeManageTabButton, _runtimeStockTabButton, _runtimeStaffTabButton,
				_runtimeEventTabButton, _runtimeReportTabButton, _runtimeStatsTabButton,
				_runtimeOpenOrdersPopupButton, _runtimeOpenPricesPopupButton,
				_runtimeOpenStaffPopupButton, _runtimeOpenEventPopupButton,
				_runtimeOpenReportPopupButton, _runtimeCloseOrdersPopupButton,
				_runtimeClosePricesPopupButton, _runtimeCloseStaffPopupButton,
				_runtimeCloseEventPopupButton, _runtimeCloseReportPopupButton,
				_runtimeApplyPriceButton, _runtimePlaceOrderButton,
				_runtimeBuyShopCatalogButton, _runtimeAssignShelfButton,
				_runtimeRefillShelfButton, _runtimeRefillAllShelvesButton,
				_runtimeHireCandidateButton, _runtimeFireEmployeeButton,
				_runtimeEventOptionAButton, _runtimeEventOptionBButton,
				_runtimeTriggerEventButton, _runtimeCopyReportButton
			})
			{
				ConfigureButtonSize(button);
			}
		}

		private static void ConfigureFillControl(Control? control)
		{
			if (control == null)
				return;

			control.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		}

		private static void ConfigureWrapLabel(Label? label)
		{
			if (label == null)
				return;

			label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
			label.ClipText = false;
		}

		private static void ConfigureButtonSize(Button? button, float minHeight = 42f, float minWidth = 0f)
		{
			if (button == null)
				return;

			button.CustomMinimumSize = new Vector2(
				Math.Max(button.CustomMinimumSize.X, minWidth),
				Math.Max(button.CustomMinimumSize.Y, minHeight));
			button.ClipText = true;
			button.FocusMode = FocusModeEnum.All;
			button.MouseDefaultCursorShape = CursorShape.PointingHand;
		}

		private void EnsureRuntimePanel()
		{
			var scenePanel = FindNodeRecursive("RuntimeControlPanel", this) ?? FindNodeRecursive("LeftMenu", this);
			if (scenePanel == null)
			{
				GD.PrintErr("UIManager: RuntimeControlPanel is missing from UIRoot.tscn. Open res://Scenes/UIRoot.tscn and restore the runtime layout shell.");
				return;
			}

			BindRuntimePanelFromScene(scenePanel);
			HideLegacyPanels();
		}

		private void BindRuntimePanelFromScene(Node panel)
		{
			_storeHud = FindNodeRecursive("StoreHUD", this) as Control;
			Node hudRoot = _storeHud ?? this;
			_runtimeHudLabel = FindNodeRecursive("RuntimeHudLabel", this) as Label;
			_runtimeDailyRevenueLabel = FindNodeRecursive("RuntimeDailyRevenueLabel", hudRoot) as Label;
			_runtimeTimeOfDayLabel = FindNodeRecursive("RuntimeTimeOfDayLabel", hudRoot) as Label;
			_runtimeHintLabel = FindNodeRecursive("RuntimeHintLabel", this) as Label;
			_runtimeTaskBoxLabel = FindNodeRecursive("RuntimeTaskBoxLabel", this) as Label;
			_runtimeBusinessProgressBar = FindNodeRecursive("RuntimeBusinessProgressBar", this) as ProgressBar;
			_runtimeSelectedShelfLabel = FindNodeRecursive("RuntimeSelectedShelfLabel", hudRoot) as Label;
			_runtimeSelectedEmployeeLabel = FindNodeRecursive("RuntimeSelectedEmployeeLabel", hudRoot) as Label;
			_runtimeSelectedCustomerLabel = FindNodeRecursive("RuntimeSelectedCustomerLabel", hudRoot) as Label;
			_runtimeStockWarningsLabel = FindNodeRecursive("RuntimeStockWarningsLabel", hudRoot) as Label;
			_topHotbar = FindNodeRecursive("TopBar", hudRoot) as Control ?? FindNodeRecursive("TopMenu", this) as Control;
			_shop2DView = FindNodeRecursive("Shop2DView", this) as Control;
			_runtimeControlPanel = (FindNodeRecursive("RuntimeControlPanel", hudRoot) ?? FindNodeRecursive("LeftMenu", this)) as Control;
			_rightContextPanel = FindNodeRecursive("RightContextPanel", this) as Control;
			_rightContextTitleLabel = FindNodeRecursive("RightContextTitle", this) as Label;
			_rightContextLabel = FindNodeRecursive("RightContextLabel", this) as Label;
			_gameplayRoot = GetNodeOrNull<Control>("MarginContainer");
			_modalRoot = FindNodeRecursive("ModalRoot", this) as Control;
			_startupMenu = FindNodeRecursive("StartupMenu", this) as Control;
			_startupOptionsPopup = FindNodeRecursive("StartupOptionsPopup", this) as Control;
			_newGameSetupPanel = FindNodeRecursive("NewGameSetupPanel", this) as Control;
			_startupNewGameButton = FindNodeRecursive("StartupNewGameButton", this) as Button;
			_startupLoadFileButton = FindNodeRecursive("StartupLoadFileButton", this) as Button;
			_startupOptionsButton = FindNodeRecursive("StartupOptionsButton", this) as Button;
			_startupExitButton = FindNodeRecursive("StartupExitButton", this) as Button;
			_startupCloseOptionsButton = FindNodeRecursive("StartupCloseOptionsButton", this) as Button;
			_startupTooltipsToggle = FindNodeRecursive("StartupTooltipsToggle", this) as CheckBox;
			_newGameStoreNameInput = FindNodeRecursive("NewGameStoreNameInput", this) as LineEdit;
			_newGameDifficultyPicker = FindNodeRecursive("NewGameDifficultyPicker", this) as OptionButton;
			_newGameDurationPicker = FindNodeRecursive("NewGameDurationPicker", this) as OptionButton;
			_newGameValidationLabel = FindNodeRecursive("NewGameValidationLabel", this) as Label;
			_newGameStartButton = FindNodeRecursive("NewGameStartButton", this) as Button;
			_newGameBackButton = FindNodeRecursive("NewGameBackButton", this) as Button;
			_runtimeReportLabel = FindNodeRecursive("RuntimeReportLabel", this) as Label;
			_runtimeNotificationLabel = FindNodeRecursive("RuntimeNotificationLabel", this) as Label;
			_notificationPanel = FindNodeRecursive("NotificationPanel", hudRoot) as Control;
			_runtimeAlertSummaryLabel = FindNodeRecursive("RuntimeAlertSummaryLabel", this) as Label;
			_runtimeOpenShopButton = FindNodeRecursive("RuntimeOpenShopButton", this) as Button;
			_runtimeNextDayButton = FindNodeRecursive("RuntimeNextDayButton", this) as Button;
			_runtimeResetButton = FindNodeRecursive("RuntimeResetButton", this) as Button;
			_runtimeMainMenuButton = FindNodeRecursive("RuntimeMainMenuButton", this) as Button;
			_runtimeSaveButton = FindNodeRecursive("RuntimeSaveButton", this) as Button;
			_runtimeLoadButton = FindNodeRecursive("RuntimeLoadButton", this) as Button;
			_runtimePauseSpeedButton = FindNodeRecursive("RuntimePauseSpeedButton", this) as Button;
			_runtimeNormalSpeedButton = FindNodeRecursive("RuntimeNormalSpeedButton", this) as Button;
			_runtimeFastSpeedButton = FindNodeRecursive("RuntimeFastSpeedButton", this) as Button;
			_runtimeFasterSpeedButton = FindNodeRecursive("RuntimeFasterSpeedButton", this) as Button;
			_runtimeManageTabButton = FindNodeRecursive("RuntimeManageTabButton", panel) as Button;
			_runtimeStockTabButton = FindNodeRecursive("RuntimeStockTabButton", panel) as Button;
			_runtimeStaffTabButton = FindNodeRecursive("RuntimeStaffTabButton", panel) as Button;
			_runtimeEventTabButton = FindNodeRecursive("RuntimeEventTabButton", panel) as Button;
			_runtimeReportTabButton = FindNodeRecursive("RuntimeReportTabButton", panel) as Button;
			_runtimeStatsTabButton = FindNodeRecursive("RuntimeStatsTabButton", panel) as Button;
			_runtimeCopyReportButton = FindNodeRecursive("RuntimeCopyReportButton", this) as Button;
			_runtimeOrdersPopup = FindNodeRecursive("RuntimeOrdersPopup", this) as Control;
			_runtimePricesPopup = FindNodeRecursive("RuntimePricesPopup", this) as Control;
			_runtimeStaffPopup = FindNodeRecursive("RuntimeStaffPopup", this) as Control;
			_runtimeEventPopup = FindNodeRecursive("RuntimeEventPopup", this) as Control;
			_runtimeReportPopup = FindNodeRecursive("RuntimeReportPopup", this) as Control;
			_runtimeOpenOrdersPopupButton = FindNodeRecursive("RuntimeOpenOrdersPopupButton", panel) as Button;
			_runtimeOpenPricesPopupButton = FindNodeRecursive("RuntimeOpenPricesPopupButton", panel) as Button;
			_runtimeOpenStaffPopupButton = FindNodeRecursive("RuntimeOpenStaffPopupButton", panel) as Button;
			_runtimeOpenEventPopupButton = FindNodeRecursive("RuntimeOpenEventPopupButton", panel) as Button;
			_runtimeOpenReportPopupButton = FindNodeRecursive("RuntimeOpenReportPopupButton", panel) as Button;
			_runtimeCloseOrdersPopupButton = FindNodeRecursive("RuntimeCloseOrdersPopupButton", this) as Button;
			_runtimeClosePricesPopupButton = FindNodeRecursive("RuntimeClosePricesPopupButton", this) as Button;
			_runtimeCloseStaffPopupButton = FindNodeRecursive("RuntimeCloseStaffPopupButton", this) as Button;
			_runtimeCloseEventPopupButton = FindNodeRecursive("RuntimeCloseEventPopupButton", this) as Button;
			_runtimeCloseReportPopupButton = FindNodeRecursive("RuntimeCloseReportPopupButton", this) as Button;
			_runtimePriceProductPicker = FindNodeRecursive("RuntimePriceProductPicker", this) as OptionButton;
			_runtimePriceTargetLabel = FindNodeRecursive("RuntimePriceTargetLabel", this) as Label;
			_runtimePriceInput = FindNodeRecursive("RuntimePriceInput", this) as LineEdit;
			_runtimeApplyPriceButton = FindNodeRecursive("RuntimeApplyPriceButton", this) as Button;
			_runtimeOrderProductPicker = FindNodeRecursive("RuntimeOrderProductPicker", this) as OptionButton;
			_runtimeOrderSupplierPicker = FindNodeRecursive("RuntimeOrderSupplierPicker", this) as OptionButton;
			_runtimeOrderQuantityInput = FindNodeRecursive("RuntimeOrderQuantityInput", this) as LineEdit;
			_runtimePlaceOrderButton = FindNodeRecursive("RuntimePlaceOrderButton", this) as Button;
			_runtimeShopCatalogPicker = FindNodeRecursive("RuntimeShopCatalogPicker", this) as OptionButton;
			_runtimeBuyShopCatalogButton = FindNodeRecursive("RuntimeBuyShopCatalogButton", this) as Button;
			_runtimeShelfSummaryLabel = FindNodeRecursive("RuntimeShelfSummaryLabel", this) as Label;
			_runtimeShelfPicker = FindNodeRecursive("RuntimeShelfPicker", this) as OptionButton;
			_runtimeShelfProductPicker = FindNodeRecursive("RuntimeShelfProductPicker", this) as OptionButton;
			_runtimeAssignShelfButton = FindNodeRecursive("RuntimeAssignShelfButton", this) as Button;
			_runtimeShelfRefillInput = FindNodeRecursive("RuntimeShelfRefillInput", this) as LineEdit;
			_runtimeRefillShelfButton = FindNodeRecursive("RuntimeRefillShelfButton", this) as Button;
			_runtimeRefillAllShelvesButton = FindNodeRecursive("RuntimeRefillAllShelvesButton", this) as Button;
			_runtimeStaffSummaryLabel = FindNodeRecursive("RuntimeStaffSummaryLabel", this) as Label;
			_runtimeCandidatePicker = FindNodeRecursive("RuntimeCandidatePicker", this) as OptionButton;
			_runtimeHireCandidateButton = FindNodeRecursive("RuntimeHireCandidateButton", this) as Button;
			_runtimeEmployeePicker = FindNodeRecursive("RuntimeEmployeePicker", this) as OptionButton;
			_runtimeFireEmployeeButton = FindNodeRecursive("RuntimeFireEmployeeButton", this) as Button;
			_runtimeEventLabel = FindNodeRecursive("RuntimeEventLabel", this) as Label;
			_runtimeEventOptionAButton = FindNodeRecursive("RuntimeEventOptionAButton", this) as Button;
			_runtimeEventOptionBButton = FindNodeRecursive("RuntimeEventOptionBButton", this) as Button;
			_runtimeTriggerEventButton = FindNodeRecursive("RuntimeTriggerEventButton", this) as Button;
			_runtimePriceSection = FindNodeRecursive("RuntimePriceSection", panel) as Control;
			_runtimeStockSection = FindNodeRecursive("RuntimeStockSection", panel) as Control;
			_runtimeStaffSection = FindNodeRecursive("RuntimeStaffSection", panel) as Control;
			_runtimeEventSection = FindNodeRecursive("RuntimeEventSection", panel) as Control;
			_runtimeReportSection = FindNodeRecursive("RuntimeReportSection", panel) as Control;
			BindShop2DView();
			EnsureSaveSlotPopup();
			EnsureFlowPopups();
			EnsureConfirmationPopup();
			EnsureContextualHelp();

			if (_runtimeHudLabel == null || _runtimeReportLabel == null || _runtimeNotificationLabel == null
				|| _runtimeOpenShopButton == null || _runtimeNextDayButton == null || _runtimeResetButton == null
				|| _runtimePriceProductPicker == null || _runtimePriceInput == null || _runtimeApplyPriceButton == null
				|| _runtimeOrderProductPicker == null || _runtimeOrderSupplierPicker == null
				|| _runtimeOrderQuantityInput == null || _runtimePlaceOrderButton == null)
			{
				GD.PushWarning("LeftMenu exists, but one or more expected child nodes were not found. Missing controls will be inactive until their node names are restored.");
			}

			SetStartupMode(_game == null);
		}

		private void HideLegacyPanels()
		{
			foreach (Node child in GetChildren())
			{
				if (IsManagedRuntimeUi(child.Name) || IsManagedStartupUi(child.Name))
					continue;

				if (child is CanvasItem item)
					item.Visible = false;
			}
		}

		private static bool IsManagedRuntimeUi(StringName name)
		{
			return name == "MarginContainer"
				|| name == "RootLayout"
				|| name == "BodyLayout"
				|| name == "ModalRoot"
				|| name == "TopMenu"
				|| name == "LeftMenu"
				|| name == "MainGameArea"
				|| name == "Shop2DView"
				|| name == "RuntimeControlPanel"
				|| name == "RuntimePricesPopup"
				|| name == "RuntimeStaffPopup"
				|| name == "RuntimeEventPopup"
				|| name == "RuntimeReportPopup"
				|| name == "RuntimeOrdersPopup"
				|| name == "SaveSlotPopup"
				|| name == "ContextualHelpTooltip"
				|| name == "ConfirmationPopup";
		}

		private static bool IsManagedStartupUi(StringName name)
		{
			return name == "StartupMenu"
				|| name == "NewGameSetupPanel";
		}

		private void WireRuntimeActions()
		{
			if (_runtimeActionsWired)
				return;

			if (_runtimeOpenShopButton != null)
			{
				_runtimeOpenShopButton.Disabled = false;
				_runtimeOpenShopButton.Pressed += OnRuntimeOpenShopPressed;
			}

			if (_runtimeNextDayButton != null)
			{
				_runtimeNextDayButton.Disabled = false;
				_runtimeNextDayButton.Pressed += OnRuntimeNextDayPressed;
			}

			if (_runtimeResetButton != null)
			{
				_runtimeResetButton.Disabled = false;
				_runtimeResetButton.Pressed += OnRuntimeResetPressed;
			}
			if (_runtimeMainMenuButton != null)
				_runtimeMainMenuButton.Pressed += OnRuntimeMainMenuPressed;

			if (_runtimeSaveButton != null)
				_runtimeSaveButton.Pressed += OnRuntimeSavePressed;
			if (_runtimeLoadButton != null)
				_runtimeLoadButton.Pressed += OnRuntimeLoadPressed;
			if (_startupNewGameButton != null)
				_startupNewGameButton.Pressed += OnStartupNewGamePressed;
			if (_startupLoadFileButton != null)
				_startupLoadFileButton.Pressed += OnStartupLoadFilePressed;
			if (_startupOptionsButton != null)
				_startupOptionsButton.Pressed += ShowStartupOptions;
			if (_startupExitButton != null)
				_startupExitButton.Pressed += OnStartupExitPressed;
			if (_startupCloseOptionsButton != null)
				_startupCloseOptionsButton.Pressed += HideStartupOptions;
			if (_startupTooltipsToggle != null)
				_startupTooltipsToggle.Toggled += enabled =>
				{
					_contextualHelp?.SetEnabled(enabled);
					Notify(enabled ? "Informațiile la hover sunt active." : "Informațiile la hover sunt dezactivate.");
				};
			if (_newGameStartButton != null)
				_newGameStartButton.Pressed += OnNewGameStartPressed;
			if (_newGameBackButton != null)
				_newGameBackButton.Pressed += ShowStartupMainMenu;
			if (_openShopAnywayButton != null)
				_openShopAnywayButton.Pressed += StartBusinessFromWarning;
			if (_openShopBackButton != null)
				_openShopBackButton.Pressed += HideOpenShopWarning;
			if (_tutorialCloseButton != null)
				_tutorialCloseButton.Pressed += HideTutorialPopup;
			if (_tutorialDisableButton != null)
				_tutorialDisableButton.Pressed += DisableTutorialPopups;
			if (_confirmationCancelButton != null)
				_confirmationCancelButton.Pressed += HideConfirmationPopup;
			if (_confirmationConfirmButton != null)
				_confirmationConfirmButton.Pressed += ConfirmPendingAction;
			if (_saveSlotCloseButton != null)
				_saveSlotCloseButton.Pressed += HideSaveSlotPopup;
			for (int i = 0; i < _saveSlotButtons.Length; i++)
			{
				int slot = i + 1;
				if (_saveSlotButtons[i] != null)
					_saveSlotButtons[i]!.Pressed += () => OnSaveSlotPressed(slot);
			}

			if (_runtimePauseSpeedButton != null)
				_runtimePauseSpeedButton.Pressed += () => SetRuntimeSpeed(0.0);
			if (_runtimeNormalSpeedButton != null)
				_runtimeNormalSpeedButton.Pressed += () => SetRuntimeSpeed(1.0);
			if (_runtimeFastSpeedButton != null)
				_runtimeFastSpeedButton.Pressed += () => SetRuntimeSpeed(2.0);
			if (_runtimeFasterSpeedButton != null)
				_runtimeFasterSpeedButton.Pressed += () => SetRuntimeSpeed(4.0);

			if (_runtimeManageTabButton != null)
				_runtimeManageTabButton.Pressed += () =>
				{
					ShowRuntimeSection(_runtimePriceSection);
					ShowPricesPopup();
				};
			if (_runtimeStockTabButton != null)
				_runtimeStockTabButton.Pressed += () =>
				{
					ShowRuntimeSection(_runtimeStockSection);
					ShowOrdersPopup();
				};
			if (_runtimeStaffTabButton != null)
				_runtimeStaffTabButton.Pressed += () =>
				{
					ShowRuntimeSection(_runtimeStaffSection);
					ShowStaffPopup();
				};
			if (_runtimeEventTabButton != null)
				_runtimeEventTabButton.Pressed += () =>
				{
					ShowRuntimeSection(_runtimeEventSection);
					ShowEventPopup();
				};
			if (_runtimeReportTabButton != null)
				_runtimeReportTabButton.Pressed += () =>
				{
					_showAdvancedReport = false;
					ShowRuntimeSection(_runtimeReportSection);
					ShowReportPopup();
				};
			if (_runtimeStatsTabButton != null)
				_runtimeStatsTabButton.Pressed += () =>
				{
					_showAdvancedReport = true;
					ShowRuntimeSection(_runtimeReportSection);
					ShowReportPopup();
					Notify("Detaliile avansate sunt afișate în raport.");
				};
			if (_runtimeCopyReportButton != null)
				_runtimeCopyReportButton.Pressed += OnRuntimeCopyReportPressed;
			if (_runtimeOpenOrdersPopupButton != null)
				_runtimeOpenOrdersPopupButton.Pressed += ShowOrdersPopup;
			if (_runtimeOpenPricesPopupButton != null)
				_runtimeOpenPricesPopupButton.Pressed += ShowPricesPopup;
			if (_runtimeOpenStaffPopupButton != null)
				_runtimeOpenStaffPopupButton.Pressed += ShowStaffPopup;
			if (_runtimeOpenEventPopupButton != null)
				_runtimeOpenEventPopupButton.Pressed += ShowEventPopup;
			if (_runtimeOpenReportPopupButton != null)
				_runtimeOpenReportPopupButton.Pressed += ShowReportPopup;
			if (_runtimeCloseOrdersPopupButton != null)
				_runtimeCloseOrdersPopupButton.Pressed += HideOrdersPopup;
			if (_runtimeClosePricesPopupButton != null)
				_runtimeClosePricesPopupButton.Pressed += HidePricesPopup;
			if (_runtimeCloseStaffPopupButton != null)
				_runtimeCloseStaffPopupButton.Pressed += HideStaffPopup;
			if (_runtimeCloseEventPopupButton != null)
				_runtimeCloseEventPopupButton.Pressed += HideEventPopup;
			if (_runtimeCloseReportPopupButton != null)
				_runtimeCloseReportPopupButton.Pressed += HideReportPopup;

			if (_runtimeApplyPriceButton != null)
			{
				_runtimeApplyPriceButton.Disabled = false;
				_runtimeApplyPriceButton.Pressed += OnRuntimeApplyPricePressed;
			}

			if (_runtimePriceProductPicker != null)
			{
				_runtimePriceProductPicker.ItemSelected += index =>
				{
					if (_updatingPricePickerSelection)
						return;

					BeginPriceEdit(GetProductIdAt(_runtimePriceProductPicker, (int)index), focusInput: true);
				};
			}

			if (_runtimePlaceOrderButton != null)
			{
				_runtimePlaceOrderButton.Disabled = false;
				_runtimePlaceOrderButton.Pressed += OnRuntimePlaceOrderPressed;
			}

			if (_runtimeBuyShopCatalogButton != null)
			{
				_runtimeBuyShopCatalogButton.Disabled = false;
				_runtimeBuyShopCatalogButton.Pressed += OnRuntimeBuyShopCatalogPressed;
			}

			if (_runtimeAssignShelfButton != null)
			{
				_runtimeAssignShelfButton.Disabled = false;
				_runtimeAssignShelfButton.Pressed += OnRuntimeAssignShelfPressed;
			}

			if (_runtimeRefillShelfButton != null)
			{
				_runtimeRefillShelfButton.Disabled = false;
				_runtimeRefillShelfButton.Pressed += OnRuntimeRefillShelfPressed;
			}

			if (_runtimeRefillAllShelvesButton != null)
			{
				_runtimeRefillAllShelvesButton.Disabled = false;
				_runtimeRefillAllShelvesButton.Pressed += OnRuntimeRefillAllShelvesPressed;
			}

			if (_runtimeHireCandidateButton != null)
			{
				_runtimeHireCandidateButton.Disabled = false;
				_runtimeHireCandidateButton.Pressed += OnRuntimeHireCandidatePressed;
			}

			if (_runtimeFireEmployeeButton != null)
			{
				_runtimeFireEmployeeButton.Disabled = false;
				_runtimeFireEmployeeButton.Pressed += OnRuntimeFireEmployeePressed;
			}

			if (_runtimeEventOptionAButton != null)
				_runtimeEventOptionAButton.Pressed += () => OnRuntimeResolveEventPressed(0);
			if (_runtimeEventOptionBButton != null)
				_runtimeEventOptionBButton.Pressed += () => OnRuntimeResolveEventPressed(1);
			if (_runtimeTriggerEventButton != null)
				_runtimeTriggerEventButton.Pressed += OnRuntimeTriggerEventPressed;

			WireShopInteractions();
			_runtimeActionsWired = true;
			ShowRuntimeSection(_runtimePriceSection);
		}

		private void EnsureContextualHelp()
		{
			_contextualHelp ??= new ContextualHelpTooltip(this);
			_contextualHelp.SetEnabled(_startupTooltipsToggle?.ButtonPressed ?? true);
			RegisterContextualHelpTargets();
		}

		private void RegisterContextualHelpTargets()
		{
			if (_contextualHelp == null || _contextualHelpWired)
				return;

			_contextualHelp.Register(
				_runtimeHudLabel,
				"Status",
				"Arată pe scurt ziua curentă, faza, banii, profitul și alertele importante.");
			_contextualHelp.Register(
				_runtimeTaskBoxLabel,
				"Pași de urmat",
				"Afișează recomandarea principală pentru faza curentă.");
			_contextualHelp.Register(
				_runtimeAlertSummaryLabel,
				"Alerte",
				"Rezumatul problemelor care necesită atenție imediată.");
			_contextualHelp.Register(
				_runtimeNotificationLabel,
				"Notificare",
				"Confirmările și mesajele de sistem apar aici.");
			_contextualHelp.Register(
				_runtimeReportLabel,
				"Raport",
				"Rezumatul zilei curente sau al ultimei zile încheiate, cu explicații și recomandări.");
			_contextualHelp.Register(
				_runtimeBusinessProgressBar,
				"Progres business",
				"Arată cât din ziua de vânzare a trecut.");
			_contextualHelp.Register(
				_runtimePriceProductPicker,
				"Produs",
				"Alege produsul al cărui preț vrei să îl modifici.");
			_contextualHelp.Register(
				_runtimePriceInput,
				"Preț nou",
				"Introdu prețul nou. Schimbarea se aplică doar în Administrare.");
			_contextualHelp.Register(
				_runtimeOrderProductPicker,
				"Produs de comandat",
				"Alege produsul care trebuie adăugat în stoc.");
			_contextualHelp.Register(
				_runtimeOrderSupplierPicker,
				"Furnizor",
				"Alege furnizorul și verifică timpul de livrare plus fiabilitatea. Fiabilitatea mare reduce riscul de întârziere.");
			_contextualHelp.Register(
				_runtimeOrderQuantityInput,
				"Cantitate",
				"Numărul de bucăți pe care vrei să le comanzi.");
			_contextualHelp.Register(
				_runtimeShelfPicker,
				"Raft",
				"Alege raftul care trebuie reumplut sau reasociat.");
			_contextualHelp.Register(
				_runtimeShelfProductPicker,
				"Produs pe raft",
				"Alege produsul care va fi pus pe raftul selectat.");
			_contextualHelp.Register(
				_runtimeShelfRefillInput,
				"Reumplere",
				"Numărul de bucăți pe care vrei să le muți pe raft.");
			_contextualHelp.Register(
				_runtimeShelfSummaryLabel,
				"Sănătatea stocului",
				"Arată rapid ce produse sunt bine aprovizionate, în avertizare sau critice. Best seller-ele și suprastocul sunt marcate separat.");
			_contextualHelp.Register(
				_runtimeShopCatalogPicker,
				"Îmbunătățire",
				"Alege o investiție pentru magazin: raft, depozit, decor sau hardware.");
			_contextualHelp.Register(
				_runtimeCandidatePicker,
				"Candidat",
				"Alege un candidat disponibil pentru angajare.");
			_contextualHelp.Register(
				_runtimeEmployeePicker,
				"Angajat",
				"Alege un angajat existent dacă trebuie concediat.");
			_contextualHelp.Register(
				_runtimeStaffSummaryLabel,
				"Situația personalului",
				"Vezi rolurile, salariile, moralul și performanța. Moralul mare crește eficiența; moralul scăzut poate încetini magazinul.");
			_contextualHelp.Register(
				_runtimeTriggerEventButton,
				"Generează eveniment",
				"Folosește acest buton doar pentru testare internă. În joc, evenimentele apar natural și schimbă riscul sau oportunitățile.");

			_contextualHelpWired = true;
		}

		private void ShowRuntimeSection(Control? section)
		{
			if (_runtimePriceSection != null)
				_runtimePriceSection.Visible = section == _runtimePriceSection;
			if (_runtimeStockSection != null)
				_runtimeStockSection.Visible = section == _runtimeStockSection;
			if (_runtimeStaffSection != null)
				_runtimeStaffSection.Visible = section == _runtimeStaffSection;
			if (_runtimeEventSection != null)
				_runtimeEventSection.Visible = section == _runtimeEventSection;
			if (_runtimeReportSection != null)
				_runtimeReportSection.Visible = section == _runtimeReportSection;
			HideAllPopups();
		}

		private void WireShopInteractions()
		{
			if (_shopInteractionsWired)
				return;

			WireShopZone(_shelfCartridgeZone, () => OpenWorldPrices(1), "Raft cartușe");
			WireShopZone(_shelfConsoleZone, () => OpenWorldPrices(2), "Raft console");
			WireShopZone(_shelfCollectorZone, () => OpenWorldPrices(3), "Raft colecție");
			WireShopZone(_storageZone, ShowOrdersPopup, "Depozit");
			WireShopZone(_deliveryZone, ShowOrdersPopup, "Livrare");
			WireShopZone(_registerZone, ShowStaffPopup, "Casă");
			WireShopZone(_entranceZone, () => Notify("Intrare: urmărește fluxul clienților în faza de vânzare."), "Intrare");
			_shopInteractionsWired = true;
		}

		private void WireShopZone(ColorRect? zone, Action action, string label)
		{
			if (zone == null)
				return;

			zone.MouseFilter = MouseFilterEnum.Stop;
			zone.MouseDefaultCursorShape = CursorShape.PointingHand;
			zone.TooltipText = label switch
			{
				"Depozit" => "Verifică stocul și comenzile",
				"Livrare" => "Vezi livrările și comenzile în așteptare",
				"Casă" => "Verifică coada și casierii",
				"Intrare" => "Fluxul clienților",
				_ => "Gestionează produsul și prețul"
			};
			zone.MouseEntered += () => zone.Modulate = new Color(1.08f, 0.9f, 0.9f, 1f);
			zone.MouseExited += () => zone.Modulate = Colors.White;
			zone.GuiInput += input =>
			{
				if (input is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left && mouse.Pressed)
				{
					SetContextPanel(label, zone.TooltipText);
					action();
					Notify($"{label}: meniu deschis.");
				}
			};
		}

		private void OpenWorldPrices(int productId)
		{
			ShowRuntimeSection(_runtimePriceSection);
			if (!SelectProductInPicker(_runtimePriceProductPicker, productId))
				Notify($"Produsul #{productId} nu este disponibil în lista de prețuri.");
			ShowPricesPopup(productId);
		}

		private void ShowOrdersPopup()
		{
			HideAllPopups();
			if (_runtimeOrdersPopup != null)
				ShowBoundedPopup(_runtimeOrdersPopup, OrdersPopupSize);
			else
				Notify("Meniul de comenzi lipsește din scenă.");
			UpdateTaskBoxDisplay();
		}

		private void ShowPricesPopup()
		{
			ShowPricesPopup(null);
		}

		private void ShowPricesPopup(int? requestedProductId)
		{
			int? targetProductId = requestedProductId ?? _activePriceProductId ?? GetSelectedProductId(_runtimePriceProductPicker);
			HideAllPopups();
			if (_runtimePricesPopup != null)
			{
				ShowBoundedPopup(_runtimePricesPopup, PricesPopupSize);
				BeginPriceEdit(targetProductId, focusInput: true);
			}
			else
				Notify("Meniul de prețuri lipsește din scenă.");
			UpdateTaskBoxDisplay();
		}

		private void ShowStaffPopup()
		{
			_staffChecked = true;
			HideAllPopups();
			if (_runtimeStaffPopup != null)
				ShowBoundedPopup(_runtimeStaffPopup, StaffPopupSize);
			else
				Notify("Meniul de personal lipsește din scenă.");
			UpdateTaskBoxDisplay();
		}

		private void ShowEventPopup()
		{
			HideAllPopups();
			if (_runtimeEventPopup != null)
				ShowBoundedPopup(_runtimeEventPopup, EventPopupSize);
			else
				Notify("Meniul de evenimente lipsește din scenă.");
			UpdateTaskBoxDisplay();
		}

		private void UpdateTaskBoxDisplay()
		{
			if (_runtimeTaskBoxLabel != null)
			{
				_runtimeTaskBoxLabel.Visible = true;
				_runtimeTaskBoxLabel.Text = BuildTaskBoxText();
			}
			RefreshMenuProgressIndicators();
		}

		private string BuildTaskBoxText()
		{
			if (_game == null)
				return "";

			var lines = new List<string> { "Sarcini rapide" };
			if (IsTutorialActive())
			{
				lines.Add(GetTutorialTaskSummary());
				return string.Join("\n", lines);
			}

			switch (_game.CurrentPhase)
			{
				case DayPhase.Management:
					var missing = BuildOpeningTaskList();
					if (missing.Count == 0)
						lines.Add("Gata de deschidere.");
					else
						lines.AddRange(missing);
					break;

				case DayPhase.Business:
					lines.Add($"Vânzare: urmărește coada {_game.CurrentQueueLength} și stocul.");
					if (HasLowShelfStock())
						lines.Add("Aprovizionează mâine");
					break;

				case DayPhase.Closing:
					lines.Add(_reportsViewed ? "Raport verificat." : "Verifică rapoartele");
					lines.Add("Salvează dacă vrei să păstrezi progresul");
					break;

				default:
					lines.Add("Verifică livrările");
					lines.Add("Pregătește administrarea");
					break;
			}

			return string.Join("\n", lines);
		}

		private List<string> BuildOpeningTaskList()
		{
			var tasks = new List<string>();
			if (_game == null)
				return tasks;

			var currentObjective = _game.GetCurrentOnboardingObjective();
			if (_game.CurrentDay <= 3 && currentObjective != null)
			{
				tasks.Add(GetTaskForObjective(currentObjective.Objective.Id));
				return tasks;
			}

			if (!_pricesChecked)
				tasks.Add("Setează prețurile");
			if (!_ordersPlaced)
				tasks.Add("Plasează o comandă");
			if (!_stockChecked || HasLowShelfStock())
				tasks.Add("Aprovizionează rafturile");
			if (!_staffChecked)
				tasks.Add("Verifică personalul");
			if (!_reportsViewed)
				tasks.Add("Verifică rapoartele");
			if (_game.CurrentDecision != null)
				tasks.Add("Rezolvă evenimentul");
			if (_game.Employees.CountRole(EmployeeRole.Cashier) <= 0)
				tasks.Add("Angajează/verifică un casier");

			return tasks.Distinct().Take(3).ToList();
		}

		private string GetTaskForObjective(string objectiveId)
		{
			return objectiveId switch
			{
				"stock_first_shelf" => "Aprovizionează rafturile",
				"serve_first_customer" => "Deschide magazinul",
				"finish_first_day" => "Citește raportul zilei",
				"set_first_price" => "Setează prețurile",
				"buy_first_shelf" => "Cumpără un raft nou",
				"hire_first_worker" => "Verifică personalul",
				"keep_reputation_60" => "Menține reputația peste 60%",
				"serve_five_customers" => "Servește 5 clienți",
				_ => "Urmărește obiectivul curent"
			};
		}

		private void RefreshMenuProgressIndicators()
		{
			SetMenuChangedState(_runtimeManageTabButton, _pricesChecked);
			SetMenuChangedState(_runtimeOpenPricesPopupButton, _pricesChecked);
			SetMenuChangedState(_runtimeStockTabButton, _ordersPlaced || _stockChecked);
			SetMenuChangedState(_runtimeOpenOrdersPopupButton, _ordersPlaced || _stockChecked);
			SetMenuChangedState(_runtimeStaffTabButton, _staffChanged);
			SetMenuChangedState(_runtimeOpenStaffPopupButton, _staffChanged);
			SetMenuChangedState(_runtimeEventTabButton, _eventsChecked);
			SetMenuChangedState(_runtimeOpenEventPopupButton, _eventsChecked);
			SetMenuChangedState(_runtimeReportTabButton, _reportsViewed);
			SetMenuChangedState(_runtimeStatsTabButton, _reportsViewed);
			SetMenuChangedState(_runtimeOpenReportPopupButton, _reportsViewed);
		}

		private static void SetMenuChangedState(Button? button, bool changed)
		{
			if (button == null)
				return;

			if (!changed)
			{
				button.RemoveThemeStyleboxOverride("normal");
				button.RemoveThemeStyleboxOverride("hover");
				button.RemoveThemeStyleboxOverride("pressed");
				button.RemoveThemeStyleboxOverride("focus");
				button.RemoveThemeColorOverride("font_color");
				button.RemoveThemeColorOverride("font_hover_color");
				button.RemoveThemeColorOverride("font_pressed_color");
				return;
			}

			var normal = CreateChangedButtonStyle(MenuChangedColor, MenuChangedBorderColor, 2);
			var hover = CreateChangedButtonStyle(new Color(0.92f, 0.54f, 0.22f, 1f), MenuChangedBorderColor, 2);
			var pressed = CreateChangedButtonStyle(new Color(0.62f, 0.31f, 0.12f, 1f), MenuChangedBorderColor, 2);
			button.AddThemeStyleboxOverride("normal", normal);
			button.AddThemeStyleboxOverride("hover", hover);
			button.AddThemeStyleboxOverride("focus", hover);
			button.AddThemeStyleboxOverride("pressed", pressed);
			button.AddThemeColorOverride("font_color", new Color(1f, 0.94f, 0.82f, 1f));
			button.AddThemeColorOverride("font_hover_color", new Color(1f, 0.98f, 0.9f, 1f));
			button.AddThemeColorOverride("font_pressed_color", new Color(1f, 0.9f, 0.72f, 1f));
		}

		private static StyleBoxFlat CreateChangedButtonStyle(Color background, Color border, int borderWidth)
		{
			var style = new StyleBoxFlat
			{
				BgColor = background,
				BorderColor = border,
				BorderWidthLeft = borderWidth,
				BorderWidthTop = borderWidth,
				BorderWidthRight = borderWidth,
				BorderWidthBottom = borderWidth,
				CornerRadiusTopLeft = 1,
				CornerRadiusTopRight = 1,
				CornerRadiusBottomRight = 1,
				CornerRadiusBottomLeft = 1,
				ContentMarginLeft = 12,
				ContentMarginTop = 7,
				ContentMarginRight = 12,
				ContentMarginBottom = 7
			};
			return style;
		}

		private string GetGuidedDayObjectiveTitle()
		{
			if (_game == null)
				return "Obiectiv ghidat";

			return _game.CurrentDay switch
			{
				1 => "Ziua 1: învață bucla",
				2 => "Ziua 2: controlează stocul",
				3 => "Ziua 3: optimizează echipa",
				_ => "Obiectiv ghidat"
			};
		}

		private string GetGuidedDayObjectiveSummary()
		{
			if (_game == null)
				return "";

			return _game.CurrentDay switch
			{
				1 => "Aprovizionează un raft, deschide magazinul și servește primul client.",
				2 => "Cumpără un raft sau comandă stoc dacă rafturile se golesc.",
				3 => "Verifică angajații și menține reputația peste 60%.",
				_ => ""
			};
		}

		private void ShowReportPopup()
		{
			HideAllPopups();
			if (_runtimeReportPopup != null)
				ShowBoundedPopup(_runtimeReportPopup, ReportPopupSize);
			else
				Notify("Meniul de raport lipsește din scenă.");
			_reportsViewed = true;
			if (_game != null && IsCompletedReport(GetDisplayReport()))
				CompleteTutorialStep(TutorialStep.ReviewReport);
			UpdateTaskBoxDisplay();
		}

		private void ShowBoundedPopup(Control popup, Vector2 preferredSize, bool useModalRoot = true, bool avoidTopMenu = true)
		{
			if (useModalRoot)
				PrepareModalPopupHost(popup);
			FitPopupToViewport(popup, preferredSize, avoidTopMenu);
			popup.Visible = true;
			popup.MoveToFront();
			RefreshModalOverlayVisibility();
		}

		private void PrepareModalPopupHost(Control popup)
		{
			if (_modalRoot == null)
				return;

			if (popup.GetParent() != _modalRoot)
				popup.Reparent(_modalRoot);

			_modalRoot.SetAnchorsPreset(LayoutPreset.FullRect);
			_modalRoot.Position = Vector2.Zero;
			_modalRoot.Size = GetViewportRect().Size;
			_modalRoot.Visible = true;
			_modalRoot.MoveToFront();
		}

		private void RefreshModalOverlayVisibility()
		{
			if (_modalRoot == null)
				return;

			bool hasVisiblePopup = false;
			foreach (Node child in _modalRoot.GetChildren())
			{
				if (child is Control control && control.Visible)
				{
					hasVisiblePopup = true;
					break;
				}
			}

			_modalRoot.Visible = hasVisiblePopup;
			if (!hasVisiblePopup)
				return;

			_modalRoot.SetAnchorsPreset(LayoutPreset.FullRect);
			_modalRoot.Position = Vector2.Zero;
			_modalRoot.Size = GetViewportRect().Size;
			_modalRoot.MoveToFront();
		}

		private void FitPopupToViewport(Control popup, Vector2 preferredSize, bool avoidTopMenu = true)
		{
			var viewportSize = GetViewportRect().Size;
			if (viewportSize.X <= 0 || viewportSize.Y <= 0)
				return;

			const float margin = 16f;
			float topInset = margin;
			if (avoidTopMenu && _topHotbar != null && _topHotbar.Visible)
			{
				var topRect = _topHotbar.GetGlobalRect();
				if (topRect.Size.Y > 0)
					topInset = Math.Clamp(topRect.End.Y + 12f, margin, Math.Max(margin, viewportSize.Y - margin));
			}

			var availableSize = new Vector2(
				Math.Max(240f, viewportSize.X - margin * 2f),
				Math.Max(140f, viewportSize.Y - topInset - margin));
			var contentMinimum = popup.GetCombinedMinimumSize();
			var desiredSize = new Vector2(
				Math.Max(300f, preferredSize.X),
				Math.Max(preferredSize.Y, contentMinimum.Y));
			var finalSize = new Vector2(
				Math.Min(desiredSize.X, availableSize.X),
				Math.Min(desiredSize.Y, availableSize.Y));
			var position = new Vector2(
				Math.Max(margin, (viewportSize.X - finalSize.X) * 0.5f),
				Math.Max(topInset, topInset + (availableSize.Y - finalSize.Y) * 0.5f));

			popup.SetAnchorsPreset(LayoutPreset.TopLeft);
			popup.Position = position;
			popup.Size = finalSize;
		}

		private void RefreshVisiblePopupBounds()
		{
			if (_modalRoot != null)
			{
				_modalRoot.SetAnchorsPreset(LayoutPreset.FullRect);
				_modalRoot.Position = Vector2.Zero;
				_modalRoot.Size = GetViewportRect().Size;
			}

			if (_runtimeOrdersPopup?.Visible == true)
				FitPopupToViewport(_runtimeOrdersPopup, OrdersPopupSize);
			if (_runtimePricesPopup?.Visible == true)
				FitPopupToViewport(_runtimePricesPopup, PricesPopupSize);
			if (_runtimeStaffPopup?.Visible == true)
				FitPopupToViewport(_runtimeStaffPopup, StaffPopupSize);
			if (_runtimeEventPopup?.Visible == true)
				FitPopupToViewport(_runtimeEventPopup, EventPopupSize);
			if (_runtimeReportPopup?.Visible == true)
				FitPopupToViewport(_runtimeReportPopup, ReportPopupSize);
			if (_saveSlotPopup?.Visible == true)
				FitPopupToViewport(_saveSlotPopup, SaveSlotPopupSize, _game != null);
			if (_openShopWarningPopup?.Visible == true)
				FitPopupToViewport(_openShopWarningPopup, new Vector2(430, 250));
			if (_tutorialPopup?.Visible == true)
				FitPopupToViewport(_tutorialPopup, TutorialPopupSize);
			if (_confirmationPopup?.Visible == true)
				FitPopupToViewport(_confirmationPopup, ConfirmationPopupSize, _game != null);
			if (_startupOptionsPopup?.Visible == true)
				FitPopupToViewport(_startupOptionsPopup, StartupOptionsPopupSize, avoidTopMenu: false);

			RefreshModalOverlayVisibility();
		}

#if DEBUG
		private void RunDebugOverlapCheck()
		{
			if (!EnableLayoutDebug)
				return;

			var viewport = new Rect2(Vector2.Zero, GetViewportRect().Size);
			var layoutControls = new List<Control>();
			AddDebugControl(layoutControls, _topHotbar);
			AddDebugControl(layoutControls, _runtimeControlPanel);
			AddDebugControl(layoutControls, FindNodeRecursive("MainGameArea", this) as Control);

			for (int i = 0; i < layoutControls.Count; i++)
			{
				Rect2 rect = layoutControls[i].GetGlobalRect();
				if (!RectContains(viewport, rect))
					GD.PushWarning($"UI layout bounds: {layoutControls[i].GetPath()} rect {FormatRect(rect)} is outside viewport {FormatRect(viewport)}.");

				for (int j = i + 1; j < layoutControls.Count; j++)
				{
					Rect2 otherRect = layoutControls[j].GetGlobalRect();
					Vector2 overlap = GetOverlapSize(rect, otherRect);
					if (overlap.X > 4f && overlap.Y > 4f)
					{
						GD.PushWarning(
							$"UI layout overlap: {layoutControls[i].GetPath()} {FormatRect(rect)} overlaps " +
							$"{layoutControls[j].GetPath()} {FormatRect(otherRect)} by {overlap.X:0.#}x{overlap.Y:0.#}.");
					}
				}
			}

			if (_modalRoot?.Visible == true)
			{
				foreach (Node child in _modalRoot.GetChildren())
				{
					if (child is not Control popup || !popup.Visible)
						continue;

					Rect2 rect = popup.GetGlobalRect();
					if (!RectContains(viewport, rect))
						GD.PushWarning($"UI popup bounds: {popup.GetPath()} rect {FormatRect(rect)} is outside viewport {FormatRect(viewport)}.");
				}
			}
		}

		private static void AddDebugControl(List<Control> controls, Control? control)
		{
			if (control is { Visible: true })
				controls.Add(control);
		}

		private static Vector2 GetOverlapSize(Rect2 a, Rect2 b)
		{
			float left = Math.Max(a.Position.X, b.Position.X);
			float top = Math.Max(a.Position.Y, b.Position.Y);
			float right = Math.Min(a.End.X, b.End.X);
			float bottom = Math.Min(a.End.Y, b.End.Y);
			return new Vector2(Math.Max(0f, right - left), Math.Max(0f, bottom - top));
		}

		private static bool RectContains(Rect2 outer, Rect2 inner)
		{
			const float tolerance = 1f;
			return inner.Position.X >= outer.Position.X - tolerance
				&& inner.Position.Y >= outer.Position.Y - tolerance
				&& inner.End.X <= outer.End.X + tolerance
				&& inner.End.Y <= outer.End.Y + tolerance;
		}

		private static string FormatRect(Rect2 rect)
		{
			return $"({rect.Position.X:0.#},{rect.Position.Y:0.#} {rect.Size.X:0.#}x{rect.Size.Y:0.#})";
		}
#endif

		private void HideOrdersPopup()
		{
			if (_runtimeOrdersPopup != null)
				_runtimeOrdersPopup.Visible = false;
			RefreshModalOverlayVisibility();
		}

		private void HidePricesPopup()
		{
			if (_runtimePricesPopup != null)
				_runtimePricesPopup.Visible = false;
			ClearPriceEditTarget();
			RefreshModalOverlayVisibility();
		}

		private void HideStaffPopup()
		{
			if (_runtimeStaffPopup != null)
				_runtimeStaffPopup.Visible = false;
			RefreshModalOverlayVisibility();
		}

		private void HideEventPopup()
		{
			if (_runtimeEventPopup != null)
				_runtimeEventPopup.Visible = false;
			RefreshModalOverlayVisibility();
		}

		private void HideReportPopup()
		{
			if (_runtimeReportPopup != null)
				_runtimeReportPopup.Visible = false;
			RefreshModalOverlayVisibility();
		}

		private void HideAllPopups()
		{
			HideOrdersPopup();
			HidePricesPopup();
			HideStaffPopup();
			HideEventPopup();
			HideReportPopup();
			HideSaveSlotPopup();
			HideOpenShopWarning();
			DismissTutorialPopup();
			HideConfirmationPopup();
			HideStartupOptions();
		}

		private bool CloseCurrentModalOrPanel()
		{
			if (_confirmationPopup?.Visible == true)
			{
				HideConfirmationPopup();
				return true;
			}
			if (_openShopWarningPopup?.Visible == true)
			{
				HideOpenShopWarning();
				return true;
			}
			if (_tutorialPopup?.Visible == true)
			{
				HideTutorialPopup();
				return true;
			}
			if (_saveSlotPopup?.Visible == true)
			{
				HideSaveSlotPopup();
				return true;
			}
			if (_runtimePricesPopup?.Visible == true)
			{
				HidePricesPopup();
				return true;
			}
			if (_runtimeOrdersPopup?.Visible == true)
			{
				HideOrdersPopup();
				return true;
			}
			if (_runtimeStaffPopup?.Visible == true)
			{
				HideStaffPopup();
				return true;
			}
			if (_runtimeEventPopup?.Visible == true)
			{
				HideEventPopup();
				return true;
			}
			if (_runtimeReportPopup?.Visible == true)
			{
				HideReportPopup();
				return true;
			}
			if (_startupOptionsPopup?.Visible == true)
			{
				HideStartupOptions();
				return true;
			}
			if (_newGameSetupPanel?.Visible == true)
			{
				ShowStartupMainMenu();
				return true;
			}

			return false;
		}

		private void DismissTutorialPopup()
		{
			if (_tutorialPopup != null)
				_tutorialPopup.Visible = false;
			RefreshModalOverlayVisibility();
		}

		private void HideOpenShopWarning()
		{
			if (_openShopWarningPopup != null)
				_openShopWarningPopup.Visible = false;
			RefreshModalOverlayVisibility();
		}

		private void HideTutorialPopup()
		{
			if (!IsTutorialActive())
			{
				if (_tutorialPopup != null)
					_tutorialPopup.Visible = false;
				RefreshModalOverlayVisibility();
				return;
			}

			if (_tutorialWaitingForAction)
			{
				if (_tutorialPopup != null)
					_tutorialPopup.Visible = false;
				RefreshModalOverlayVisibility();
				HighlightTutorialTarget();
				return;
			}

			AdvanceTutorialStep();
		}

		private bool IsTutorialActive()
		{
			return !_tutorialSeenThisSession && _tutorialStep != TutorialStep.None;
		}

		private void StartGuidedTutorial()
		{
			if (!_tutorialEnabled)
				return;

			_tutorialSeenThisSession = false;
			_tutorialStep = TutorialStep.StockShelf;
			_tutorialWaitingForAction = false;
			ShowTutorialStep();
		}

		private void AdvanceTutorialStep()
		{
			if (!IsTutorialActive())
				return;

			if (_tutorialStep == TutorialStep.Complete)
			{
				FinishTutorial();
				return;
			}

			_tutorialStep += 1;
			ShowTutorialStep();
		}

		private void CompleteTutorialStep(TutorialStep step)
		{
			if (!IsTutorialActive() || _tutorialStep != step)
				return;

			AdvanceTutorialStep();
		}

		private void FinishTutorial()
		{
			_tutorialSeenThisSession = true;
			_tutorialStep = TutorialStep.None;
			_tutorialWaitingForAction = false;
			DismissTutorialPopup();
			Notify("Tutorial complet. Acum poți juca liber și folosi panoul din stânga pentru următorii pași.");
			Refresh();
		}

		private void DisableTutorialPopups()
		{
			_tutorialEnabled = false;
			_tutorialSeenThisSession = true;
			_tutorialStep = TutorialStep.None;
			_tutorialWaitingForAction = false;
			DismissTutorialPopup();
			Notify("Tutorialul a fost dezactivat pentru această sesiune.");
			Refresh();
		}

		private void ShowTutorialStep()
		{
			EnsureFlowPopups();
			if (_tutorialPopup == null || _tutorialTitleLabel == null || _tutorialBodyLabel == null || _tutorialProgressLabel == null || _tutorialCloseButton == null)
				return;

			var (title, body, buttonText, waitsForAction) = GetTutorialStepContent(_tutorialStep);
			_tutorialWaitingForAction = waitsForAction;
			_tutorialTitleLabel.Text = title;
			_tutorialBodyLabel.Text = body;
			_tutorialProgressLabel.Text = _tutorialStep == TutorialStep.Complete
				? "Tutorial complet"
				: $"Pas {(int)_tutorialStep + 1}/{TutorialStepCount}";
			_tutorialCloseButton.Text = buttonText;
			ShowBoundedPopup(_tutorialPopup, TutorialPopupSize);

			if (_rightContextPanel != null)
				SetContextPanel("Tutorial", GetTutorialTaskSummary());

			HighlightTutorialTarget();
		}

		private (string Title, string Body, string ButtonText, bool WaitsForAction) GetTutorialStepContent(TutorialStep step)
		{
			return step switch
			{
					TutorialStep.StockShelf => (
						"Aprovizionează raftul",
						"Bun venit! Primul pas: aprovizionează un raft. Deschide Comenzi și apasă Realimentează.",
						"Arată-mi unde",
						true),
					TutorialStep.SetPrice => (
						"Setează prețul",
						"Acum setează prețul unui produs. Deschide Prețuri, verifică produsul afișat și confirmă un preț valid.",
						"Arată-mi unde",
						true),
					TutorialStep.OpenStore => (
						"Deschide",
						"Acum deschide magazinul. Clienții intră prin ușă și caută produse pe rafturi.",
						"Sunt gata",
						true),
				TutorialStep.ObserveCustomers => (
					"Urmărește clienții",
					"Clienții merg la rafturi înainte să cumpere. Primii clienți vin mai rar în modul Relaxat.",
					"Privesc",
					true),
				TutorialStep.CashierQueue => (
					"Casa și coada",
					"După cumpărături, clienții așteaptă la casă. Coada lungă poate scădea reputația.",
					"Am înțeles",
					false),
				TutorialStep.MoneyEarned => (
					"Banii vin din vânzări",
					"Numerarul crește când vinzi și scade din salarii, comenzi, chirie și taxe.",
					"Continuă",
					false),
				TutorialStep.Reputation => (
					"Reputația contează",
					"Reputația scade dacă rafturile sunt goale sau clienții așteaptă prea mult.",
					"Continuă",
					false),
				TutorialStep.CurrentObjective => (
					"Obiectiv curent",
					GetCurrentObjectiveText(),
					"Continuă",
					false),
				TutorialStep.ReviewReport => (
					"Citește raportul zilei",
					"Raportul explică banii, clienții serviți și reputația.",
					"Arată-mi unde",
					true),
				TutorialStep.Complete => (
					"Tutorial complet",
					"Bucla de bază: raft, stoc, clienți, casă, raport. Urmărește checklistul.",
					"Închide",
					false),
				_ => ("", "", "Continuă", false)
			};
		}

		private string GetTutorialTaskSummary()
		{
			return _tutorialStep switch
			{
					TutorialStep.StockShelf => "Mută produse din depozit pe raft.",
					TutorialStep.SetPrice => "Deschide Prețuri și confirmă prețul unui produs.",
					TutorialStep.OpenStore => "Apasă «Deschide».",
				TutorialStep.ObserveCustomers => "Urmărește clienții la rafturi.",
				TutorialStep.CashierQueue => "Urmărește coada de la casă.",
				TutorialStep.MoneyEarned => "Urmărește numerarul din HUD.",
				TutorialStep.Reputation => "Ține rafturile pline și coada scurtă.",
				TutorialStep.CurrentObjective => GetCurrentObjectiveText(),
				TutorialStep.ReviewReport => "Deschide raportul la finalul zilei.",
				TutorialStep.Complete => "Tutorial terminat. Continuă cu checklistul.",
				_ => "Urmează instrucțiunile tutorialului."
			};
		}

		private string GetCurrentObjectiveText()
		{
			var current = _game?.GetCurrentOnboardingObjective();
			if (current == null)
				return "Obiectiv: continuă să vinzi și citește raportul zilei.";

			return $"Obiectiv: {current.Objective.TitleRo}.";
		}

		private void HighlightTutorialTarget()
		{
			Control? target = _tutorialStep switch
				{
					TutorialStep.StockShelf => _runtimeStockTabButton,
					TutorialStep.SetPrice => _runtimeManageTabButton,
					TutorialStep.OpenStore => _runtimeOpenShopButton,
				TutorialStep.ObserveCustomers => _queueLane ?? _shop2DView,
				TutorialStep.CashierQueue => _queueLane ?? _registerZone,
				TutorialStep.MoneyEarned => _runtimeHudLabel,
				TutorialStep.CurrentObjective => _runtimeTaskBoxLabel,
				TutorialStep.ReviewReport => _runtimeReportTabButton,
				_ => _runtimeHintLabel
			};

			if (target == null)
				return;

			target.GrabFocus();
			_contextualHelp?.Refresh();
		}

		private void EnsureSaveSlotPopup()
		{
			if (_saveSlotPopup != null)
				return;

			_saveSlotPopup = new PanelContainer
			{
				Name = "SaveSlotPopup",
				Visible = false,
				CustomMinimumSize = SaveSlotPopupSize
			};
			(_modalRoot ?? this).AddChild(_saveSlotPopup);

			var margin = new MarginContainer { Name = "SaveSlotMargin" };
			margin.AddThemeConstantOverride("margin_left", 18);
			margin.AddThemeConstantOverride("margin_top", 16);
			margin.AddThemeConstantOverride("margin_right", 18);
			margin.AddThemeConstantOverride("margin_bottom", 16);
			_saveSlotPopup.AddChild(margin);

			var root = new VBoxContainer { Name = "SaveSlotRoot" };
			root.AddThemeConstantOverride("separation", 10);
			margin.AddChild(root);

			_saveSlotTitleLabel = new Label
			{
				Name = "SaveSlotTitleLabel",
				HorizontalAlignment = HorizontalAlignment.Center,
				Text = T("BTN_LOAD_GAME")
			};
			root.AddChild(_saveSlotTitleLabel);

			for (int i = 0; i < _saveSlotButtons.Length; i++)
			{
				var button = new Button
				{
					Name = $"SaveSlot{i + 1}Button",
					CustomMinimumSize = new Vector2(0, 64),
					AutowrapMode = TextServer.AutowrapMode.WordSmart,
					Text = $"{T("LABEL_SLOT")} {i + 1}\n{T("LABEL_EMPTY")}"
				};
				root.AddChild(button);
				_saveSlotButtons[i] = button;
			}

			_saveSlotCloseButton = new Button
			{
				Name = "SaveSlotCloseButton",
				Text = T("BTN_CLOSE")
			};
			root.AddChild(_saveSlotCloseButton);
		}

		private void ShowSaveSlotPopup(SaveSlotMenuMode mode, bool openedFromStartup)
		{
			EnsureSaveSlotPopup();
			_saveSlotMode = mode;
			_saveSlotOpenedFromStartup = openedFromStartup;
			if (_saveSlotTitleLabel != null)
				_saveSlotTitleLabel.Text = mode == SaveSlotMenuMode.Save ? T("BTN_SAVE_GAME") : T("BTN_LOAD_GAME");
			RefreshSaveSlotButtons();
			if (_saveSlotPopup != null)
				ShowBoundedPopup(_saveSlotPopup, SaveSlotPopupSize, avoidTopMenu: _game != null);
		}

		private void HideSaveSlotPopup()
		{
			if (_saveSlotPopup != null)
				_saveSlotPopup.Visible = false;
			RefreshModalOverlayVisibility();
		}

		private void EnsureFlowPopups()
		{
			if (_openShopWarningPopup != null && _tutorialPopup != null)
				return;

			Node popupParent = _modalRoot ?? this;
			if (_openShopWarningPopup == null)
			{
				_openShopWarningPopup = new PanelContainer
				{
					Name = "OpenShopWarningPopup",
					Visible = false,
					CustomMinimumSize = new Vector2(430, 250)
				};
				popupParent.AddChild(_openShopWarningPopup);

				var margin = new MarginContainer();
				margin.AddThemeConstantOverride("margin_left", 18);
				margin.AddThemeConstantOverride("margin_top", 16);
				margin.AddThemeConstantOverride("margin_right", 18);
				margin.AddThemeConstantOverride("margin_bottom", 16);
				_openShopWarningPopup.AddChild(margin);

				var root = new VBoxContainer();
				root.AddThemeConstantOverride("separation", 10);
				margin.AddChild(root);

				root.AddChild(new Label
				{
					Text = "Magazinul are probleme nerezolvate.",
					HorizontalAlignment = HorizontalAlignment.Center
				});

				_openShopWarningLabel = new Label
				{
					AutowrapMode = TextServer.AutowrapMode.WordSmart
				};
				root.AddChild(_openShopWarningLabel);

				var buttons = new HBoxContainer();
				buttons.AddThemeConstantOverride("separation", 8);
				root.AddChild(buttons);

				_openShopBackButton = new Button { Text = "Înapoi", SizeFlagsHorizontal = SizeFlags.ExpandFill };
				_openShopAnywayButton = new Button { Text = "Deschide oricum", SizeFlagsHorizontal = SizeFlags.ExpandFill };
				buttons.AddChild(_openShopBackButton);
				buttons.AddChild(_openShopAnywayButton);
			}

			if (_tutorialPopup == null)
			{
				_tutorialPopup = new PanelContainer
				{
					Name = "FirstRunTutorialPopup",
					Visible = false,
					CustomMinimumSize = TutorialPopupSize
				};
				popupParent.AddChild(_tutorialPopup);

				var margin = new MarginContainer();
				margin.AddThemeConstantOverride("margin_left", 20);
				margin.AddThemeConstantOverride("margin_top", 18);
				margin.AddThemeConstantOverride("margin_right", 20);
				margin.AddThemeConstantOverride("margin_bottom", 18);
				_tutorialPopup.AddChild(margin);

				var root = new VBoxContainer();
				root.AddThemeConstantOverride("separation", 10);
				margin.AddChild(root);

				_tutorialProgressLabel = new Label
				{
					HorizontalAlignment = HorizontalAlignment.Center,
					Modulate = new Color(0.83f, 0.68f, 0.66f, 1f)
				};
				root.AddChild(_tutorialProgressLabel);

				_tutorialTitleLabel = new Label
				{
					Text = "Cum decurge ziua",
					HorizontalAlignment = HorizontalAlignment.Center
				};
				root.AddChild(_tutorialTitleLabel);

				var bodyScroll = new ScrollContainer
				{
					Name = "TutorialBodyScroll",
					CustomMinimumSize = new Vector2(0, 96),
					SizeFlagsHorizontal = SizeFlags.ExpandFill,
					SizeFlagsVertical = SizeFlags.ExpandFill,
					HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
				};
				root.AddChild(bodyScroll);

				_tutorialBodyLabel = new Label
				{
					Text = "Dimineața primești livrări și evenimente.\nÎn Administrare ajustezi prețuri, comenzi și angajați.\nÎn faza de vânzare magazinul se deschide și vin clienții.\nLa Închidere primești raportul zilei.",
					AutowrapMode = TextServer.AutowrapMode.WordSmart,
					ClipText = true,
					CustomMinimumSize = new Vector2(0, 96),
					SizeFlagsHorizontal = SizeFlags.ExpandFill
				};
				bodyScroll.AddChild(_tutorialBodyLabel);

				var tutorialButtons = new HBoxContainer();
				tutorialButtons.AddThemeConstantOverride("separation", 10);
				root.AddChild(tutorialButtons);

				_tutorialDisableButton = new Button
				{
					Text = "Oprește tutorialul",
					SizeFlagsHorizontal = SizeFlags.ExpandFill
				};
				tutorialButtons.AddChild(_tutorialDisableButton);

				_tutorialCloseButton = new Button
				{
					Text = "Continuă",
					SizeFlagsHorizontal = SizeFlags.ExpandFill
				};
				tutorialButtons.AddChild(_tutorialCloseButton);
			}
		}

		private void EnsureConfirmationPopup()
		{
			if (_confirmationPopup != null)
				return;

			Node popupParent = _modalRoot ?? this;
			_confirmationPopup = new PanelContainer
			{
				Name = "ConfirmationPopup",
				Visible = false,
				CustomMinimumSize = ConfirmationPopupSize
			};
			popupParent.AddChild(_confirmationPopup);

			var margin = new MarginContainer();
			margin.AddThemeConstantOverride("margin_left", 18);
			margin.AddThemeConstantOverride("margin_top", 16);
			margin.AddThemeConstantOverride("margin_right", 18);
			margin.AddThemeConstantOverride("margin_bottom", 16);
			_confirmationPopup.AddChild(margin);

			var root = new VBoxContainer();
			root.AddThemeConstantOverride("separation", 10);
			margin.AddChild(root);

			_confirmationTitleLabel = new Label
			{
				Text = "Confirmare",
				HorizontalAlignment = HorizontalAlignment.Center
			};
			root.AddChild(_confirmationTitleLabel);

			_confirmationBodyLabel = new Label
			{
				AutowrapMode = TextServer.AutowrapMode.WordSmart
			};
			root.AddChild(_confirmationBodyLabel);

			var buttons = new HBoxContainer();
			buttons.AddThemeConstantOverride("separation", 8);
			root.AddChild(buttons);

			_confirmationCancelButton = new Button { Text = "Anulează", SizeFlagsHorizontal = SizeFlags.ExpandFill };
			_confirmationConfirmButton = new Button { Text = "Confirmă", SizeFlagsHorizontal = SizeFlags.ExpandFill };
			buttons.AddChild(_confirmationCancelButton);
			buttons.AddChild(_confirmationConfirmButton);
		}

		private void ShowConfirmation(string title, string body, string confirmText, Action action)
		{
			EnsureConfirmationPopup();
			_pendingConfirmationAction = action;
			if (_confirmationTitleLabel != null)
				_confirmationTitleLabel.Text = title;
			if (_confirmationBodyLabel != null)
				_confirmationBodyLabel.Text = body;
			if (_confirmationConfirmButton != null)
				_confirmationConfirmButton.Text = "Confirmă";
			if (_confirmationCancelButton != null)
				_confirmationCancelButton.Text = "Anulează";
			if (_confirmationPopup != null)
			{
				HideAllPopupsExceptConfirmation();
				ShowBoundedPopup(_confirmationPopup, ConfirmationPopupSize, avoidTopMenu: _game != null);
			}
		}

		private void HideAllPopupsExceptConfirmation()
		{
			HideOrdersPopup();
			HidePricesPopup();
			HideStaffPopup();
			HideEventPopup();
			HideReportPopup();
			HideSaveSlotPopup();
			HideOpenShopWarning();
		}

		private void HideConfirmationPopup()
		{
			_pendingConfirmationAction = null;
			if (_confirmationPopup != null)
				_confirmationPopup.Visible = false;
			RefreshModalOverlayVisibility();
		}

		private void ConfirmPendingAction()
		{
			var action = _pendingConfirmationAction;
			HideConfirmationPopup();
			action?.Invoke();
		}

		private void RefreshSaveSlotButtons()
		{
			bool hasLoadableSlot = false;
			for (int i = 0; i < _saveSlotButtons.Length; i++)
			{
				var button = _saveSlotButtons[i];
				if (button == null)
					continue;

				int slot = i + 1;
				bool loadable = IsSaveSlotLoadable(slot);
				hasLoadableSlot |= loadable;
				button.Text = BuildSaveSlotText(slot);
				button.Disabled = _saveSlotMode == SaveSlotMenuMode.Load && !loadable;
			}

			if (_saveSlotTitleLabel != null)
				_saveSlotTitleLabel.Text = _saveSlotMode == SaveSlotMenuMode.Load && !hasLoadableSlot
					? $"{T("BTN_LOAD_GAME")}\nNu există salvări."
					: _saveSlotMode == SaveSlotMenuMode.Save ? T("BTN_SAVE_GAME") : T("BTN_LOAD_GAME");
		}

		private bool IsSaveSlotLoadable(int slot)
		{
			string path = GetSaveSlotPath(slot);
			if (!File.Exists(path))
				return false;

			try
			{
				var persistence = new DataPersistenceManager();
				var save = persistence.Load(path);
				persistence.Validate(save);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private string BuildSaveSlotText(int slot)
		{
			string path = GetSaveSlotPath(slot);
			if (!File.Exists(path))
				return $"{T("LABEL_SLOT")} {slot}\n{T("LABEL_EMPTY")}";

			try
			{
				var save = new DataPersistenceManager().Load(path);
				string phase = save.CurrentPhase.HasValue ? PhaseName(save.CurrentPhase.Value) : T("LABEL_PHASE");
				string timestamp = FormatSaveTimestamp(save.SavedAtUtc);
				return $"{T("LABEL_SLOT")} {slot}\n{T("LABEL_DAY")} {save.CurrentDay} | {Money.FromMicros(save.CashMicros)} | {T("LABEL_REPUTATION")} {save.Reputation} | {phase}\n{timestamp}";
			}
			catch (Exception ex)
			{
				GD.PrintErr($"UIManager: failed to read save slot {slot} metadata - {ex.Message}");
				return $"{T("LABEL_SLOT")} {slot}\nSalvare ilizibilă";
			}
		}

		private static string FormatSaveTimestamp(string? savedAtUtc)
		{
			if (string.IsNullOrWhiteSpace(savedAtUtc))
				return T("LABEL_NO_TIMESTAMP");

			return DateTimeOffset.TryParse(savedAtUtc, out var timestamp)
				? timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
				: savedAtUtc;
		}

		private void OnSaveSlotPressed(int slot)
		{
			if (_saveSlotMode == SaveSlotMenuMode.Save)
			{
				SaveToSlot(slot);
				HideSaveSlotPopup();
				return;
			}

			bool loaded = LoadFromSlot(slot, _saveSlotOpenedFromStartup);
			if (loaded)
				HideSaveSlotPopup();
			else
				RefreshSaveSlotButtons();
		}

		private string GetSaveSlotPath(int slot)
		{
			string saveFolder = GetRepoLocalSaveFolderPath();
			Directory.CreateDirectory(saveFolder);
			return Path.Combine(saveFolder, $"slot_{slot}.json");
		}

		private string GetRepoLocalSaveFolderPath()
		{
			string projectFolder = ProjectSettings.GlobalizePath("res://");
			string normalizedProjectFolder = projectFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string repoFolder = Directory.GetParent(normalizedProjectFolder)?.FullName ?? normalizedProjectFolder;
			return Path.Combine(repoFolder, SaveFolderName);
		}

		private void HideStartupMenu()
		{
			HideStartupOptions();
			if (_newGameSetupPanel != null)
				_newGameSetupPanel.Visible = false;
			SetStartupMode(false);
		}

		private void SetStartupMode(bool isStartup)
		{
			if (_startupMenu != null)
				_startupMenu.Visible = isStartup;
			if (_newGameSetupPanel != null)
				_newGameSetupPanel.Visible = false;
			if (_gameplayRoot != null)
				_gameplayRoot.Visible = !isStartup;
			if (_storeHud != null)
				_storeHud.Visible = !isStartup;
			if (_modalRoot != null && isStartup)
				_modalRoot.Visible = false;
			if (_topHotbar != null)
				_topHotbar.Visible = !isStartup;
			if (_shop2DView != null)
				_shop2DView.Visible = !isStartup;
			if (_runtimeControlPanel != null)
				_runtimeControlPanel.Visible = !isStartup;
			if (_rightContextPanel != null)
				_rightContextPanel.Visible = false;

			if (isStartup)
			{
				_contextualHelp?.Dismiss();
				HideAllPopups();
			}
			else
			{
				RefreshModalOverlayVisibility();
			}
		}

		private void ShowStartupOptions()
		{
			if (_newGameSetupPanel != null)
				_newGameSetupPanel.Visible = false;
			if (_startupOptionsPopup != null)
				ShowBoundedPopup(_startupOptionsPopup, StartupOptionsPopupSize, avoidTopMenu: false);
		}

		private void HideStartupOptions()
		{
			if (_startupOptionsPopup != null)
				_startupOptionsPopup.Visible = false;
			RefreshModalOverlayVisibility();
		}

		private void ShowStartupMainMenu()
		{
			HideStartupOptions();
			if (_startupMenu != null)
				_startupMenu.Visible = true;
			if (_newGameSetupPanel != null)
				_newGameSetupPanel.Visible = false;
			if (_newGameValidationLabel != null)
				_newGameValidationLabel.Text = "";
		}

		private void ShowNewGameSetupPanel()
		{
			HideStartupOptions();
			PopulateNewGameSetupOptions();
			if (_newGameSetupPanel != null)
			{
				if (_startupMenu != null)
					_startupMenu.Visible = false;
				_newGameSetupPanel.Visible = true;
				_newGameSetupPanel.MoveToFront();
			}
			else
			{
				GD.PushWarning("UIManager: NewGameSetupPanel is missing from UIRoot.tscn.");
				Notify("Panoul Joc nou lipsește din scenă. Meniul principal rămâne deschis.");
				if (_startupMenu != null)
					_startupMenu.Visible = true;
				return;
			}

			if (_gameplayRoot != null)
				_gameplayRoot.Visible = false;
			if (_modalRoot != null)
				_modalRoot.Visible = false;
			if (_topHotbar != null)
				_topHotbar.Visible = false;
			if (_shop2DView != null)
				_shop2DView.Visible = false;
			if (_runtimeControlPanel != null)
				_runtimeControlPanel.Visible = false;
			if (_rightContextPanel != null)
				_rightContextPanel.Visible = false;

			if (_newGameStoreNameInput != null && string.IsNullOrWhiteSpace(_newGameStoreNameInput.Text))
				_newGameStoreNameInput.Text = GameStartSettings.Default.StoreName;
			if (_newGameValidationLabel != null)
				_newGameValidationLabel.Text = "";
		}

		private void BindShop2DView()
		{
			var shop = FindNodeRecursive("Shop2DView", this);
			if (shop == null)
				return;

			_shop2DStatusLabel = FindNodeRecursive("Shop2DStatusLabel", shop) as Label;
			_shop2DShelfCartridgeLabel = FindNodeRecursive("Shop2DShelfCartridgeLabel", shop) as Label;
			_shop2DShelfConsoleLabel = FindNodeRecursive("Shop2DShelfConsoleLabel", shop) as Label;
			_shop2DShelfCollectorLabel = FindNodeRecursive("Shop2DShelfCollectorLabel", shop) as Label;
			_shop2DStorageLabel = FindNodeRecursive("Shop2DStorageLabel", shop) as Label;
			_deliveryCountLabel = FindNodeRecursive("DeliveryCountLabel", shop) as Label;
			_shop2DRegisterLabel = FindNodeRecursive("Shop2DRegisterLabel", shop) as Label;
			_shelfCartridgeZone = FindNodeRecursive("ShelfCartridgeZone2D", shop) as ColorRect;
			_shelfConsoleZone = FindNodeRecursive("ShelfConsoleZone2D", shop) as ColorRect;
			_shelfCollectorZone = FindNodeRecursive("ShelfCollectorZone2D", shop) as ColorRect;
			_storageZone = FindNodeRecursive("StorageZone2D", shop) as ColorRect;
			_deliveryZone = FindNodeRecursive("DeliveryZone2D", shop) as ColorRect;
			_entranceZone = FindNodeRecursive("EntranceZone2D", shop) as ColorRect;
			_storageFillBar = FindNodeRecursive("StorageFillBar", shop) as ColorRect;
			_registerZone = FindNodeRecursive("RegisterZone2D", shop) as ColorRect;
			_queueLane = FindNodeRecursive("QueueLane2D", shop) as ColorRect;
			for (int i = 0; i < _customerMarkers.Length; i++)
				_customerMarkers[i] = FindNodeRecursive($"CustomerMarker{i + 1}", shop) as ColorRect;
			for (int i = 0; i < _queueSpots.Length; i++)
				_queueSpots[i] = FindNodeRecursive($"QueueSpot{i + 1}", shop) as ColorRect;
			_cashierMarker = FindNodeRecursive("CashierMarker", shop) as ColorRect;
			_stockerMarker = FindNodeRecursive("StockerMarker", shop) as ColorRect;
			_managerMarker = FindNodeRecursive("ManagerMarker", shop) as ColorRect;
			_deliveryCrateMarker1 = FindNodeRecursive("DeliveryCrateMarker1", shop) as ColorRect;
			_deliveryCrateMarker2 = FindNodeRecursive("DeliveryCrateMarker2", shop) as ColorRect;
			CacheShopSceneLayout();
			ApplyShopSceneScale();
			EnsureShopVisualControllers();
		}

		private void CacheShopSceneLayout()
		{
			if (_shop2DView == null || _shopNodeLayouts.Count > 0)
				return;

			foreach (Node child in _shop2DView.GetChildren())
			{
				if (child is not Control control || !ShouldScaleShopChild(control))
					continue;

				_shopNodeLayouts[control] = new ShopNodeLayout(control.Position, control.Scale);
			}
		}

		private void ApplyShopSceneScale()
		{
			if (_shop2DView == null)
				return;

			CacheShopSceneLayout();
			Vector2 available = _shop2DView.Size;
			if (available.X <= 0f || available.Y <= 0f)
				return;

			float scale = Math.Clamp(Math.Min(available.X / ShopDesignWidth, available.Y / ShopDesignHeight), 0.52f, 0.88f);
			foreach (var entry in _shopNodeLayouts)
			{
				Control control = entry.Key;
				ShopNodeLayout layout = entry.Value;
				if (!GodotObject.IsInstanceValid(control))
					continue;

				control.Position = layout.Position * scale;
				control.Scale = layout.Scale * scale;
			}
		}

		private static bool ShouldScaleShopChild(Control control)
		{
			string name = control.Name.ToString();
			return name is not "ShopBackdrop"
				&& name is not "ShopFloor"
				&& !name.EndsWith("VisualLayer", StringComparison.Ordinal);
		}

		private void EnsureShopVisualControllers()
		{
			if (_shop2DView == null)
				return;

			_storeLayoutManager ??= new StoreLayoutManager(
				_shop2DView,
				_entranceZone,
				_shelfCartridgeZone,
				_shelfConsoleZone,
				_shelfCollectorZone,
				_storageZone,
				_deliveryZone,
				_registerZone,
				_queueLane);
			_customerVisualController ??= new CustomerVisualController(_shop2DView, _storeLayoutManager);
			_employeeVisualController ??= new EmployeeVisualController(_shop2DView, _storeLayoutManager, _customerVisualController);
			_storeFurnitureVisualController ??= new StoreFurnitureVisualController(_shop2DView, _storeLayoutManager);
		}

		private void InvalidateShopNavigation()
		{
			_storeLayoutManager?.InvalidateNavigationCache();
			_customerVisualController?.InvalidatePaths();
			_employeeVisualController?.InvalidatePaths();
			_lastFullRefreshSignature = "";
		}

		private void AutoWireNodes()
		{
			var type = GetType();
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var field in fields)
			{
				if (!typeof(Node).IsAssignableFrom(field.FieldType))
					continue;

				if (field.GetValue(this) != null)
					continue;

				var node = FindNodeRecursive(field.Name, this);
				if (node != null && field.FieldType.IsAssignableFrom(node.GetType()))
					field.SetValue(this, node);
			}
		}

		private Node? FindNodeRecursive(string name, Node parent)
		{
			foreach (Node child in parent.GetChildren())
			{
				if (child.Name == name)
					return child;

				var result = FindNodeRecursive(name, child);
				if (result != null)
					return result;
			}

			return null;
		}

		public void Refresh(bool forceFullRefresh = true)
		{
			if (_game == null) return;
			// Do not force startup mode changes during a Refresh; caller should manage startup visibility.
			TrackCompletedReport();
			if (forceFullRefresh)
				_lastFullRefreshSignature = BuildFullRefreshSignature();
			bool fullRefresh = forceFullRefresh || ShouldRunFullRefresh();
			RefreshRuntimePanel(fullRefresh);
			if (fullRefresh)
				RefreshShop2DView();
			DebugCheckLayout();
		}

		private bool ShouldRunFullRefresh()
		{
			string signature = BuildFullRefreshSignature();
			if (signature == _lastFullRefreshSignature)
				return false;

			_lastFullRefreshSignature = signature;
			return true;
		}

		private string BuildFullRefreshSignature()
		{
			if (_game == null)
				return "";

			var builder = new StringBuilder(256);
			builder.Append(_game.CurrentDay).Append('|')
				.Append(_game.CurrentPhase).Append('|')
				.Append(_game.BusinessTicksRemaining / 30).Append('|')
				.Append(_game.Economy.Cash.ToMicros()).Append('|')
				.Append(_game.RunProfit.ToMicros()).Append('|')
				.Append(_game.Customers.Reputation).Append('|')
				.Append(_game.CurrentQueueLength).Append('|')
				.Append(_game.CurrentCustomersServedToday).Append('|')
				.Append(_game.CurrentCustomersLostToday).Append('|')
				.Append(_game.Inventory.TotalStorageUnits).Append('/')
				.Append(_game.Inventory.StorageCapacity).Append('|')
				.Append(_game.Inventory.PendingOrders.Count).Append('|')
				.Append(_game.Employees.Employees.Count).Append('|')
				.Append(_game.Employees.Candidates.Count).Append('|')
				.Append(_game.PurchasedCatalogItemIds.Count).Append('|')
				.Append(_game.LastCheckoutFeedbackSequence).Append('|')
				.Append(_game.LastReputationFeedbackSequence);

			foreach (var product in _game.Inventory.Products)
				builder.Append("|p").Append(product.Id).Append(':').Append(product.Quantity).Append(':').Append(product.SalePrice.ToMicros()).Append(':').Append(product.Popularity);
			foreach (var shelf in _game.Inventory.Shelves)
				builder.Append("|s").Append(shelf.Id).Append(':').Append(shelf.ProductId).Append(':').Append(shelf.CurrentStock).Append('/').Append(shelf.Capacity).Append(':').Append((int)shelf.DisplayType);

			return builder.ToString();
		}

		private void DebugCheckLayout()
		{
			if (!EnableLayoutDebug)
				return;

			var viewport = GetViewportRect();
			DebugCheckLayoutRecursive(this, viewport, "UIRoot");
		}

		private void DebugCheckLayoutRecursive(Node node, Rect2 viewport, string path)
		{
			foreach (Node child in node.GetChildren())
			{
				string childPath = $"{path}/{child.Name}";
				if (child is Control control && control.Visible)
				{
					var rect = control.GetGlobalRect();
					if (rect.Position.X < -1 || rect.Position.Y < -1)
						GD.PushWarning($"Layout: {childPath} has negative position {rect.Position}.");
					if (rect.End.X > viewport.Size.X + 1 || rect.End.Y > viewport.Size.Y + 1)
						GD.PushWarning($"Layout: {childPath} exceeds viewport {viewport.Size} with rect {rect}.");
					if (rect.Size.X > viewport.Size.X || rect.Size.Y > viewport.Size.Y)
						GD.PushWarning($"Layout: {childPath} is larger than viewport: {rect.Size}.");
					if (child is Label label && label.ClipText && label.GetCombinedMinimumSize().X > rect.Size.X + 1)
						GD.PushWarning($"Layout: clipped label {childPath} may be too narrow.");
				}
				DebugCheckLayoutRecursive(child, viewport, childPath);
			}
		}

		private void RefreshShop2DView()
		{
			if (_game == null)
				return;

			if (_shop2DStatusLabel != null)
			{
				_shop2DStatusLabel.Visible = false;
				_shop2DStatusLabel.Text = "";
			}

			var cartridge = _game.Inventory.GetProduct(1);
			var console = _game.Inventory.GetProduct(2);
			var collector = _game.Inventory.GetProduct(3);
			int cartridgeShelfStock = GetShelfStock(1);
			int consoleShelfStock = GetShelfStock(2);
			int collectorShelfStock = GetShelfStock(3);

			if (_shop2DShelfCartridgeLabel != null && cartridge != null)
				_shop2DShelfCartridgeLabel.Text = $"{ProductName(cartridge)}\n{cartridge.SalePrice}\nRaft {cartridgeShelfStock} | Depozit {cartridge.Quantity}\nStare: {DescribeShelfStock(cartridgeShelfStock, 60)}";

			if (_shop2DShelfConsoleLabel != null && console != null)
				_shop2DShelfConsoleLabel.Text = $"{ProductName(console)}\n{console.SalePrice}\nRaft {consoleShelfStock} | Depozit {console.Quantity}\nStare: {DescribeShelfStock(consoleShelfStock, 10)}";

			if (_shop2DShelfCollectorLabel != null && collector != null)
				_shop2DShelfCollectorLabel.Text = $"{ProductName(collector)}\n{collector.SalePrice}\nRaft {collectorShelfStock} | Depozit {collector.Quantity}\nStare: {DescribeShelfStock(collectorShelfStock, 8)}";

			if (_shop2DStorageLabel != null)
			{
				bool nearlyFull = _game.Inventory.StorageCapacity > 0 && _game.Inventory.TotalStorageUnits >= _game.Inventory.StorageCapacity * 0.85f;
				_shop2DStorageLabel.Text = $"{T("LABEL_STORAGE").ToUpperInvariant()}\n{_game.Inventory.TotalStorageUnits}/{_game.Inventory.StorageCapacity}\n{(nearlyFull ? "Aproape plin" : "Spațiu disponibil")}";
			}

			if (_deliveryCountLabel != null)
				_deliveryCountLabel.Text = $"{T("LABEL_ORDERS")} {_game.Inventory.PendingOrders.Count}";

			if (_shop2DRegisterLabel != null)
			{
				int queueLength = _game.CurrentPhase == DayPhase.Business ? _game.CurrentQueueLength : 0;
				_shop2DRegisterLabel.Text = $"CASĂ\n{DescribeRegisterState()}\nCoadă {queueLength}\nServiți {_game.CurrentCustomersServedToday}\nCap. {_game.CheckoutCapacity}/val";
			}

			RefreshShopZoneColors(cartridgeShelfStock, consoleShelfStock, collectorShelfStock);
			EnsureShopVisualControllers();
			_storeLayoutManager?.RefreshLayout();
			_storeFurnitureVisualController?.Refresh(_game);
			_customerVisualController?.Refresh(_game);
			_employeeVisualController?.Refresh(_game);
			RefreshShopMarkers();
		}

		private void RefreshShopZoneColors(int cartridgeShelfStock, int consoleShelfStock, int collectorShelfStock)
		{
			if (_game == null)
				return;

			SetShelfColor(_shelfCartridgeZone, GetDominantShelfState(1), new Color(0.48f, 0.15f, 0.22f));
			SetShelfColor(_shelfConsoleZone, GetDominantShelfState(2), new Color(0.56f, 0.17f, 0.23f));
			SetShelfColor(_shelfCollectorZone, GetDominantShelfState(3), new Color(0.63f, 0.19f, 0.25f));

			if (_storageZone != null)
			{
				float storageRatio = _game.Inventory.StorageCapacity <= 0 ? 0f : (float)_game.Inventory.TotalStorageUnits / _game.Inventory.StorageCapacity;
				_storageZone.Color = storageRatio >= 0.95f
					? new Color(0.62f, 0.17f, 0.21f)
					: storageRatio >= 0.75f ? new Color(0.72f, 0.24f, 0.26f) : new Color(0.46f, 0.15f, 0.22f);
				if (_storageFillBar != null)
				{
					_storageFillBar.Size = new Vector2(Math.Clamp(storageRatio, 0.03f, 1f) * 168f, 8f);
					_storageFillBar.Color = storageRatio >= 0.95f
						? new Color(0.73f, 0.2f, 0.24f)
						: storageRatio >= 0.75f ? new Color(0.9f, 0.45f, 0.37f) : new Color(0.78f, 0.28f, 0.29f);
				}
			}

			if (_registerZone != null)
				_registerZone.Color = _game.CurrentPhase == DayPhase.Business ? new Color(0.6f, 0.18f, 0.24f) : new Color(0.35f, 0.1f, 0.19f);

			if (_queueLane != null)
			{
				bool queuePressure = _game.CurrentPhase == DayPhase.Business && _game.CurrentQueueLength >= 4;
				_queueLane.Color = queuePressure ? new Color(0.69f, 0.18f, 0.22f) : new Color(0.52f, 0.16f, 0.23f);
			}
		}

		private string DescribeShopPhaseState()
		{
			if (_game == null)
				return "";

			return _game.CurrentPhase switch
			{
				DayPhase.Management => "PREGĂTIRE: verifică rafturi, comenzi și personal",
				DayPhase.Business => $"MAGAZIN DESCHIS: {DescribeSpeed()}, {GetBusinessProgressPercent()}%",
				DayPhase.Closing => "ZI ÎNCHEIATĂ: citește raportul",
				DayPhase.Morning => "DIMINEAȚĂ: livrări și taxe",
				_ => ""
			};
		}

		private static void SetShelfColor(ColorRect? zone, ShelfState state, Color normal)
		{
			if (zone == null)
				return;

			zone.Color = state switch
			{
				ShelfState.Empty => new Color(0.52f, 0.13f, 0.19f),
				ShelfState.LowStock => new Color(0.74f, 0.26f, 0.27f),
				ShelfState.Full => new Color(0.78f, 0.28f, 0.29f),
				_ => normal
			};
		}

		private ShelfState GetDominantShelfState(int productId)
		{
			if (_game == null)
				return ShelfState.Empty;

			var shelves = _game.Inventory.GetShelvesForProduct(productId).ToList();
			if (shelves.Count == 0)
				return ShelfState.Empty;
			if (shelves.Any(s => s.State == ShelfState.Empty))
				return ShelfState.Empty;
			if (shelves.Any(s => s.State == ShelfState.LowStock))
				return ShelfState.LowStock;
			if (shelves.All(s => s.State == ShelfState.Full))
				return ShelfState.Full;

			return ShelfState.Normal;
		}

		private string DescribeRegisterState()
		{
			if (_game == null || _game.CurrentPhase != DayPhase.Business)
				return "Liber";
			if (_game.CurrentQueueLength >= 4)
				return "Aglomerat";

			return "Ocupat";
		}

		private void RefreshShopMarkers()
		{
			if (_game == null)
				return;

			bool advancedVisualsReady = _customerVisualController != null && _employeeVisualController != null && _storeFurnitureVisualController != null;
			if (advancedVisualsReady)
			{
				for (int i = 0; i < _customerMarkers.Length; i++)
				{
					if (_customerMarkers[i] != null)
						_customerMarkers[i]!.Visible = false;
				}
				if (_cashierMarker != null)
					_cashierMarker.Visible = false;
				if (_stockerMarker != null)
					_stockerMarker.Visible = false;
				if (_managerMarker != null)
					_managerMarker.Visible = false;
				if (_deliveryCrateMarker1 != null)
					_deliveryCrateMarker1.Visible = false;
				if (_deliveryCrateMarker2 != null)
					_deliveryCrateMarker2.Visible = false;
			}
			else
			{
				float progress = _game.CurrentPhase == DayPhase.Business && _game.BusinessTicksPerDayTotal > 0
					? 1f - (float)_game.BusinessTicksRemaining / _game.BusinessTicksPerDayTotal
					: 0f;

				for (int i = 0; i < _customerMarkers.Length; i++)
				{
					var marker = _customerMarkers[i];
					if (marker == null)
						continue;

					int visibleCustomerCount = _game.CurrentPhase == DayPhase.Business
						? Math.Min(_customerMarkers.Length, Math.Max(2, _game.CurrentQueueLength + 1))
						: _game.CurrentPhase == DayPhase.Management ? 1 : 0;
					marker.Visible = i < visibleCustomerCount;
					float offset = i * 0.16f;
					float t = Math.Clamp((progress + offset) % 1f, 0f, 1f);
					marker.Position = GetCustomerAislePosition(t, i);
				}

				if (_cashierMarker != null)
					_cashierMarker.Visible = _game.Employees.CountRole(EmployeeRole.Cashier) > 0;
				if (_stockerMarker != null)
					_stockerMarker.Visible = _game.Employees.CountRole(EmployeeRole.Stocker) > 0;
				if (_managerMarker != null)
					_managerMarker.Visible = _game.Employees.CountRole(EmployeeRole.Manager) > 0;
				if (_deliveryCrateMarker1 != null)
					_deliveryCrateMarker1.Visible = _game.Inventory.PendingOrders.Count > 0;
				if (_deliveryCrateMarker2 != null)
					_deliveryCrateMarker2.Visible = _game.Inventory.PendingOrders.Count > 1;
			}

			for (int i = 0; i < _queueSpots.Length; i++)
			{
				var spot = _queueSpots[i];
				if (spot == null)
					continue;

				bool occupied = _game.CurrentPhase == DayPhase.Business && _game.CurrentQueueLength > i;
				spot.Color = occupied ? new Color(0.92f, 0.58f, 0.5f) : new Color(0.68f, 0.25f, 0.27f);
			}
		}

		private static Vector2 GetCustomerAislePosition(float t, int customerIndex)
		{
			float laneOffset = customerIndex * 14f;
			var entrance = new Vector2(430f + laneOffset, 520f);
			var aisleEntry = new Vector2(494f + laneOffset * 0.35f, 520f);
			var upperAisle = new Vector2(494f + laneOffset * 0.35f, 168f);
			var browseAisle = new Vector2(494f + laneOffset * 0.35f, 468f);
			var queueEntry = new Vector2(698f + customerIndex * 22f, 468f);

			return t switch
			{
				< 0.16f => Lerp(entrance, aisleEntry, t / 0.16f),
				< 0.46f => Lerp(aisleEntry, upperAisle, (t - 0.16f) / 0.30f),
				< 0.72f => Lerp(upperAisle, browseAisle, (t - 0.46f) / 0.26f),
				_ => Lerp(browseAisle, queueEntry, (t - 0.72f) / 0.28f)
			};
		}

		private static Vector2 Lerp(Vector2 from, Vector2 to, float amount)
		{
			float clampedAmount = Math.Clamp(amount, 0f, 1f);
			return from + (to - from) * clampedAmount;
		}

		private int GetShelfStock(int productId)
		{
			if (_game == null)
				return 0;

			int stock = 0;
			foreach (var shelf in _game.Inventory.GetShelvesForProduct(productId))
				stock += shelf.CurrentStock;

			return stock;
		}

		private static string DescribeShelfStock(int stock, int startingStock)
		{
			if (stock <= 0)
				return T("STATUS_EMPTY");
			if (stock <= Math.Max(1, startingStock / 4))
				return T("STATUS_LOW");
			if (stock >= startingStock)
				return T("STATUS_FULL");

			return T("STATUS_ACTIVE");
		}

		private void PopulateRuntimeLists()
		{
			if (_game == null)
				return;

			PopulateProductPicker(_runtimePriceProductPicker, includeStock: false);
			PopulateProductPicker(_runtimeOrderProductPicker, includeStock: true);
			PopulateProductPicker(_runtimeShelfProductPicker, includeStock: true);
			PopulateShelfPicker();
			PopulateShopCatalogPicker();
			PopulateStaffPickers();

			if (_runtimeOrderSupplierPicker != null)
			{
				int selected = _runtimeOrderSupplierPicker.Selected;
				_runtimeOrderSupplierPicker.Clear();
				foreach (var supplier in VisibleSuppliers())
					_runtimeOrderSupplierPicker.AddItem(
						$"#{supplier.Id}: {supplier.Name} | livrare {supplier.DeliveryDays}z | fiabilitate {supplier.Reliability}% | cost x{supplier.PriceMultiplierBasisPoints / 10000m:0.##}",
						supplier.Id);

				if (_runtimeOrderSupplierPicker.GetItemCount() > 0)
					_runtimeOrderSupplierPicker.Selected = Math.Clamp(selected, 0, _runtimeOrderSupplierPicker.GetItemCount() - 1);
			}

			UpdateRuntimePriceInput();
		}

		private void PopulateShelfPicker()
		{
			if (_game == null || _runtimeShelfPicker == null)
				return;

			int selected = _runtimeShelfPicker.Selected;
			_runtimeShelfPicker.Clear();
			foreach (var shelf in _game.Inventory.Shelves)
			{
				var product = _game.Inventory.GetProduct(shelf.ProductId);
				_runtimeShelfPicker.AddItem(
					$"#{shelf.Id} {Localizer.Shelf(shelf.DisplayType)}: {(product == null ? "produs necunoscut" : ProductName(product))} | stoc {shelf.CurrentStock}/{shelf.Capacity} | {DescribeShelfState(shelf.CurrentStock, shelf.Capacity)}",
					shelf.Id
				);
			}

			if (_runtimeShelfPicker.GetItemCount() > 0)
				_runtimeShelfPicker.Selected = Math.Clamp(selected, 0, _runtimeShelfPicker.GetItemCount() - 1);
		}

		private void PopulateShopCatalogPicker()
		{
			if (_game == null || _runtimeShopCatalogPicker == null)
				return;

			int selected = _runtimeShopCatalogPicker.Selected;
			_runtimeShopCatalogPicker.Clear();
			foreach (var item in _game.ShopCatalog)
				_runtimeShopCatalogPicker.AddItem($"{Localizer.ShopItem(item.Id)} | {CatalogTypeRo(item.Type)} | {item.Cost} | {DescribeShopCatalogItem(item)}", item.Id);

			if (_runtimeShopCatalogPicker.GetItemCount() > 0)
				_runtimeShopCatalogPicker.Selected = Math.Clamp(selected, 0, _runtimeShopCatalogPicker.GetItemCount() - 1);
		}

		private void PopulateStaffPickers()
		{
			if (_game == null)
				return;

			if (_runtimeCandidatePicker != null)
			{
				int selected = _runtimeCandidatePicker.Selected;
				_runtimeCandidatePicker.Clear();
			foreach (var candidate in _game.Employees.Candidates)
				{
					var profile = candidate.Profile;
					_runtimeCandidatePicker.AddItem(
						$"{profile.Name} - {profile.Role} | salariu {profile.Salary}/zi | productivitate {profile.Efficiency} | moral {profile.Morale} | disponibil până în Z{candidate.AvailableUntilDay}",
						candidate.Id
					);
				}

				if (_runtimeCandidatePicker.GetItemCount() > 0)
					_runtimeCandidatePicker.Selected = Math.Clamp(selected, 0, _runtimeCandidatePicker.GetItemCount() - 1);
			}

			if (_runtimeEmployeePicker != null)
			{
				int selected = _runtimeEmployeePicker.Selected;
				_runtimeEmployeePicker.Clear();
				for (int i = 0; i < _game.Employees.Employees.Count; i++)
				{
					var employee = _game.Employees.Employees[i];
					_runtimeEmployeePicker.AddItem($"{employee.Name} - {employee.Role} | salariu {employee.Salary}/zi | productivitate {employee.Efficiency} | moral {employee.Morale}", i);
				}

				if (_runtimeEmployeePicker.GetItemCount() > 0)
					_runtimeEmployeePicker.Selected = Math.Clamp(selected, 0, _runtimeEmployeePicker.GetItemCount() - 1);
			}
		}

		private void PopulateProductPicker(OptionButton? picker, bool includeStock)
		{
			if (_game == null || picker == null)
				return;

			int selected = picker.Selected;
			picker.Clear();
			foreach (var product in VisibleProducts())
			{
				string label = includeStock
					? $"#{product.Id}: {ProductName(product)} | stoc {product.Quantity} | cerere {DescribeDemand(product)} | epuizare ~{EstimateDaysUntilStockout(product)} zile"
					: $"#{product.Id}: {ProductName(product)} | preț {product.SalePrice} | cerere {DescribeDemand(product)}";
				picker.AddItem(label, product.Id);
			}

			if (picker.GetItemCount() > 0)
				picker.Selected = Math.Clamp(selected, 0, picker.GetItemCount() - 1);
		}

		private void RefreshRuntimePanel(bool fullRefresh)
		{
			if (_game == null)
				return;

			if (!fullRefresh)
			{
				RefreshRuntimeHudOnly();
				return;
			}

			PopulateRuntimeLists();
			ResetDailyTaskStateIfNeeded();
			TrackCompletedReport();
			var report = GetDisplayReport();
			var previousReport = _previousCompletedReport;
			Money liveDailyProfit = _game.Economy.DailyRevenue - _game.Economy.DailyExpenses;
			if (_lastObservedPhase != null && _lastObservedPhase != _game.CurrentPhase)
				Notify(BuildPhaseTransitionMessage(_lastObservedPhase.Value, _game.CurrentPhase, report));
			NotifySimulationFeedback();

			if (_runtimeHudLabel != null)
				SetRuntimeHudText(liveDailyProfit);
			RefreshStoreHudDetails(liveDailyProfit);

			if (_runtimeReportLabel != null)
				_runtimeReportLabel.Text = BuildRuntimeReport(report, previousReport);

			if (_runtimeHintLabel != null)
			{
				_runtimeHintLabel.Visible = true;
				SetLabelTextIfChanged(_runtimeHintLabel, BuildPhaseHint(), ref _lastHintText);
			}
			if (_runtimeTaskBoxLabel != null)
			{
				_runtimeTaskBoxLabel.Text = BuildTaskBoxText();
				_runtimeTaskBoxLabel.Visible = true;
			}
			RefreshMenuProgressIndicators();
			if (_runtimeBusinessProgressBar != null)
			{
				_runtimeBusinessProgressBar.Visible = _game.CurrentPhase == DayPhase.Business;
				SetBusinessProgressValue(GetBusinessProgressPercent());
			}

			if (_runtimeAlertSummaryLabel != null)
			{
				var alertSummary = BuildAlertSummary();
				_runtimeAlertSummaryLabel.Visible = true;
				SetLabelTextIfChanged(_runtimeAlertSummaryLabel, alertSummary, ref _lastAlertSummaryText);
			}

			if (IsTutorialActive()
				&& _tutorialStep == TutorialStep.ObserveCustomers
				&& _game.CurrentPhase == DayPhase.Business
				&& (_game.CurrentCustomersServedToday > 0 || GetBusinessProgressPercent() >= 15))
			{
				CompleteTutorialStep(TutorialStep.ObserveCustomers);
			}

			if (_game.CurrentPhase == DayPhase.Closing && _lastObservedPhase != DayPhase.Closing)
			{
				_showAdvancedReport = false;
				ShowRuntimeSection(_runtimeReportSection);
				ShowReportPopup();
			}
			if (!_game.IsLoopActive)
			{
				_showAdvancedReport = false;
				ShowRuntimeSection(_runtimeReportSection);
				ShowReportPopup();
			}
			if (_game.CurrentDecision != null && _lastObservedPhase != _game.CurrentPhase && _game.CurrentPhase == DayPhase.Management)
				ShowEventPopup();
			_lastObservedPhase = _game.CurrentPhase;

			if (_runtimeOpenShopButton != null)
			{
				_runtimeOpenShopButton.Disabled = !_game.IsLoopActive || _game.CurrentPhase != DayPhase.Management;
				_runtimeOpenShopButton.Text = "Deschide";
				_runtimeOpenShopButton.Modulate = Colors.White;
			}
			if (_runtimeNextDayButton != null)
			{
				_runtimeNextDayButton.Disabled = !_game.IsLoopActive || _game.CurrentPhase != DayPhase.Closing;
				_runtimeNextDayButton.Text = "Ziua urm.";
				_runtimeNextDayButton.Modulate = Colors.White;
			}
			bool speedEnabled = _game.IsLoopActive && _game.CurrentPhase == DayPhase.Business;
			if (_runtimePauseSpeedButton != null)
				_runtimePauseSpeedButton.Disabled = !speedEnabled;
			if (_runtimeNormalSpeedButton != null)
				_runtimeNormalSpeedButton.Disabled = !speedEnabled;
			if (_runtimeFastSpeedButton != null)
				_runtimeFastSpeedButton.Disabled = !speedEnabled;
			if (_runtimeFasterSpeedButton != null)
				_runtimeFasterSpeedButton.Disabled = !speedEnabled;
			bool managementEnabled = _game.IsLoopActive && _game.CurrentPhase == DayPhase.Management;
			if (_runtimeManageTabButton != null)
				_runtimeManageTabButton.Disabled = !managementEnabled;
			if (_runtimeStockTabButton != null)
				_runtimeStockTabButton.Disabled = !managementEnabled;
			if (_runtimeStaffTabButton != null)
				_runtimeStaffTabButton.Disabled = !managementEnabled;
			if (_runtimeEventTabButton != null)
				_runtimeEventTabButton.Disabled = !managementEnabled;
			if (_runtimeReportTabButton != null)
				_runtimeReportTabButton.Disabled = false;
			if (_runtimeApplyPriceButton != null)
				_runtimeApplyPriceButton.Disabled = !managementEnabled;
			if (_runtimePlaceOrderButton != null)
				_runtimePlaceOrderButton.Disabled = !managementEnabled;
			if (_runtimeBuyShopCatalogButton != null)
				_runtimeBuyShopCatalogButton.Disabled = !managementEnabled;
			if (_runtimeOpenOrdersPopupButton != null)
				_runtimeOpenOrdersPopupButton.Disabled = !managementEnabled;
			if (_runtimeOpenPricesPopupButton != null)
				_runtimeOpenPricesPopupButton.Disabled = !managementEnabled;
			if (_runtimeOpenStaffPopupButton != null)
				_runtimeOpenStaffPopupButton.Disabled = !managementEnabled;
			if (_runtimeOpenEventPopupButton != null)
				_runtimeOpenEventPopupButton.Disabled = !managementEnabled;
			if (_runtimeOpenReportPopupButton != null)
				_runtimeOpenReportPopupButton.Disabled = false;
			if (_runtimeStatsTabButton != null)
				_runtimeStatsTabButton.Disabled = false;
			if (_runtimeAssignShelfButton != null)
				_runtimeAssignShelfButton.Disabled = !managementEnabled || _game.Inventory.Shelves.Count == 0;
			if (_runtimeRefillShelfButton != null)
				_runtimeRefillShelfButton.Disabled = !managementEnabled || _game.Inventory.Shelves.Count == 0;
			if (_runtimeRefillAllShelvesButton != null)
				_runtimeRefillAllShelvesButton.Disabled = !managementEnabled;
			if (_runtimeHireCandidateButton != null)
				_runtimeHireCandidateButton.Disabled = !managementEnabled || _game.Employees.Candidates.Count == 0;
			if (_runtimeFireEmployeeButton != null)
				_runtimeFireEmployeeButton.Disabled = !managementEnabled || _game.Employees.Employees.Count == 0;
			if (_runtimeTriggerEventButton != null)
				_runtimeTriggerEventButton.Disabled = !managementEnabled || _game.CurrentDecision != null;
			RefreshEventDecisionPanel();

			if (_runtimeStaffSummaryLabel != null)
			{
				var operations = _game.Employees.GetOperations();
				var employees = _game.Employees.Employees
					.Take(4)
					.Select(employee => $"{employee.Name}: {employee.Role} | salariu {employee.Salary} | moral {employee.Morale} | performanță {employee.Efficiency}")
					.ToList();
				string explanation = _game.Employees.AverageMorale >= 70
					? "Moral bun: echipa lucrează eficient."
					: _game.Employees.AverageMorale <= 45
						? "Moral scăzut: eficiența poate scădea."
						: "Moral stabil: urmărește-l înainte să apară probleme.";
				string taskFeedback = _game.Employees.CountRole(EmployeeRole.Stocker) > 0 && !HasLowShelfStock()
					? "\nStocarii nu au sarcină urgentă: rafturile sunt stabile."
					: "";
				_runtimeStaffSummaryLabel.Text =
					$"Salarii {_game.Employees.TotalSalary}/zi | Moral {_game.Employees.AverageMorale} | {explanation}\n" +
					$"Impact operațional: casă {_game.CheckoutCapacity}/val | rafturi {_game.DailyRestockCapacity}/zi | protecție moral {operations.MoraleProtection}\n" +
					(employees.Count > 0 ? $"\n{string.Join("\n", employees)}" : "\nNu există angajați activi.") +
					taskFeedback;
			}

			if (_runtimeShelfSummaryLabel != null)
				_runtimeShelfSummaryLabel.Text = BuildShelfSummary();
		}

		private void RefreshRuntimeHudOnly()
		{
			if (_game == null)
				return;

			Money liveDailyProfit = _game.Economy.DailyRevenue - _game.Economy.DailyExpenses;
			if (_runtimeHudLabel != null)
				SetRuntimeHudText(liveDailyProfit);
			RefreshStoreHudDetails(liveDailyProfit);

			if (_runtimeBusinessProgressBar != null)
			{
				_runtimeBusinessProgressBar.Visible = _game.CurrentPhase == DayPhase.Business;
				SetBusinessProgressValue(GetBusinessProgressPercent());
			}

			if (_runtimeHintLabel != null && _game.CurrentPhase == DayPhase.Business)
			{
				_runtimeHintLabel.Visible = true;
				SetLabelTextIfChanged(_runtimeHintLabel, BuildPhaseHint(), ref _lastHintText);
			}

			if (_runtimeAlertSummaryLabel != null)
			{
				_runtimeAlertSummaryLabel.Visible = true;
				SetLabelTextIfChanged(_runtimeAlertSummaryLabel, BuildAlertSummary(), ref _lastAlertSummaryText);
			}

			NotifySimulationFeedback();
		}

		private void SetRuntimeHudText(Money liveDailyProfit)
		{
			if (_game == null || _runtimeHudLabel == null)
				return;

			_runtimeHudLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
			_runtimeHudLabel.ClipText = false;
			int queueLength = _game.CurrentPhase == DayPhase.Business ? _game.CurrentQueueLength : 0;
			string text = $"Zi {_game.CurrentDay}/{_game.LoopDayLimit} | {ShortPhaseName(_game.CurrentPhase)} | Lei {_game.Economy.Cash} | Rep {_game.Customers.Reputation} | Cl {_game.CurrentCustomersServedToday}/{_game.CurrentCustomersLostToday} | Stoc {_game.Inventory.TotalStorageUnits}/{_game.Inventory.StorageCapacity} | Coadă {queueLength}";
			SetLabelTextIfChanged(_runtimeHudLabel, text, ref _lastHudText);
		}

		private void RefreshStoreHudDetails(Money liveDailyProfit)
		{
			if (_game == null)
				return;

			if (_runtimeTimeOfDayLabel != null)
				_runtimeTimeOfDayLabel.Text = DescribeShopPhaseState();
			if (_runtimeDailyRevenueLabel != null)
				_runtimeDailyRevenueLabel.Text = $"Venit azi {_game.Economy.DailyRevenue} | Profit azi {liveDailyProfit} | Cheltuieli {_game.Economy.DailyExpenses}";
			if (_runtimeSelectedShelfLabel != null)
				_runtimeSelectedShelfLabel.Text = BuildSelectedShelfText();
			if (_runtimeSelectedEmployeeLabel != null)
				_runtimeSelectedEmployeeLabel.Text = BuildSelectedEmployeeText();
			if (_runtimeSelectedCustomerLabel != null)
				_runtimeSelectedCustomerLabel.Text = BuildSelectedCustomerText();
			if (_runtimeStockWarningsLabel != null)
				_runtimeStockWarningsLabel.Text = BuildStockWarningsText();
		}

		private string BuildSelectedShelfText()
		{
			if (_game == null || _runtimeShelfPicker == null || _runtimeShelfPicker.GetItemCount() == 0 || _runtimeShelfPicker.Selected < 0)
				return "Raft selectat: -";

			int shelfId = _runtimeShelfPicker.GetItemId(_runtimeShelfPicker.Selected);
			var shelf = _game.Inventory.GetShelf(shelfId);
			if (shelf == null)
				return "Raft selectat: -";

			var product = _game.Inventory.GetProduct(shelf.ProductId);
			string productName = product == null ? $"Produs #{shelf.ProductId}" : ProductName(product);
			return $"Raft selectat: #{shelf.Id} {Localizer.Shelf(shelf.DisplayType)} | {productName} | {shelf.CurrentStock}/{shelf.Capacity}";
		}

		private string BuildSelectedEmployeeText()
		{
			if (_game == null || _runtimeEmployeePicker == null || _runtimeEmployeePicker.GetItemCount() == 0 || _runtimeEmployeePicker.Selected < 0)
				return "Angajat selectat: -";

			int index = _runtimeEmployeePicker.GetItemId(_runtimeEmployeePicker.Selected);
			if (index < 0 || index >= _game.Employees.Employees.Count)
				return "Angajat selectat: -";

			var employee = _game.Employees.Employees[index];
			return $"Angajat selectat: {employee.Name} | {employee.Role} | moral {employee.Morale} | eficiență {employee.Efficiency}";
		}

		private string BuildSelectedCustomerText()
		{
			if (_game == null)
				return "Client selectat: -";

			if (_game.CurrentPhase != DayPhase.Business)
				return "Client selectat: -";

			return $"Client selectat: flux activ | coadă {_game.CurrentQueueLength} | serviți {_game.CurrentCustomersServedToday} | pierduți {_game.CurrentCustomersLostToday}";
		}

		private string BuildStockWarningsText()
		{
			if (_game == null)
				return "Avertizări stoc: nu există date.";

			var warnings = new List<string>();
			foreach (var product in VisibleProducts())
			{
				int shelfStock = GetShelfStock(product.Id);
				int shelfCapacity = GetShelfCapacity(product.Id);
				if (shelfCapacity <= 0)
					continue;

				if (shelfStock <= 0)
					warnings.Add($"{ProductName(product)} raft gol");
				else if (shelfStock <= Math.Max(1, shelfCapacity / 4))
					warnings.Add($"{ProductName(product)} raft scăzut");
				else if (product.Quantity <= Math.Max(1, shelfCapacity / 3))
					warnings.Add($"{ProductName(product)} depozit scăzut");
			}

			if (_game.Inventory.FreeStorageUnits < 10)
				warnings.Add("depozit aproape plin");
			if (_game.Inventory.PendingOrders.Count > 0)
				warnings.Add($"{_game.Inventory.PendingOrders.Count} livrări în așteptare");

			return warnings.Count == 0 ? "Avertizări stoc: niciuna" : "Avertizări stoc: " + string.Join(", ", warnings.Take(5));
		}

		private static string ShortPhaseName(DayPhase phase)
		{
			return phase switch
			{
				DayPhase.Management => "Adm.",
				DayPhase.Business => "Vânz.",
				DayPhase.Closing => "Înch.",
				DayPhase.Morning => "Dim.",
				_ => phase.ToString()
			};
		}

		private void SetBusinessProgressValue(double value)
		{
			if (_runtimeBusinessProgressBar == null || Math.Abs(_lastBusinessProgressValue - value) < 0.5)
				return;

			_runtimeBusinessProgressBar.Value = value;
			_lastBusinessProgressValue = value;
		}

		private static void SetLabelTextIfChanged(Label label, string text, ref string cachedText)
		{
			if (cachedText == text)
				return;

			label.Text = text;
			cachedText = text;
		}

		private string BuildPhaseHint()
		{
			if (_game == null)
				return "";

			if (IsTutorialActive())
				return GetTutorialPhaseHint();

			if (!_game.IsLoopActive)
				return $"Sesiune încheiată: {DescribeLoopResult(_game.CurrentLoopResult)}. Citește raportul sau salvează înainte de reset.";

			return _game.CurrentPhase switch
			{
				DayPhase.Management => BuildManagementHint(),
				DayPhase.Business => $"Magazinul este deschis. Urmărește coada, stocul și numerarul. Progres: {GetBusinessProgressPercent()}%.",
				DayPhase.Closing => "Ziua s-a încheiat. Deschide raportul, compară profitul și pregătește următoarea zi.",
				DayPhase.Morning => "Dimineața: verifică livrările, taxele și evenimentele înainte să treci la Administrare.",
				_ => ""
			};
		}

		private string GetTutorialPhaseHint()
		{
			return _tutorialStep switch
			{
				TutorialStep.StockShelf => "Tutorial: aprovizionează raftul din depozit.",
				TutorialStep.SetPrice => "Tutorial: setează prețul unui produs înainte să deschizi magazinul.",
				TutorialStep.OpenStore => "Tutorial: deschide magazinul când primul raft are produse.",
				TutorialStep.ObserveCustomers => "Tutorial: clienții intră prin ușă și caută produse pe rafturi.",
				TutorialStep.CashierQueue => "Tutorial: după cumpărături, clienții așteaptă la casă.",
				TutorialStep.MoneyEarned => "Tutorial: numerarul crește din vânzări și scade din costuri.",
				TutorialStep.Reputation => "Tutorial: reputația scade din rafturi goale sau așteptare lungă.",
				TutorialStep.CurrentObjective => GetCurrentObjectiveText(),
				TutorialStep.ReviewReport => "Tutorial: raportul îți explică ziua.",
				TutorialStep.Complete => "Tutorial: continuă cu sarcinile rapide.",
				_ => ""
			};
		}

		private void ResetDailyTaskStateIfNeeded()
		{
			if (_game == null || _lastTaskResetDay == _game.CurrentDay)
				return;

			_lastTaskResetDay = _game.CurrentDay;
			ResetTaskState();
		}

		private void ResetTaskState()
		{
			_pricesChecked = false;
			_stockChecked = false;
			_ordersPlaced = false;
			_staffChecked = false;
			_staffChanged = false;
			_eventsChecked = false;
			_reportsViewed = false;
		}

		private string BuildNextActionGuidance(DailyReport report)
		{
			if (_game == null)
				return "";

			if (IsTutorialActive())
				return "Tutorial activ:\n" + GetTutorialTaskSummary();

			var currentObjective = _game.GetCurrentOnboardingObjective();
			string guidedPrefix = currentObjective != null
				? $"Obiectiv: {currentObjective.Objective.TitleRo}\n{currentObjective.Objective.SummaryRo}\n"
				: _game.CurrentDay <= 3
					? $"Obiectiv ghidat: {GetGuidedDayObjectiveSummary()}\n"
					: "";

			if (!_game.IsLoopActive)
				return guidedPrefix + $"Sesiune încheiată: {DescribeLoopResult(_game.CurrentLoopResult)}. Deschide raportul sau salvează progresul.";
			if (_game.CurrentPhase == DayPhase.Business)
				return guidedPrefix + $"Acum: urmărește coada, rafturile și numerarul. Progres business: {GetBusinessProgressPercent()}%.";
			if (_game.CurrentPhase == DayPhase.Closing)
				return guidedPrefix + $"Acum: citește raportul zilei. Profitul de azi este {report.NetProfit}.";
			if (_game.CurrentPhase != DayPhase.Management)
				return guidedPrefix + "Acum: verifică livrările, taxele și evenimentele înainte de administrare.";

			var priorities = new List<string>();
			if (_game.CurrentDecision != null)
				priorities.Add("Rezolvă evenimentul activ.");
			if (currentObjective?.Objective.Id == "stock_first_shelf")
				priorities.Add("Deschide Comenzi și apasă Realimentează.");
			if (currentObjective?.Objective.Id == "set_first_price")
				priorities.Add("Deschide Prețuri, verifică produsul selectat și confirmă un preț valid.");
			else if (HasLowShelfStock())
				priorities.Add("Realimentează produsele aproape epuizate.");
			if (currentObjective?.Objective.Id == "serve_first_customer")
				priorities.Add("Apasă Deschide și urmărește primul client până la casă.");
			if (currentObjective?.Objective.Id == "hire_first_worker")
				priorities.Add("Deschide Personal și angajează un lucrător potrivit.");
			if (currentObjective?.Objective.Id == "buy_first_shelf")
				priorities.Add("Deschide Comenzi și cumpără un raft nou.");
			if (HasLowShelfStock())
				priorities.Add("Verifică rafturile înainte să deschizi magazinul.");
			if (_game.Inventory.FreeStorageUnits < 10)
				priorities.Add("Evită comenzile mari; depozitul este aproape plin.");
			if (_game.Employees.CountRole(EmployeeRole.Cashier) <= 0)
				priorities.Add("Angajează un casier înainte de deschidere.");

			if (priorities.Count == 0)
				priorities.Add("Verifică prețurile, stocul, personalul și apoi deschide magazinul.");

			var numberedPriorities = priorities
				.Distinct()
				.Select((priority, index) => $"{index + 1}. {priority}");

			return guidedPrefix + "Următorul pas:\n" + string.Join("\n", numberedPriorities);
		}

		private static string CheckMark(bool done) => done ? "[x]" : "[ ]";

		private string BuildManagementHint()
		{
			if (_game == null)
				return "";
			if (_game.CurrentDecision != null)
				return "Rezolvă evenimentul activ înainte de deschidere. Apoi verifică dacă prețurile și stocul sunt încă bune.";
			if (_game.GetCurrentOnboardingObjective()?.Objective.Id == "set_first_price")
				return "Setează un preț pentru produs înainte să vinzi. Deschide Prețuri și confirmă un preț valid.";
			if (HasLowShelfStock())
				return "Raftul este gol sau aproape gol. Aprovizionează-l pentru a atrage clienți.";
			if (_game.Inventory.FreeStorageUnits < 10)
				return "Depozitul este aproape plin. Prioritizează vânzarea și evită comenzile mari.";
			if (_game.Employees.Candidates.Count > 0 && _game.Employees.CountRole(EmployeeRole.Cashier) == 0)
				return "Nu ai casier. Angajează unul înainte de deschidere ca să reduci pierderile la coadă.";

			return "Verifică prețurile, stocul, personalul și apoi deschide magazinul.";
		}

		private string BuildAlertSummary()
		{
			if (_game == null)
				return "Alerte: nu există date.";

			var alerts = new List<string>();
			if (_game.CurrentDecision != null || (_game.CurrentEvent != null && _game.CurrentEvent.IsActive))
				alerts.Add("eveniment activ");
			if (HasLowShelfStock())
				alerts.Add("Raftul este gol. Aprovizionează-l pentru a atrage clienți.");
			if (_game.CurrentPhase == DayPhase.Business && _game.CurrentQueueLength >= 4)
				alerts.Add("Reputația poate scădea: clienții așteaptă prea mult la casă.");
			if (_game.Inventory.PendingOrders.Count > 0)
				alerts.Add("livrări în așteptare");
			if (_game.Inventory.FreeStorageUnits < 10)
				alerts.Add("depozit aproape plin");
			if (_game.Economy.NextTaxDueDay - _game.CurrentDay <= 1)
				alerts.Add("TVA aproape de plată");
			if (_game.Employees.CountRole(EmployeeRole.Cashier) <= 0)
				alerts.Add("Nu ai casier disponibil.");
			if (_game.CurrentPhase == DayPhase.Management && _game.Employees.CountRole(EmployeeRole.Stocker) > 0 && !HasLowShelfStock())
				alerts.Add("Stocarii nu au sarcină urgentă.");
			if (_game.Employees.AverageMorale <= 45)
				alerts.Add("moral scăzut");
			if (_game.Customers.Reputation <= 40)
				alerts.Add("Reputația este scăzută: verifică rafturile și coada.");

			return alerts.Count == 0 ? "Alerte: niciuna" : $"Alerte ({alerts.Count}): " + string.Join(", ", alerts);
		}

		private string BuildDefaultContextText()
		{
			if (_game == null)
				return "Pornește sau încarcă un joc pentru detalii.";

			return _game.CurrentPhase switch
			{
				DayPhase.Management => "Administrare: selectează rafturi, depozit, livrare sau casa de marcat pentru comenzi, prețuri și personal.",
				DayPhase.Business => $"Vânzare: coadă {_game.CurrentQueueLength}, clienți serviți {_game.CurrentCustomersServedToday}, pierduți {_game.CurrentCustomersLostToday}.",
				DayPhase.Closing => $"Închidere: profit {_game.LastDailyReport.NetProfit}. {BuildReportRecommendation(_game.LastDailyReport)}",
				_ => "Dimineață: verifică livrările, taxele și evenimentele înainte de administrare."
			};
		}

		private string BuildPhaseTransitionMessage(DayPhase previousPhase, DayPhase currentPhase, DailyReport report)
		{
			return currentPhase switch
			{
				DayPhase.Management => "A început faza de Administrare. Recomandat: verifică livrările, rafturile, prețurile și personalul înainte să deschizi magazinul.",
				DayPhase.Business => "A început faza de Vânzare. Urmărește coada, stocul și numerarul în timp real.",
				DayPhase.Closing => $"A început faza de Închidere. Profitul zilei este {report.NetProfit}. Deschide raportul ca să vezi cauzele și următorii pași.",
				DayPhase.Morning => $"Dimineață nouă: ziua {(_game?.CurrentDay ?? report.Day)} începe. Verifică livrările și evenimentele înainte de administrare.",
				_ => $"{PhaseName(previousPhase)} -> {PhaseName(currentPhase)}"
			};
		}

		private void SetContextPanel(string title, string body)
		{
			if (_rightContextTitleLabel == null)
				_rightContextTitleLabel = FindNodeRecursive("RightContextTitle", this) as Label;
			if (_rightContextTitleLabel != null)
				_rightContextTitleLabel.Text = title;
			if (_rightContextLabel != null)
				_rightContextLabel.Text = body;
			if (_rightContextPanel != null)
				_rightContextPanel.Visible = true;
		}

		private int GetBusinessProgressPercent()
		{
			if (_game == null || _game.BusinessTicksPerDayTotal <= 0)
				return 0;

			int elapsed = Math.Max(0, _game.BusinessTicksPerDayTotal - _game.BusinessTicksRemaining);
			return Math.Clamp(elapsed * 100 / _game.BusinessTicksPerDayTotal, 0, 100);
		}

		private void RefreshEventDecisionPanel()
		{
			if (_game == null)
				return;

			var decision = _game.CurrentDecision;
			bool canResolve = _game.IsLoopActive && _game.CurrentPhase == DayPhase.Management && decision != null;
			if (_runtimeEventLabel != null)
			{
				if (decision == null)
				{
					if (_game.CurrentEvent != null && _game.CurrentEvent.IsActive)
					{
						_runtimeEventLabel.Text =
							$"Eveniment curent: {EventName(_game.CurrentEvent.Type)}\n" +
							$"Ce s-a întâmplat: {DescribeEventImpact(_game.CurrentEvent.Type)}\n" +
							$"De ce contează: {DescribeEventWhy(_game.CurrentEvent.Type)}\n" +
							$"Risc: {DescribeEventRisk(_game.CurrentEvent.Type)}\n" +
							$"Recompensă posibilă: {DescribeEventReward(_game.CurrentEvent.Type)}";
					}
					else
					{
						_runtimeEventLabel.Text = "Eveniment curent: niciunul\nAcum: nu ai un eveniment activ.";
					}
				}
				else
				{
					_runtimeEventLabel.Text =
						$"{Localizer.EventPrompt(decision.Type)}\n" +
						$"Ce s-a întâmplat: {DescribeEventImpact(decision.Type)}\n" +
						$"De ce contează: {DescribeEventWhy(decision.Type)}\n" +
						$"Risc: {DescribeEventRisk(decision.Type)}\n" +
						$"Recompensă posibilă: {DescribeEventReward(decision.Type)}\n" +
						$"Opțiunea A: {DescribeEventChoice(decision.Type, 0)}\n" +
						$"Opțiunea B: {DescribeEventChoice(decision.Type, 1)}";
				}
			}

			if (_runtimeEventOptionAButton != null)
			{
				_runtimeEventOptionAButton.Text = decision == null ? "Opțiunea A" : $"A: {Localizer.EventOptionA(decision.Type)}";
				_runtimeEventOptionAButton.Disabled = !canResolve;
			}

			if (_runtimeEventOptionBButton != null)
			{
				_runtimeEventOptionBButton.Text = decision == null ? "Opțiunea B" : $"B: {Localizer.EventOptionB(decision.Type)}";
				_runtimeEventOptionBButton.Disabled = !canResolve;
			}

			if (_runtimeTriggerEventButton != null)
			{
				_runtimeTriggerEventButton.Visible = EnableLayoutDebug;
				_runtimeTriggerEventButton.Disabled = !EnableLayoutDebug || !(_game.IsLoopActive && _game.CurrentPhase == DayPhase.Management && decision == null);
			}
		}

		private void NotifySimulationFeedback()
		{
			if (_game == null)
				return;

			var messages = new List<string>();
			if (_game.LastCheckoutFeedbackSequence > _lastCheckoutFeedbackSequence)
			{
				_lastCheckoutFeedbackSequence = _game.LastCheckoutFeedbackSequence;
				if (!string.IsNullOrWhiteSpace(_game.LastCheckoutFeedbackRo))
					messages.Add(_game.LastCheckoutFeedbackRo);
			}

			if (_game.LastReputationFeedbackSequence > _lastReputationFeedbackSequence)
			{
				_lastReputationFeedbackSequence = _game.LastReputationFeedbackSequence;
				if (!string.IsNullOrWhiteSpace(_game.LastReputationFeedbackRo))
					messages.Add(_game.LastReputationFeedbackRo);
			}

			if (messages.Count > 0)
				Notify(string.Join("\n", messages));
		}

		private string BuildShelfSummary()
		{
			if (_game == null)
				return "Nu există date despre rafturi.";

			var builder = new StringBuilder();
			foreach (var product in VisibleProducts())
			{
				int shelfStock = GetShelfStock(product.Id);
				int shelfCapacity = GetShelfCapacity(product.Id);
				int daysUntilStockout = EstimateDaysUntilStockout(product);
				string health = shelfStock <= 0
					? "Critic"
					: shelfStock <= Math.Max(1, shelfCapacity / 4)
						? "Avertizare"
						: product.Quantity > Math.Max(12, shelfCapacity * 2) && daysUntilStockout >= 8
							? "Bine (suprastoc)"
							: "Bine";
				string badge = product.Popularity >= 75 || _game.GetProductDemandBasisPoints(product) >= 13_000
					? "best seller"
					: daysUntilStockout >= 10 && product.Quantity > Math.Max(12, shelfCapacity)
						? "suprastoc"
						: DescribeShelfState(shelfStock, shelfCapacity);
				builder.AppendLine($"{health}: {ProductName(product)} | raft {shelfStock}/{shelfCapacity} | depozit {product.Quantity} | cerere {DescribeDemand(product)} | {badge} | epuizare ~{daysUntilStockout} zile");
			}

			builder.AppendLine($"{T("LABEL_STORAGE")} total: {_game.Inventory.TotalStorageUnits}/{_game.Inventory.StorageCapacity}");
			return builder.ToString().TrimEnd();
		}

		private string BuildRuntimeReport(DailyReport report, DailyReport? previousReport)
		{
			if (_game == null)
				return "Nu există stare de joc.";

			var builder = new StringBuilder();
			Money runProfit = _game.RunProfit;
			if (!_game.IsLoopActive)
			{
				builder.AppendLine(T("REPORT_RUN_ENDED"));
				builder.AppendLine($"{_game.CurrentLoopResult.Status}: {_game.CurrentLoopResult.Reason}");
				builder.AppendLine($"{T("LABEL_CASH")} {_game.Economy.Cash} | Profit campanie {runProfit} | Medalie {_game.GetProfitMedalName(runProfit)}");
				builder.AppendLine();
			}

			int bestSellerId = report.BestSellerProductId != 0 ? report.BestSellerProductId : GetFallbackBestSellerId();
			int worstSellerId = report.WorstSellerProductId != 0 ? report.WorstSellerProductId : GetFallbackWorstSellerId();
			string comparison = previousReport == null
				? "primul raport"
				: $"profit {FormatSignedMoney(report.NetProfit - previousReport.NetProfit)}, venit {FormatSignedMoney(report.Revenue - previousReport.Revenue)}, cheltuieli {FormatSignedMoney(report.Expenses - previousReport.Expenses)}";

			builder.AppendLine($"{T("REPORT_DAY")} {report.Day}/{_game.LoopDayLimit} | {PhaseName(_game.CurrentPhase)} | {MoodName(report.EconomicMood)}");
			builder.AppendLine($"Obiectiv: profit campanie {runProfit} | Bronze {_game.BronzeProfitTarget} | Silver {_game.SilverProfitTarget} | Gold {_game.GoldProfitTarget}");
			builder.AppendLine($"Venit: {report.GrossRevenue} | Cheltuieli: {report.Expenses} | Profit: {report.NetProfit}");
			builder.AppendLine($"Comparație: {comparison}.");
			builder.AppendLine($"Best seller: {ProductNameOrFallback(bestSellerId)}");
			builder.AppendLine($"Worst seller: {ProductNameOrFallback(worstSellerId)}");
			builder.AppendLine($"Recomandare: {BuildReportRecommendation(report)}");
			builder.AppendLine($"Motiv principal: {DescribeMainProblem(report)}; {DescribeProfitCause(report)}.");
			builder.AppendLine($"{T("LABEL_REPUTATION")}: {report.Reputation} | Clienți pierduți: {report.LostCheckoutCustomers + report.LostStockoutCustomers} | Zile rămase: {_game.DaysRemaining}");
			if (report.EventDescription != "None")
				builder.AppendLine($"{T("BTN_EVENT")}: {EventDescription(_game.CurrentEvent)}");
			if (report.SupportEventEffects != "None")
				builder.AppendLine($"{T("LABEL_SUPPORT")}: {Localizer.SupportEffect(report.SupportEventEffects)}");

			if (_showAdvancedReport)
				AppendAdvancedReportDetails(builder, report);
			else
				builder.AppendLine("Pentru taxe, rafturi, personal și livrări, deschide Detalii.");

			return builder.ToString();
		}

		private void AppendAdvancedReportDetails(StringBuilder builder, DailyReport report)
		{
			if (_game == null)
				return;

			var operations = _game.Employees.GetOperations();
			builder.AppendLine();
			builder.AppendLine("DETALII AVANSATE");
			builder.AppendLine($"Numerar: {report.StartingCash} -> {report.EndingCash} ({FormatSignedMoney(report.EndingCash - report.StartingCash)}). Cauză: {DescribeCashCause(report)}");
			builder.AppendLine($"Costuri: salarii {report.Payroll} | furnizori {report.SupplierCosts} | chirie {report.Rent} | utilități {report.Utilities} | taxe {report.BusinessTax}");
			builder.AppendLine($"TVA acumulat {report.VatAccrued} | TVA plătit {report.VatPaid} | TVA de plată {_game.Economy.VatOwed}");
			builder.AppendLine($"Vândute {report.UnitsSold} | epuizări {report.Stockouts} | realimentate {report.RestockedUnits} | depozit {report.StorageUsed}/{report.StorageCapacity}");
			builder.AppendLine($"Personal: casă {_game.CheckoutCapacity}/val | rafturi {_game.DailyRestockCapacity}/zi | moral {_game.Employees.AverageMorale}");
			builder.AppendLine($"{T("LABEL_SHOP_UPGRADES")}: decor {_game.DecorationLevel} | hardware {_game.HardwareLevel}");

			builder.AppendLine();
			builder.AppendLine(T("LABEL_PRODUCTS").ToUpperInvariant());
			foreach (var product in VisibleProducts())
			{
				int shelfStock = GetShelfStock(product.Id);
				int shelfCapacity = GetShelfCapacity(product.Id);
				string status = shelfStock <= 0 ? T("STATUS_EMPTY") : shelfStock <= Math.Max(1, shelfCapacity / 4) ? T("STATUS_LOW") : T("STATUS_OK");
				builder.AppendLine($"{ProductName(product)}: {status} | raft {shelfStock}/{shelfCapacity} | depozit {product.Quantity} | preț {product.SalePrice}");
			}

			builder.AppendLine();
			builder.AppendLine($"{T("LABEL_ORDERS").ToUpperInvariant()} | spațiu liber {_game.Inventory.FreeStorageUnits}");
			if (_game.Inventory.PendingOrders.Count == 0)
				builder.AppendLine(T("REPORT_NO_PENDING_ORDERS"));
			else
			{
				foreach (var order in _game.Inventory.PendingOrders)
				{
					var product = _game.Inventory.GetProduct(order.ProductId);
					builder.AppendLine($"{(product == null ? $"Produs {order.ProductId}" : ProductName(product))} x{order.Quantity}, sosește în {order.RemainingDays}z");
				}
			}

			builder.AppendLine();
			builder.AppendLine(T("LABEL_NEXT_MOVES").ToUpperInvariant());
			AppendReportAdvice(builder, report, operations);
		}

		private string ProductNameOrFallback(int productId)
		{
			if (_game == null || productId <= 0)
				return "nedeterminat";

			var product = _game.Inventory.GetProduct(productId);
			return product == null ? $"Produs #{productId}" : ProductName(product);
		}

		private int GetFallbackBestSellerId()
		{
			return VisibleProducts()
				.OrderByDescending(product => product.Popularity)
				.ThenBy(product => product.Id)
				.FirstOrDefault()?.Id ?? 0;
		}

		private int GetFallbackWorstSellerId()
		{
			return VisibleProducts()
				.OrderBy(product => EstimateDaysUntilStockout(product))
				.ThenBy(product => product.Popularity)
				.ThenBy(product => product.Id)
				.FirstOrDefault()?.Id ?? 0;
		}

		private string DescribeMainProblem(DailyReport report)
		{
			if (report.StorageOverflowUnits > 0)
				return "depozit depășit";
			if (report.LostCheckoutCustomers > report.LostStockoutCustomers && report.LostCheckoutCustomers > 0)
				return "coadă la casă";
			if (report.LostStockoutCustomers > 0 || report.Stockouts > 0)
				return "rafturi goale";
			if (report.Profit < Money.Zero)
				return "profit negativ";
			if (_game != null && _game.Inventory.FreeStorageUnits < 10)
				return "depozit aproape plin";

			return "niciuna";
		}

		private string BuildReportRecommendation(DailyReport report)
		{
			if (report.LostStockoutCustomers > 0 || report.Stockouts > 0)
				return "Verifică comenzile pentru produsele epuizate.";
			if (report.Reputation < 45 || report.CustomerSatisfaction < 45)
				return "Analizează reclamațiile sau prețurile.";
			if (report.LostCheckoutCustomers > 0)
				return "Verifică personalul de la casă.";
			if (report.Profit < Money.Zero || report.NetProfit < Money.Zero)
				return "Revizuiește prețurile și costurile.";
			if (report.StorageOverflowUnits > 0)
				return "Redu comenzile sau mărește depozitul.";

			return "Pregătește stocul și prețurile pentru următoarea zi.";
		}

		private void TrackCompletedReport()
		{
			if (_game == null)
				return;

			var report = _game.LastDailyReport;
			if (!IsCompletedReport(report) || _lastCompletedReportDay == report.Day)
				return;

			_previousCompletedReport = _lastCompletedReport;
			_lastCompletedReport = report;
			_lastCompletedReportDay = report.Day;
		}

		private DailyReport GetDisplayReport()
		{
			if (_game == null)
				return DailyReport.Empty(1, Money.Zero);

			var current = _game.LastDailyReport;
			return IsCompletedReport(current) ? current : _lastCompletedReport ?? current;
		}

		private static bool IsCompletedReport(DailyReport report)
		{
			return report.Revenue != Money.Zero
				|| report.Expenses != Money.Zero
				|| report.Profit != Money.Zero
				|| report.UnitsSold > 0
				|| report.Stockouts > 0
				|| report.RestockedUnits > 0
				|| report.StorageOverflowUnits > 0
				|| report.LostCheckoutCustomers > 0
				|| report.LostStockoutCustomers > 0
				|| report.CustomersServed > 0
				|| report.QueuePressure > 0
				|| report.EventDescription != "None"
				|| report.SupportEventEffects != "None";
		}

		private int GetAlertCount()
		{
			if (_game == null)
				return 0;

			int count = 0;
			if (_game.CurrentDecision != null || (_game.CurrentEvent != null && _game.CurrentEvent.IsActive))
				count++;
			if (HasLowShelfStock())
				count++;
			if (_game.Inventory.FreeStorageUnits < 10)
				count++;
			if (_game.Economy.NextTaxDueDay - _game.CurrentDay <= 1)
				count++;
			if (_game.Employees.CountRole(EmployeeRole.Cashier) <= 0)
				count++;
			if (_game.Customers.Reputation <= 40)
				count++;
			return count;
		}

		private static string FormatSignedMoney(Money value)
		{
			return value >= Money.Zero ? $"+{value}" : value.ToString();
		}

		private static string FormatSignedInt(int value)
		{
			return value >= 0 ? $"+{value}" : value.ToString();
		}

		private string DescribeLoopResult(LoopResult result)
		{
			return $"{result.Status}: {result.Reason}";
		}

		private string DescribeCashCause(DailyReport report)
		{
			if (report.Profit > Money.Zero)
				return "vânzările au depășit costurile";
			if (report.Profit < Money.Zero)
				return "costurile au fost mai mari decât veniturile";
			return "veniturile și cheltuielile s-au echilibrat";
		}

		private string DescribeProfitCause(DailyReport report)
		{
			if (report.Stockouts > 0 || report.LostStockoutCustomers > 0)
				return "stocul a limitat vânzările";
			if (report.LostCheckoutCustomers > 0)
				return "coada la casă a blocat vânzările";
			if (report.EventDescription != "None")
				return "evenimentul zilei a influențat rezultatul";
			if (report.SupplierCosts > report.Payroll && report.SupplierCosts > report.Rent)
				return "costurile furnizorilor au fost ridicate";
			if (report.Revenue > report.Expenses)
				return "cererea a fost mai bună decât costurile";
			return "cheltuielile au crescut mai repede decât veniturile";
		}

		private string DescribeExpenseCause(DailyReport report)
		{
			if (report.Payroll >= report.SupplierCosts && report.Payroll >= report.Rent && report.Payroll >= report.Utilities)
				return "salariile au fost cea mai mare cheltuială";
			if (report.SupplierCosts >= report.Payroll && report.SupplierCosts >= report.Rent && report.SupplierCosts >= report.Utilities)
				return "comenzile și livrările au costat cel mai mult";
			if (report.Rent >= report.Payroll && report.Rent >= report.SupplierCosts)
				return "chiriei i-a revenit o pondere mare";
			return "taxele și utilitățile au împins costurile în sus";
		}

		private string DescribeEventImpact(GameEventType type)
		{
			return type switch
			{
				GameEventType.DemandSpike => "cererea a crescut și rafturile se golesc mai repede",
				GameEventType.SupplierDelay => "livrările întârzie și riști rafturi goale",
				GameEventType.ReputationCrisis => "încrederea clienților scade",
				GameEventType.EmployeeStrike => "moralul personalului scade și apar probleme de capacitate",
				GameEventType.HolidayRush => "vin mai mulți clienți decât de obicei",
				GameEventType.PriceFluctuation => "prețurile se mișcă și marjele devin mai instabile",
				GameEventType.ImportPressure => "costurile de aprovizionare cresc",
				GameEventType.DefectiveProductComplaint => "o reclamație poate afecta reputația",
				GameEventType.RefundRequest => "o cerere de rambursare poate costa bani sau reputație",
				GameEventType.WarrantyDispute => "relația cu furnizorul și încrederea clienților sunt sub presiune",
				_ => "situația schimbă cererea, costurile sau reputația"
			};
		}

		private string DescribeEventWhy(GameEventType type)
		{
			return type switch
			{
				GameEventType.DemandSpike => "Ai nevoie de stoc și de casieri suficienți ca să nu pierzi vânzări.",
				GameEventType.SupplierDelay => "Dacă marfa nu ajunge, rafturile rămân goale și pierzi bani.",
				GameEventType.ReputationCrisis => "Reputația slabă reduce încrederea și vânzările viitoare.",
				GameEventType.EmployeeStrike => "Moralul afectează viteza și eficiența personalului.",
				GameEventType.HolidayRush => "Poți câștiga mai mult, dar doar dacă ești pregătit pentru coadă.",
				GameEventType.PriceFluctuation => "Marjele se schimbă și trebuie să urmărești prețurile mai atent.",
				GameEventType.ImportPressure => "Costurile cresc repede și pot mânca profitul.",
				GameEventType.DefectiveProductComplaint => "O reclamație neclară poate costa atât bani, cât și reputație.",
				GameEventType.RefundRequest => "Decizia arată cât de bine îți protejezi clientul.",
				GameEventType.WarrantyDispute => "Rezolvi sau amâni problema și asta se vede în reputație și costuri.",
				_ => "Evenimentul poate afecta banii, reputația sau stocul."
			};
		}

		private string DescribeEventRisk(GameEventType type)
		{
			return type switch
			{
				GameEventType.DemandSpike => "rafturi goale dacă nu ai stoc suficient",
				GameEventType.SupplierDelay => "livrări întârziate și vânzări pierdute",
				GameEventType.ReputationCrisis => "cerere mai slabă în zilele următoare",
				GameEventType.EmployeeStrike => "cozi mai mari și realimentare mai lentă",
				GameEventType.HolidayRush => "clienți pierduți dacă personalul nu ține pasul",
				GameEventType.PriceFluctuation => "marje instabile și prețuri nepotrivite",
				GameEventType.ImportPressure => "costuri mai mari pentru produse sensibile la import",
				GameEventType.DefectiveProductComplaint => "bani pierduți sau reputație afectată",
				GameEventType.RefundRequest => "cost imediat sau client nemulțumit",
				GameEventType.WarrantyDispute => "furnizor mai slab sau încredere mai mică",
				_ => "costuri, reputație sau stoc afectate"
			};
		}

		private string DescribeEventReward(GameEventType type)
		{
			return type switch
			{
				GameEventType.DemandSpike => "venit suplimentar dacă rafturile rămân pline",
				GameEventType.SupplierDelay => "poți proteja stocul alegând transport rapid",
				GameEventType.ReputationCrisis => "reputație recuperată prin suport vizibil",
				GameEventType.EmployeeStrike => "moral mai bun dacă investești în echipă",
				GameEventType.HolidayRush => "vânzări mari într-o zi aglomerată",
				GameEventType.PriceFluctuation => "marje mai bune dacă ajustezi prețurile",
				GameEventType.ImportPressure => "profit protejat prin decizie rapidă",
				GameEventType.DefectiveProductComplaint => "încredere păstrată prin rezolvare clară",
				GameEventType.RefundRequest => "satisfacție mai bună dacă tratezi clientul corect",
				GameEventType.WarrantyDispute => "furnizor controlat și reputație protejată",
				_ => "șansa de a transforma riscul în profit"
			};
		}

		private string DescribeEventChoice(GameEventType type, int option)
		{
			return type switch
			{
				GameEventType.DemandSpike when option == 0 => "faci o promoție mică și câștigi volum, dar cheltui bani",
				GameEventType.DemandSpike => "păstrezi cash-ul, dar riști să pierzi o parte din reputație",
				GameEventType.SupplierDelay when option == 0 => "plătești transport prioritar ca să grăbești livrarea",
				GameEventType.SupplierDelay => "aștepți livrarea și accepți rafturi goale mai mult timp",
				GameEventType.ReputationCrisis when option == 0 => "oferi rambursări și repari încrederea, dar plătești mai mult",
				GameEventType.ReputationCrisis => "ignori problema și lași reputația să scadă mai tare",
				GameEventType.EmployeeStrike when option == 0 => "oferi un bonus și refaci moralul, dar crești costurile",
				GameEventType.EmployeeStrike => "încerci să mergi mai departe și riști productivitate slabă",
				GameEventType.HolidayRush when option == 0 => "pregătești service suplimentar și reduci pierderile la coadă",
				GameEventType.HolidayRush => "economisești cash, dar poți pierde clienți din coadă",
				GameEventType.ImportPressure when option == 0 => "absorbi costul și protejezi prețurile pentru clienți",
				GameEventType.ImportPressure => "urci prețurile mai târziu și transferi presiunea în vânzare",
				GameEventType.DefectiveProductComplaint when option == 0 => "faci rambursare completă și protejezi reputația",
				GameEventType.DefectiveProductComplaint => "oferi credit în magazin și păstrezi o parte din valoare",
				GameEventType.RefundRequest when option == 0 => "aprobi returul și păstrezi încrederea clientului",
				GameEventType.RefundRequest => "refuzi și riști să pierzi reputație",
				GameEventType.WarrantyDispute when option == 0 => "investighezi furnizorul și încerci să rezolvi corect problema",
				GameEventType.WarrantyDispute => "amâni răspunsul și lași problema să se stingă greu",
				_ => "această opțiune schimbă costul, reputația sau stocul"
			};
		}

		private string DescribeDemand(Product product)
		{
			if (_game == null)
				return "necunoscută";

			int demandBasisPoints = _game.GetProductDemandBasisPoints(product);
			return demandBasisPoints switch
			{
				>= 13_000 => "foarte mare",
				>= 11_000 => "mare",
				>= 9_500 => "medie",
				_ => "scăzută"
			};
		}

		private int EstimateDaysUntilStockout(Product product)
		{
			if (_game == null || product.Quantity <= 0)
				return 0;

			int demandBasisPoints = _game.GetProductDemandBasisPoints(product);
			int estimatedDailySales = Math.Max(1, (product.Popularity * demandBasisPoints) / 10_000 / 10);
			return Math.Max(0, (int)Math.Ceiling(product.Quantity / (double)estimatedDailySales));
		}

		private static string DescribeShelfState(int currentStock, int capacity)
		{
			if (currentStock <= 0)
				return "epuizat";
			if (capacity <= 0)
				return "fără capacitate";
			if (currentStock <= Math.Max(1, capacity / 4))
				return "stoc mic";
			if (currentStock >= capacity)
				return "plin";

			return "normal";
		}

		private static string DescribeShopCatalogItem(ShopCatalogItem item)
		{
			return item.Type switch
			{
				ShopCatalogItemType.Shelf => $"raft +{item.ShelfCapacity}",
				ShopCatalogItemType.Storage => $"depozit +{item.StorageCapacityBonus}",
				ShopCatalogItemType.Decoration => $"decor +{item.DecorationBonus}",
				ShopCatalogItemType.Hardware => $"echipament +{item.HardwareBonus}",
				_ => "îmbunătățire"
			};
		}

		private static string CatalogTypeRo(ShopCatalogItemType type)
		{
			return type switch
			{
				ShopCatalogItemType.Shelf => "Raft",
				ShopCatalogItemType.Storage => "Depozit",
				ShopCatalogItemType.Decoration => "Decor",
				ShopCatalogItemType.Hardware => "Echipament",
				_ => "Îmbunătățire"
			};
		}

		private void AppendReportAdvice(StringBuilder builder, DailyReport report, EmployeeOperations operations)
		{
			if (_game == null)
				return;

			bool added = false;
			if (report.LostCheckoutCustomers > report.LostStockoutCustomers && report.LostCheckoutCustomers > 0)
			{
				builder.AppendLine("- Coada la casă este blocajul principal. Angajează un casier sau redu presiunea cererii până crește capacitatea.");
				added = true;
			}

			if (report.LostStockoutCustomers > 0 || report.Stockouts > 0)
			{
				builder.AppendLine("- Rafturile goale pierd vânzări. Comandă produse cu stoc mic și păstrează gestionari disponibili înainte de deschidere.");
				added = true;
			}

			if (report.RestockedUnits < operations.DailyRestockCapacity / 2 && HasLowShelfStock())
			{
				builder.AppendLine("- Depozitul nu este singura problemă; rafturile sunt încă slab alimentate. Adaugă stoc sau reatribuie capacitatea rafturilor.");
				added = true;
			}

			if (report.Profit < Money.Zero)
			{
				builder.AppendLine("- Ziua a pierdut bani. Compară salariile și comenzile cu venitul înainte să angajezi mai mult personal.");
				added = true;
			}

			if (report.StorageOverflowUnits > 0)
			{
				builder.AppendLine("- Livrările au depășit depozitul. Comandă mai puțin sau extinde depozitul înainte de achiziții mari.");
				added = true;
			}

			foreach (var product in VisibleProducts())
			{
				int shelfStock = GetShelfStock(product.Id);
				int shelfCapacity = GetShelfCapacity(product.Id);
				if (shelfCapacity > 0 && shelfStock <= Math.Max(1, shelfCapacity / 4) && product.Quantity <= shelfCapacity / 2)
				{
					builder.AppendLine($"- Realimentează {ProductName(product)}; raftul și depozitul sunt ambele slabe.");
					added = true;
				}
			}

			if (!added)
				builder.AppendLine("- Nu există blocaj critic. Ajustează prețurile sau pregătește stocul pentru următorul val de cerere.");
		}

		private static string DescribeProductRole(int productId)
		{
			return productId switch
			{
				1 => "fast",
				2 => "premium",
				3 => "luxury",
				4 => "handheld",
				5 => "import",
				6 => "arcade",
				7 => "utility",
				8 => "rare",
				_ => "standard"
			};
		}

		private int GetShelfCapacity(int productId)
		{
			if (_game == null)
				return 0;

			int capacity = 0;
			foreach (var shelf in _game.Inventory.GetShelvesForProduct(productId))
				capacity += shelf.Capacity;

			return capacity;
		}

		private bool HasLowShelfStock()
		{
			if (_game == null)
				return false;

			foreach (var product in VisibleProducts())
			{
				int capacity = GetShelfCapacity(product.Id);
				if (capacity > 0 && GetShelfStock(product.Id) <= Math.Max(1, capacity / 4))
					return true;
			}

			return false;
		}

		private void UpdateRuntimePriceInput()
		{
			if (_game == null || _runtimePriceProductPicker == null || _runtimePriceInput == null)
				return;

			int? id = _activePriceProductId ?? GetSelectedProductId(_runtimePriceProductPicker);
			var product = id.HasValue ? _game.Inventory.GetProduct(id.Value) : null;
			if (product != null)
			{
				SelectProductInPicker(_runtimePriceProductPicker, product.Id);
				UpdatePriceTargetLabel(product);
				if (_runtimePriceInput.HasFocus())
					return;

				_runtimePriceInput.Text = product.SalePrice.ToString();
			}
			else
			{
				ClearPriceEditTarget();
			}
		}

		private bool BeginPriceEdit(int? productId, bool focusInput)
		{
			if (_game == null)
				return false;

			int? targetProductId = productId ?? GetSelectedProductId(_runtimePriceProductPicker) ?? _game.Inventory.Products.FirstOrDefault()?.Id;
			var product = targetProductId.HasValue ? _game.Inventory.GetProduct(targetProductId.Value) : null;
			if (product == null)
			{
				ClearPriceEditTarget();
				Notify(T("NOTIFY_NO_PRODUCT"));
				return false;
			}

			_activePriceProductId = product.Id;
			SelectProductInPicker(_runtimePriceProductPicker, product.Id);
			UpdatePriceTargetLabel(product);
			if (_runtimePriceInput != null)
			{
				_runtimePriceInput.Text = product.SalePrice.ToString();
				if (focusInput)
					CallDeferred(nameof(FocusPriceInput));
			}

			return true;
		}

		private void ClearPriceEditTarget()
		{
			_activePriceProductId = null;
			if (_runtimePriceTargetLabel != null)
				_runtimePriceTargetLabel.Text = "Produs selectat: -";
		}

		private void UpdatePriceTargetLabel(Product product)
		{
			if (_runtimePriceTargetLabel != null)
				_runtimePriceTargetLabel.Text = $"Produs selectat: {ProductName(product)}\nPreț curent: {product.SalePrice}. Confirmarea schimbă doar acest produs.";
		}

		private void FocusPriceInput()
		{
			if (_runtimePriceInput == null || _runtimePricesPopup?.Visible != true)
				return;

			_runtimePriceInput.GrabFocus();
			_runtimePriceInput.SelectAll();
		}

		private static int? GetProductIdAt(OptionButton? picker, int index)
		{
			if (picker == null || index < 0 || index >= picker.GetItemCount())
				return null;

			int id = picker.GetItemId(index);
			return id > 0 ? id : null;
		}

		private static int? GetSelectedProductId(OptionButton? picker)
		{
			return GetProductIdAt(picker, picker?.Selected ?? -1);
		}

		private bool SelectProductInPicker(OptionButton? picker, int productId)
		{
			if (picker == null)
				return false;

			for (int i = 0; i < picker.GetItemCount(); i++)
			{
				if (picker.GetItemId(i) == productId)
				{
					_updatingPricePickerSelection = true;
					picker.Selected = i;
					_updatingPricePickerSelection = false;
					return true;
				}
			}

			return false;
		}

		private void OnRuntimeOpenShopPressed()
		{
			if (_game == null)
			{
				GD.PrintErr("UIManager: Open Shop pressed before GameManager initialization.");
				Notify("Sesiunea nu este inițializată. Revino la meniul principal și pornește una nouă.");
				return;
			}

			if (_game.CurrentPhase == DayPhase.Management)
			{
				var warnings = BuildOpenShopWarnings();
				if (warnings.Count > 0)
				{
					ShowOpenShopWarning(warnings);
					UpdateTaskBoxDisplay();
					return;
				}
			}

			if (_game.StartBusiness())
			{
				Notify(T("NOTIFY_SHOP_OPEN"));
				CompleteTutorialStep(TutorialStep.OpenStore);
			}
			else if (!_game.IsLoopActive)
				Notify($"Sesiunea s-a încheiat: {_game.CurrentLoopResult.Reason}");
			else if (_game.CurrentPhase == DayPhase.Business)
				Notify(T("NOTIFY_ALREADY_OPEN"));
			else if (_game.CurrentPhase == DayPhase.Closing)
				Notify(T("NOTIFY_DAY_CLOSED"));
			else
				Notify(T("NOTIFY_CANNOT_OPEN"));

			if (_game.CurrentPhase == DayPhase.Management)
				UpdateTaskBoxDisplay();

			Refresh();
		}

		private List<string> BuildOpenShopWarnings()
		{
			var warnings = new List<string>();
			if (_game == null)
				return warnings;

			if (!_game.Inventory.Shelves.Any(shelf => shelf.CurrentStock > 0))
				warnings.Add("Rafturile sunt goale. Deschide Comenzi și apasă Realimentează înainte de deschidere.");
			if (_game.Employees.CountRole(EmployeeRole.Cashier) <= 0)
				warnings.Add("Nu există casier disponibil.");
			if (_game.CurrentDecision != null)
				warnings.Add("Există o reclamație/eveniment nerezolvat.");
			else if (!_eventsChecked && _game.CurrentEvent != null && _game.CurrentEvent.IsActive)
				warnings.Add("Evenimentul activ nu a fost verificat.");

			return warnings;
		}

		private void ShowOpenShopWarning(List<string> warnings)
		{
			EnsureFlowPopups();
			if (_openShopWarningLabel != null)
				_openShopWarningLabel.Text = string.Join("\n", warnings.Take(3).Select(w => $"- {w}"));
			if (_openShopWarningPopup != null)
			{
				HideAllPopups();
				ShowBoundedPopup(_openShopWarningPopup, new Vector2(430, 250));
			}
			Notify("Magazinul are probleme nerezolvate.");
		}

		private void StartBusinessFromWarning()
		{
			HideOpenShopWarning();
			if (_game != null && _game.StartBusiness())
			{
				Notify(T("NOTIFY_SHOP_OPEN"));
				CompleteTutorialStep(TutorialStep.OpenStore);
			}
			else
				Notify(T("NOTIFY_CANNOT_OPEN"));
			Refresh();
		}

		private void OnRuntimeNextDayPressed()
		{
			if (_game == null)
			{
				GD.PrintErr("UIManager: Next Day pressed before GameManager initialization.");
				Notify("Sesiunea nu este inițializată.");
				return;
			}

			if (_game.CurrentPhase != DayPhase.Closing)
			{
				Notify(_game.CurrentPhase == DayPhase.Business ? T("NOTIFY_STILL_RUNNING") : T("NOTIFY_OPEN_BEFORE_NEXT"));
				return;
			}

			if (!_game.IsLoopActive)
			{
				Notify($"Sesiunea s-a încheiat: {_game.CurrentLoopResult.Reason}");
				Refresh();
				return;
			}

			_game.AdvanceToNextDay();
			Notify(TF("NOTIFY_STARTED_DAY", _game.CurrentDay));
			Refresh();
		}

		private void OnRuntimeResetPressed()
		{
			if (_game == null)
			{
				GD.PrintErr("UIManager: Reset pressed before GameManager initialization.");
				Notify("Nu există o sesiune activă de resetat.");
				return;
			}

			ShowConfirmation(
				"Confirmă resetarea",
				"Sesiunea curentă va reveni la ziua 1. Salvările existente nu sunt șterse.",
				"Resetează",
				() =>
				{
					_game?.ResetTick();
					HideAllPopups();
					ClearPriceEditTarget();
					InvalidateShopNavigation();
					Notify(T("NOTIFY_LOOP_RESET"));
					Refresh();
				});
		}

		private void OnRuntimeSavePressed()
		{
			if (_game == null)
			{
				Notify("Nu există o sesiune activă de salvat.");
				return;
			}
			ShowSaveSlotPopup(SaveSlotMenuMode.Save, openedFromStartup: false);
		}

		private void OnRuntimeLoadPressed()
		{
			ShowSaveSlotPopup(SaveSlotMenuMode.Load, openedFromStartup: false);
		}

		private void OnRuntimeMainMenuPressed()
		{
			if (_game == null)
			{
				SetStartupMode(true);
				return;
			}

			ShowConfirmation(
				"Revii la meniul principal?",
				"Sesiunea curentă se închide. Salvează înainte dacă vrei să păstrezi progresul.",
				"Revino la meniu",
				() =>
				{
					_tickManager ??= GetNodeOrNull<TickManager>("../../TickManager");
					_tickManager?.ClearGame();
					_game = null;
					_lastObservedPhase = null;
					ClearPriceEditTarget();
					InvalidateShopNavigation();
					ResetTaskState();
					SetStartupMode(true);
					Notify("Ai revenit la meniul principal.");
				});
		}

		private void OnStartupNewGamePressed()
		{
			ShowNewGameSetupPanel();
		}

		private void OnStartupExitPressed()
		{
			ShowConfirmation(
				"Închizi jocul?",
				"Aplicația se va închide. Progresul nesalvat nu va fi păstrat.",
				"Ieșire",
				() => GetTree().Quit());
		}

		private void OnNewGameStartPressed()
		{
			string storeName = _newGameStoreNameInput?.Text.Trim() ?? "";
			if (string.IsNullOrWhiteSpace(storeName))
			{
				if (_newGameValidationLabel != null)
					_newGameValidationLabel.Text = "Numele magazinului nu poate fi gol.";
				return;
			}

			var difficulty = GameStartSettings.Default.Difficulty;
			if (_newGameDifficultyPicker != null && _newGameDifficultyPicker.GetItemCount() > 0)
				difficulty = (GameDifficulty)_newGameDifficultyPicker.GetItemId(_newGameDifficultyPicker.Selected);

			int duration = GameManager.CampaignDurationDays;
			if (_newGameDurationPicker != null && _newGameDurationPicker.GetItemCount() > 0)
				duration = _newGameDurationPicker.GetItemId(_newGameDurationPicker.Selected);

			_tickManager ??= GetNodeOrNull<TickManager>("../../TickManager");
			if (_tickManager == null)
			{
				if (_newGameValidationLabel != null)
					_newGameValidationLabel.Text = "Sistemul de joc nu este conectat. Verifică scena GameMain.";
				Notify("TickManager lipsește; sesiunea nu poate porni.");
				return;
			}

			_tickManager.StartNewGame(new GameStartSettings(storeName, difficulty, duration));
			InvalidateShopNavigation();
			HideStartupMenu();
			Notify(T("NOTIFY_NEW_GAME"));
			_tutorialSeenThisSession = false;
			_tutorialStep = TutorialStep.None;
			Refresh();
			ShowTutorialForNewGame();
		}

		private void ShowTutorialForNewGame()
		{
			if (!_tutorialEnabled || _tutorialSeenThisSession)
				return;

			StartGuidedTutorial();
		}

		private void OnStartupLoadFilePressed()
		{
			ShowSaveSlotPopup(SaveSlotMenuMode.Load, openedFromStartup: true);
		}

		private void SetRuntimeSpeed(double speed)
		{
			_tickManager ??= GetNodeOrNull<TickManager>("../../TickManager");
			if (_tickManager == null)
			{
				Notify("Sistemul de timp nu este conectat.");
				return;
			}
			_tickManager.SetSimulationSpeed(speed);
			Notify(TF("NOTIFY_SPEED", DescribeSpeed()));
			Refresh();
		}

		private string DescribeSpeed()
		{
			double speed = _tickManager?.SimulationSpeed ?? 1.0;
			if (speed <= 0.0)
				return T("BTN_PAUSE");

			return $"{speed:0.#}x";
		}

		private void OnRuntimeApplyPricePressed()
		{
			if (_game == null || _runtimePriceProductPicker == null)
			{
				GD.PrintErr("UIManager: Apply Price pressed before runtime controls were initialized.");
				return;
			}

			int? productId = _activePriceProductId ?? GetSelectedProductId(_runtimePriceProductPicker);
			if (!productId.HasValue)
			{
				Notify(T("NOTIFY_NO_PRODUCT"));
				return;
			}

			var product = _game.Inventory.GetProduct(productId.Value);
			if (product == null)
			{
				Notify(T("NOTIFY_NO_PRODUCT"));
				ClearPriceEditTarget();
				return;
			}

			if (!TryParseMoney(_runtimePriceInput?.Text, out var price) || price < Money.Zero || price > GameManager.MaximumProductSalePrice)
			{
				Notify(T("NOTIFY_INVALID_PRICE"));
				return;
			}

			if (_game.SetProductPrice(productId.Value, price))
			{
				_pricesChecked = true;
				BeginPriceEdit(productId.Value, focusInput: false);
				Notify(TF("NOTIFY_PRICE_UPDATED", ProductName(product)));
				CompleteTutorialStep(TutorialStep.SetPrice);
				HidePricesPopup();
			}
			else
				Notify(_game.CurrentPhase == DayPhase.Management ? TF("NOTIFY_PRICE_FAILED", productId.Value) : "Prețurile pot fi schimbate doar în Administrare.");

			Refresh();
		}

		private void OnRuntimePlaceOrderPressed()
		{
			if (_game == null || _runtimeOrderProductPicker == null || _runtimeOrderSupplierPicker == null)
			{
				GD.PrintErr("UIManager: Place Order pressed before runtime controls were initialized.");
				return;
			}

			if (_runtimeOrderProductPicker.GetItemCount() == 0 || _runtimeOrderSupplierPicker.GetItemCount() == 0)
			{
				Notify(T("NOTIFY_SELECT_PRODUCT_SUPPLIER"));
				return;
			}

			int productId = _runtimeOrderProductPicker.GetItemId(_runtimeOrderProductPicker.Selected);
			int supplierId = _runtimeOrderSupplierPicker.GetItemId(_runtimeOrderSupplierPicker.Selected);
			if (!TryParseInt(_runtimeOrderQuantityInput?.Text, out int quantity) || quantity <= 0)
			{
				Notify(T("NOTIFY_INVALID_QUANTITY"));
				return;
			}

			if (_game.PlaceRestockOrder(productId, quantity, supplierId))
			{
				_ordersPlaced = true;
				var product = _game.Inventory.GetProduct(productId);
				var supplier = _game.Suppliers.Suppliers.FirstOrDefault(item => item.Id == supplierId);
				if (product != null && supplier != null)
				{
					int costBasisPoints = _game.GetSupplierCostBasisPoints(product, supplier);
					Money orderCost = Money.FromMicros(product.CostPrice.ToMicros() * quantity * costBasisPoints / 10_000);
					Notify($"{FormatSignedMoney(Money.Zero - orderCost)} • comandă furnizor pentru {ProductName(product)} x{quantity}");
				}
				else
				{
					Notify(TF("NOTIFY_ORDER_PLACED", productId, quantity));
				}
			}
			else if (_game.CurrentPhase != DayPhase.Management)
				Notify(T("NOTIFY_ORDERS_MANAGEMENT"));
			else
			{
				var product = _game.Inventory.GetProduct(productId);
				var supplier = _game.Suppliers.Suppliers.FirstOrDefault(item => item.Id == supplierId);
				if (product != null && supplier != null)
				{
					int costBasisPoints = _game.GetSupplierCostBasisPoints(product, supplier);
					Money orderCost = Money.FromMicros(product.CostPrice.ToMicros() * quantity * costBasisPoints / 10_000);
					Notify(!_game.Economy.CanAfford(orderCost) ? "Nu ai suficienți bani pentru această comandă." : T("NOTIFY_ORDER_FAILED"));
				}
				else
				{
					Notify(T("NOTIFY_ORDER_FAILED"));
				}
			}

			Refresh();
		}

		private void OnRuntimeBuyShopCatalogPressed()
		{
			if (_game == null || _runtimeShopCatalogPicker == null)
			{
				Notify("Catalogul magazinului nu este pregătit.");
				return;
			}

			if (_runtimeShopCatalogPicker.GetItemCount() == 0)
			{
				Notify("Nu există articole disponibile pentru magazin.");
				return;
			}

			int catalogItemId = _runtimeShopCatalogPicker.GetItemId(_runtimeShopCatalogPicker.Selected);
			int productId = _runtimeShelfProductPicker != null && _runtimeShelfProductPicker.GetItemCount() > 0
				? _runtimeShelfProductPicker.GetItemId(_runtimeShelfProductPicker.Selected)
				: 1;
			var item = _game.ShopCatalog.FirstOrDefault(entry => entry.Id == catalogItemId);

			if (_game.PurchaseShopCatalogItem(catalogItemId, productId))
			{
				_stockChecked = true;
				InvalidateShopNavigation();
				Notify(item == null
					? T("NOTIFY_SHOP_PURCHASED")
					: $"{FormatSignedMoney(Money.Zero - item.Cost)} • investiție magazin: {DescribeShopCatalogItem(item)}");
			}
			else
			{
				if (_game.CurrentPhase != DayPhase.Management)
					Notify("Achizițiile pentru magazin funcționează doar în Administrare.");
				else if (item != null && !_game.Economy.CanAfford(item.Cost))
					Notify("Nu ai suficienți bani pentru acest obiect.");
				else
					Notify("Obiectul nu poate fi plasat sau cumpărat acum.");
			}

			Refresh();
		}

		private void OnRuntimeAssignShelfPressed()
		{
			if (_game == null || _runtimeShelfPicker == null || _runtimeShelfProductPicker == null)
			{
				Notify("Controalele pentru rafturi nu sunt pregătite.");
				return;
			}

			if (_runtimeShelfPicker.GetItemCount() == 0 || _runtimeShelfProductPicker.GetItemCount() == 0)
			{
				Notify(T("NOTIFY_SELECT_SHELF_PRODUCT"));
				return;
			}

			int shelfId = _runtimeShelfPicker.GetItemId(_runtimeShelfPicker.Selected);
			int productId = _runtimeShelfProductPicker.GetItemId(_runtimeShelfProductPicker.Selected);
			if (_game.AssignShelfProduct(shelfId, productId))
			{
				_stockChecked = true;
				Notify(TF("NOTIFY_SHELF_ASSIGNED", shelfId, productId));
			}
			else
				Notify(T("NOTIFY_SHELF_ASSIGN_FAILED"));

			Refresh();
		}

		private void OnRuntimeRefillShelfPressed()
		{
			if (_game == null || _runtimeShelfPicker == null)
			{
				Notify("Controalele pentru rafturi nu sunt pregătite.");
				return;
			}

			if (_runtimeShelfPicker.GetItemCount() == 0)
			{
				Notify("Alege un raft.");
				return;
			}

			if (!TryParseInt(_runtimeShelfRefillInput?.Text, out int quantity) || quantity <= 0)
			{
				Notify("Cantitate invalidă pentru realimentarea raftului.");
				return;
			}

			int shelfId = _runtimeShelfPicker.GetItemId(_runtimeShelfPicker.Selected);
			int moved = _game.RefillShelf(shelfId, quantity);
			Notify(moved > 0 ? TF("NOTIFY_STOCK_MOVED", moved, shelfId) : T("NOTIFY_NO_STOCK_MOVED"));
			if (moved > 0)
			{
				_stockChecked = true;
				CompleteTutorialStep(TutorialStep.StockShelf);
			}
			Refresh();
		}

		private void OnRuntimeRefillAllShelvesPressed()
		{
			if (_game == null)
				return;

			int movedTotal = 0;
			foreach (var shelf in _game.Inventory.Shelves)
				movedTotal += _game.RefillShelf(shelf.Id, shelf.Capacity - shelf.CurrentStock);

			Notify(movedTotal > 0 ? TF("NOTIFY_SHELVES_REFILLED", movedTotal) : T("NOTIFY_NO_REFILL"));
			if (movedTotal > 0)
			{
				_stockChecked = true;
				CompleteTutorialStep(TutorialStep.StockShelf);
			}
			Refresh();
		}

		private void OnRuntimeCopyReportPressed()
		{
			try
			{
				TrackCompletedReport();
				DisplayServer.ClipboardSet(BuildRuntimeReport(GetDisplayReport(), _previousCompletedReport));
				Notify(T("NOTIFY_REPORT_COPIED"));
			}
			catch (Exception ex)
			{
				GD.PrintErr($"UIManager: failed to copy report - {ex.Message}");
				Notify("Raportul nu a putut fi copiat pe acest sistem.");
			}
		}

		private void OnRuntimeHireCandidatePressed()
		{
			if (_game == null || _runtimeCandidatePicker == null || _runtimeCandidatePicker.GetItemCount() == 0)
			{
				Notify("Nu este selectat niciun candidat.");
				return;
			}

			int candidateId = _runtimeCandidatePicker.GetItemId(_runtimeCandidatePicker.Selected);
			var candidate = _game.Employees.Candidates.Find(c => c.Id == candidateId);
			string candidateName = candidate?.Profile.Name ?? $"candidat #{candidateId}";

			if (_game.HireCandidate(candidateId))
			{
				_staffChecked = true;
				_staffChanged = true;
				Notify(TF("NOTIFY_HIRED", candidateName));
			}
			else
			{
				Notify(candidate != null && !_game.Economy.CanAfford(candidate.Profile.Salary * 2)
					? "Nu ai suficienți bani pentru acest angajat."
					: TF("NOTIFY_HIRE_FAILED", candidateName));
			}

			Refresh();
		}

		private void OnRuntimeFireEmployeePressed()
		{
			if (_game == null || _runtimeEmployeePicker == null || _runtimeEmployeePicker.GetItemCount() == 0)
			{
				Notify("Nu este selectat niciun angajat.");
				return;
			}

			int index = _runtimeEmployeePicker.GetItemId(_runtimeEmployeePicker.Selected);
			if (index < 0 || index >= _game.Employees.Employees.Count)
			{
				Notify("Angajatul selectat nu mai este disponibil.");
				Refresh();
				return;
			}

			string name = _game.Employees.Employees[index].Name;
			if (_game.FireEmployee(name))
			{
				_staffChecked = true;
				_staffChanged = true;
				Notify(TF("NOTIFY_FIRED", name));
			}
			else
				Notify(TF("NOTIFY_FIRE_FAILED", name));

			Refresh();
		}

		private void OnRuntimeResolveEventPressed(int option)
		{
			if (_game == null)
				return;

			if (_game.ResolveEventDecision(option))
			{
				_eventsChecked = true;
				Notify(T("NOTIFY_EVENT_APPLIED"));
			}
			else
				Notify(T("NOTIFY_NO_EVENT_DECISION"));

			Refresh();
		}

		private void OnRuntimeTriggerEventPressed()
		{
			if (_game == null || !EnableLayoutDebug)
				return;

			if (_game.TriggerDebugEvent())
			{
				_eventsChecked = true;
				ShowRuntimeSection(_runtimeEventSection);
				ShowEventPopup();
				Notify(T("NOTIFY_EVENT_CREATED"));
			}
			else
			{
				Notify(T("NOTIFY_EVENT_CREATE_FAILED"));
			}

			Refresh();
		}

		private void SaveToSlot(int slot)
		{
			SaveToPath(GetSaveSlotPath(slot), $"{T("LABEL_SLOT")} {slot}");
		}

		private bool LoadFromSlot(int slot, bool fromStartup)
		{
			_tickManager ??= GetNodeOrNull<TickManager>("../../TickManager");
			if (fromStartup && _game == null)
			{
				if (_tickManager == null)
				{
					Notify("TickManager lipsește; încărcarea nu poate porni.");
					SetStartupMode(true);
					return false;
				}
				_tickManager.StartNewGame();
			}

			bool loaded = LoadFromPath(GetSaveSlotPath(slot), $"{T("LABEL_SLOT")} {slot}");
			if (loaded && fromStartup)
				HideStartupMenu();
			else if (!loaded && fromStartup)
			{
				_tickManager?.ClearGame();
				_game = null;
				SetStartupMode(true);
			}

			return loaded;
		}

		private void SaveToPath(string path, string displayName)
		{
			if (_game == null)
			{
				Notify("Nu există o sesiune activă de salvat.");
				return;
			}

			try
			{
				_game.SaveGame(path);
				Notify(TF("NOTIFY_SAVE_TO", displayName));
				RefreshSaveSlotButtons();
				UpdateTaskBoxDisplay();
			}
			catch (Exception ex)
			{
				GD.PrintErr($"UIManager: save failed - {ex}");
				Notify(TF("NOTIFY_SAVE_FAILED", ex.Message));
			}
		}

		private bool LoadFromPath(string path, string displayName)
		{
			if (_game == null)
			{
				Notify("Nu există o sesiune activă pentru încărcare.");
				return false;
			}

			try
			{
				_game.LoadGame(path);
				HideAllPopups();
				ClearPriceEditTarget();
				InvalidateShopNavigation();
				ResetTaskState();
				_lastObservedPhase = null;
				_lastCheckoutFeedbackSequence = _game.LastCheckoutFeedbackSequence;
				_lastReputationFeedbackSequence = _game.LastReputationFeedbackSequence;
				Refresh();
				Notify(TF("NOTIFY_LOAD_FROM", displayName));
				return true;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"UIManager: load failed - {ex}");
				Notify(TF("NOTIFY_LOAD_FAILED", ex.Message));
				return false;
			}
		}

		private void Notify(string message)
		{
			if (_runtimeNotificationLabel != null)
			{
				_runtimeNotificationLabel.Text = message;
				bool visible = !string.IsNullOrWhiteSpace(message) && message != T("NOTIFY_RUNTIME_READY");
				_runtimeNotificationLabel.Visible = visible;
				if (_notificationPanel != null)
					_notificationPanel.Visible = visible;
			}
			else
				GD.Print(message);
		}

		private static bool TryParseInt(string? text, out int value)
		{
			return int.TryParse(text, out value);
		}

		private static bool TryParseMoney(string? text, out Money value)
		{
			value = Money.Zero;
			if (string.IsNullOrWhiteSpace(text))
				return false;

			if (!decimal.TryParse(text.Trim().Replace("lei", "", StringComparison.OrdinalIgnoreCase).Replace("RON", "", StringComparison.OrdinalIgnoreCase).Trim(), out var amount))
				return false;

			long micros = (long)Math.Round(amount * 1_000_000m);
			value = Money.FromMicros(micros);
			return true;
		}

		private sealed record ShopNodeLayout(Vector2 Position, Vector2 Scale);
	}
}
