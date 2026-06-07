using Core.Simulation.Data;
using Core.Simulation.Serialization;
using System;

namespace Core.Simulation.Logic
{
    public class SimulationLoop : IDisposable
    {
        public EntityBuffer<Item> Items { get; private set; }
        public int CurrentTick { get; private set; }

        public SimulationLoop(int maxEntities)
        {
            Items = new EntityBuffer<Item>(maxEntities);
            CurrentTick = 0;
        }

        /// <summary>Advances the simulation by one discrete step.</summary>
        public void Tick(CommandBuffer? commands = null)
        {
            if (commands != null)
            {
                commands.ForEachCommandAtTick(CurrentTick, ProcessCommand);
                commands.RemoveExecutedCommandsForTick(CurrentTick);
            }

            unsafe
            {
                Item* ptr = Items.GetBasePointer();
                int count = Items.Count;

                // Cache-friendly loop: memory is contiguous.
                for (int i = 0; i < count; i++)
                {
                    // byte overflow wraps naturally (255 -> 0).
                    ptr[i].Progress += 4;
                }
            }
            CurrentTick++;
        }

        private void ProcessCommand(GameCommand command)
        {
            switch (command.ActionType)
            {
                case 0:
                    // Reserved for future Phase 2/3 action processing.
                    break;
                default:
                    // Unsupported action types may be logged or ignored in this phase.
                    break;
            }
        }

        public void ResetTick()
        {
            CurrentTick = 0;
        }

        public void Dispose()
        {
            Items.Dispose();
        }
    }
}
