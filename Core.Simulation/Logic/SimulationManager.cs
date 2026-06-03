using System;
using System.Collections.Generic;
using Core.Simulation.Serialization;

namespace Core.Simulation.Logic
{
    /// <summary>
    /// Orchestrates the active simulation and a set of ghost simulations.
    /// Each ghost has its own command buffer for deterministic replay.
    /// </summary>
    public sealed class SimulationManager : IDisposable
    {
        public SimulationLoop Active { get; }
        public IReadOnlyList<SimulationLoop> Ghosts => _ghosts;
        public CommandBuffer ActiveCommandBuffer { get; }

        private readonly List<SimulationLoop> _ghosts;
        private readonly List<CommandBuffer> _ghostCommandBuffers;

        public SimulationManager(int maxEntities, int ghostCount = 0)
        {
            Active = new SimulationLoop(maxEntities);
            ActiveCommandBuffer = new CommandBuffer(Math.Max(32, maxEntities / 16));
            _ghosts = new List<SimulationLoop>(ghostCount);
            _ghostCommandBuffers = new List<CommandBuffer>(ghostCount);

            for (int i = 0; i < ghostCount; i++)
                AddGhostSimulation(maxEntities);
        }

        public void AddGhostSimulation(int maxEntities)
        {
            _ghosts.Add(new SimulationLoop(maxEntities));
            _ghostCommandBuffers.Add(new CommandBuffer(Math.Max(32, maxEntities / 16)));
        }

        public CommandBuffer GetGhostCommandBuffer(int index)
        {
            return _ghostCommandBuffers[index];
        }

        public void Tick()
        {
            Active.Tick(ActiveCommandBuffer);
            for (int i = 0; i < _ghosts.Count; i++)
                _ghosts[i].Tick(_ghostCommandBuffers[i]);
        }

        public void ResetTick()
        {
            Active.ResetTick();
        }

        public void Dispose()
        {
            Active.Dispose();
            ActiveCommandBuffer.Dispose();
            foreach (var buffer in _ghostCommandBuffers)
                buffer.Dispose();
            foreach (var ghost in _ghosts)
                ghost.Dispose();
        }
    }
}
