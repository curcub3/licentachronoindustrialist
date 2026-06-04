using Godot;
using Client.Scripts.Systems;
using Core.Simulation.Data;
using Core.Simulation.Serialization;

namespace Client.Scripts.Loop
{
    public partial class LoopManager : Node
    {
        [Export] public TickManager TickManager { get; set; } = null!;

        private SnapshotManager _snapshotManager = null!;
        private GameSaveData? _baselineGameState;

        public override void _Ready()
        {
            TickManager ??= GetNodeOrNull<TickManager>("../TickManager");
            if (TickManager == null)
                GD.PrintErr("LoopManager: TickManager was not assigned and could not be found.");

            _snapshotManager = new SnapshotManager(10000);
        }

        // Call when the player first spawns to create the "Tick 0" save state
        public void CreateInitialSaveState()
        {
            if (TickManager?.Game == null)
                return;

            GD.Print("LoopManager: Capturing T=0 baseline...");
            _snapshotManager.CaptureState(TickManager.Game.Simulation.Active.Items);
            _baselineGameState = TickManager.Game.Persistence.Capture(TickManager.Game);
        }

        // Call when the loop timer hits zero or the player dies
        public void TriggerLoopReset()
        {
            GD.Print("LoopManager: Initiating Time Loop Reset Sequence...");
            if (TickManager?.Game == null)
                return;

            // Step 1: Pause SceneTree to prevent _Process from reading memory while we overwrite it
            GetTree().Paused = true;

            // Step 2: Restore the "Truth" — instant unmanaged memory overwrite via Buffer.MemoryCopy
            if (_baselineGameState != null)
                TickManager.Game.Persistence.Restore(TickManager.Game, _baselineGameState);
            _snapshotManager.RestoreState(TickManager.Game.Simulation.Active.Items);

            // Step 3: Reset the deterministic clocks
            TickManager.ResetAccumulator();
            TickManager.Game.ResetTick();

            // Step 4: Resume execution
            GetTree().Paused = false;

            GD.Print("LoopManager: Reset Sequence Complete. Timeline restored to T=0.");
        }
    }
}
