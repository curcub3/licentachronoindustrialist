using System.Runtime.InteropServices;

namespace Core.Simulation.Data
{
    /// <summary>
    /// The fundamental entity struct. Strictly blittable to allow pointer access.
    /// 10,000 entities = ~200KB of RAM (fits entirely in L1/L2 Cache).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Item
    {
        public int ID;               // 4 bytes: Unique Entity Identifier
        public int Type;             // 4 bytes: Visual/Logic Type Lookup ID
        public IntVector2 Position;  // 8 bytes: Grid Coordinates
        public byte Progress;        // 1 byte:  Movement Progress (0-255 sub-pixel)
    }
}
