using System;
using System.Threading.Tasks;
using Core.Simulation.Data;

namespace Core.Simulation.Logic
{
    public unsafe class ConveyorSystem
    {
        private readonly GridMap _grid;
        private readonly int[] _moveCandidates;

        public ConveyorSystem(GridMap grid, int maxEntities)
        {
            _grid = grid;
            _moveCandidates = new int[maxEntities];
        }

        public void Update(EntityBuffer<Item> entityBuffer)
        {
            int count = entityBuffer.Count;
            Item* ptr = entityBuffer.GetBasePointer();

            // PHASE 1: INSPECT (Parallel Read)
            Array.Fill(_moveCandidates, -1, 0, count);

            Parallel.For(0, count, i =>
            {
                // Type 1 = Conveyor belt
                if (ptr[i].Type != 1) return;

                // Direction stored in Progress: 0=N, 1=E, 2=S, 3=W
                IntVector2 targetPos = ptr[i].Position;
                switch (ptr[i].Progress)
                {
                    case 0: targetPos.Y -= 1; break;
                    case 1: targetPos.X += 1; break;
                    case 2: targetPos.Y += 1; break;
                    case 3: targetPos.X -= 1; break;
                }

                int targetGridIndex = _grid.GetIndex(targetPos);
                if (targetGridIndex == -1) return;

                int targetEntityId = _grid.Tiles[targetGridIndex];
                if (targetEntityId != -1 && ptr[targetEntityId].Type == 1)
                    _moveCandidates[i] = targetEntityId;
            });

            // PHASE 2: COMMIT (Sequential Write)
            for (int i = 0; i < count; i++)
            {
                int targetId = _moveCandidates[i];
                if (targetId != -1)
                {
                    // Swap-move: transfer held item to next conveyor slot
                    // (HeldEntityId field to be added to Item struct in future iteration)
                }
            }
        }
    }
}
