using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Core.Simulation.Data
{
    /// <summary>
    /// A wrapper around a pinned array providing unsafe pointer access.
    /// Implements the "Bag" data structure with O(1) removal.
    /// </summary>
    public unsafe class EntityBuffer<T> : IDisposable where T : unmanaged
    {
        public T[] Data;
        public int Count;
        public int Capacity;

        private T* _ptr;

        public EntityBuffer(int capacity)
        {
            Capacity = capacity;
            Count = 0;
            Data = GC.AllocateArray<T>(capacity, pinned: true);
            _ptr = (T*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(Data));
        }

        public T* GetBasePointer() => _ptr;

        public void Add(T item)
        {
            if (Count >= Capacity)
                Resize(Capacity * 2);
            _ptr[Count] = item;
            Count++;
        }

        /// <summary>O(1) removal using "Swap-Back". Order is NOT preserved.</summary>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count) return;
            Count--;
            if (index < Count)
                _ptr[index] = _ptr[Count];
        }

        private void Resize(int newCapacity)
        {
            T[] newData = GC.AllocateArray<T>(newCapacity, pinned: true);
            Data.CopyTo(newData, 0);
            Data = newData;
            Capacity = newCapacity;
            _ptr = (T*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(Data));
        }

        public void Dispose()
        {
            _ptr = null;
            Data = null!;
        }
    }
}
