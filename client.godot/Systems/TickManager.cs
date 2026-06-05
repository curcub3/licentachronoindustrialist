using Godot;
using Client.Architecture.MVVM;
using Client.Scripts.Loop;
using Client.Scripts.Visuals;
using Core.Simulation.Data;
using Core.Simulation.Logic;

namespace Client.Scripts.Systems
{
    public partial class TickManager : Node
    {
        public const double FixedStep = 1.0 / 60.0;

        private double _accumulator = 0.0;
        private double _uiRefreshAccumulator = 0.0;
        public double Alpha { get; private set; }
        public double SimulationSpeed { get; private set; } = 1.0;

        [Export] public UIManager? UIManager { get; set; }
        [Export] public double BusinessUiRefreshInterval { get; set; } = 0.20;

        public GameManager? Game { get; private set; }
        public EconomyViewModel EconomyViewModel { get; private set; } = new();

        public override void _Ready()
        {
            UIManager ??= GetNodeOrNull<UIManager>("../CanvasLayer/UIRoot");
            EconomyViewModel = new EconomyViewModel();
            if (UIManager == null)
                GD.PrintErr("TickManager: UIManager was not assigned and could not be found.");
        }

        public override void _Process(double delta)
        {
            if (Game == null)
            {
                Alpha = 0.0;
                return;
            }

            if (Game.CurrentPhase != Core.Simulation.Data.DayPhase.Business)
            {
                Alpha = 0.0;
                EconomyViewModel.Update(Game.Economy.Cash);
                return;
            }

            if (SimulationSpeed <= 0.0)
                return;

            _accumulator += delta * SimulationSpeed;
            _uiRefreshAccumulator += delta;

            int loops = 0;
            while (_accumulator >= FixedStep)
            {
                Game.TickBusiness();
                _accumulator -= FixedStep;
                loops++;
            }

            if (loops > 10)
            {
                GD.PrintErr("Spiral of death detected, resetting accumulator.");
                _accumulator = 0;
            }

            Alpha = _accumulator / FixedStep;

            if (loops > 0)
            {
                EconomyViewModel.Update(Game.Economy.Cash);
                bool forceFullUiRefresh = _uiRefreshAccumulator >= BusinessUiRefreshInterval;
                UIManager?.Refresh(forceFullUiRefresh);
                if (forceFullUiRefresh)
                    _uiRefreshAccumulator = 0.0;
            }
        }

        public void ResetAccumulator()
        {
            _accumulator = 0.0;
            _uiRefreshAccumulator = 0.0;
            Alpha = 0.0;
        }

        public void SetSimulationSpeed(double speed)
        {
            SimulationSpeed = System.Math.Clamp(speed, 0.0, 4.0);
            ResetAccumulator();
        }

        public void StartNewGame(GameStartSettings? settings = null)
        {
            Game?.Dispose();
            Game = new GameManager(10000, ghostCount: 3, settings);
            EconomyViewModel = new EconomyViewModel();
            SimulationSpeed = 1.0;
            ResetAccumulator();
            UIManager?.Initialize(Game);
            UIManager?.Refresh();
            GetNodeOrNull<LoopManager>("../LoopManager")?.CreateInitialSaveState();
        }

        public void ClearGame()
        {
            Game?.Dispose();
            Game = null;
            EconomyViewModel = new EconomyViewModel();
            SimulationSpeed = 1.0;
            ResetAccumulator();
        }

        public void StartBusinessPhase()
        {
            if (Game != null && Game.StartBusiness())
                ResetAccumulator();

            UIManager?.Refresh();
        }

        public void AdvanceFromClosing()
        {
            Game?.AdvanceToNextDay();
            ResetAccumulator();
            UIManager?.Refresh();
        }

        public override void _ExitTree()
        {
            Game?.Dispose();
        }
    }
}
