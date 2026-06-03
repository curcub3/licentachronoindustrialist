using System;
using Core.Simulation.Data;

namespace Core.Simulation.Serialization
{
    public unsafe class SnapshotManager
    {
        private Item[] _snapshotData;
        private int _savedCount;

        public SnapshotManager(int capacity)
        {
            _snapshotData = GC.AllocateArray<Item>(capacity, pinned: true);
        }

        public void CaptureState(EntityBuffer<Item> activeBuffer)
        {
            _savedCount = activeBuffer.Count;
            long byteLength = _savedCount * sizeof(Item);

            fixed (Item* destPtr = _snapshotData)
            {
                Buffer.MemoryCopy(
                    source: activeBuffer.GetBasePointer(),
                    destination: destPtr,
                    destinationSizeInBytes: byteLength,
                    sourceBytesToCopy: byteLength
                );
            }
        }

        public void RestoreState(EntityBuffer<Item> activeBuffer)
        {
            activeBuffer.Count = _savedCount;
            long byteLength = _savedCount * sizeof(Item);

            fixed (Item* srcPtr = _snapshotData)
            {
                Buffer.MemoryCopy(
                    source: srcPtr,
                    destination: activeBuffer.GetBasePointer(),
                    destinationSizeInBytes: byteLength,
                    sourceBytesToCopy: byteLength
                );
            }
        }
    }
}
