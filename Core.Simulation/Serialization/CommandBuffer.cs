using System;
using Core.Simulation.Data;

namespace Core.Simulation.Serialization
{
    /// <summary>
    /// A deterministic input tape for phase 2 command scheduling.
    /// Uses a POH-backed EntityBuffer to avoid managed allocations on the hot path.
    /// </summary>
    public sealed class CommandBuffer : IDisposable
    {
        private readonly EntityBuffer<GameCommand> _commands;

        public int Count => _commands.Count;

        public CommandBuffer(int capacity = 128)
        {
            _commands = new EntityBuffer<GameCommand>(capacity);
        }

        public void Add(GameCommand command)
        {
            _commands.Add(command);
        }

        public void Schedule(GameCommand command, int targetTick)
        {
            command.Tick = targetTick;
            Add(command);
        }

        public void ForEachCommandAtTick(int tick, Action<GameCommand> action)
        {
            for (int i = 0; i < _commands.Count; i++)
            {
                var current = _commands.Data[i];
                if (current.Tick == tick)
                    action(current);
            }
        }

        public void RemoveExecutedCommandsForTick(int tick)
        {
            int index = 0;
            while (index < _commands.Count)
            {
                if (_commands.Data[index].Tick == tick)
                    _commands.RemoveAt(index);
                else
                    index++;
            }
        }

        public void Clear()
        {
            _commands.Count = 0;
        }

        public void Dispose()
        {
            _commands.Dispose();
        }
    }
}
