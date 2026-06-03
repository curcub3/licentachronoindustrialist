using System;
using System.Runtime.CompilerServices;

namespace Core.Simulation.Data
{
    public struct GridMap
    {
        public readonly int Width;
        public readonly int Height;

        // Stores the EntityID occupying the tile. -1 means Empty.
        public int[] Tiles;

        public GridMap(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new int[width * height];
            Array.Fill(Tiles, -1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(IntVector2 pos)
        {
            if ((uint)pos.X >= (uint)Width || (uint)pos.Y >= (uint)Height) return -1;
            return (pos.Y * Width) + pos.X;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int x, int y)
        {
            if ((uint)x >= (uint)Width || (uint)y >= (uint)Height) return -1;
            return (y * Width) + x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntVector2 GetCoord(int index) => new IntVector2(index % Width, index / Width);

        public bool IsWalkable(IntVector2 pos)
        {
            int idx = GetIndex(pos);
            return idx != -1 && Tiles[idx] == -1;
        }

        public bool IsWalkable(int x, int y)
        {
            int idx = GetIndex(x, y);
            return idx != -1 && Tiles[idx] == -1;
        }
    }
}
