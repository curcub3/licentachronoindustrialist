using System;
using System.Collections;
using System.Collections.Generic;
using Core.Simulation.Data;

namespace Core.Simulation.Logic
{
    public static class Pathfinder
    {
        [ThreadStatic] private static PriorityQueue<int, int>? _openSet;
        [ThreadStatic] private static BitArray? _closedSet;
        [ThreadStatic] private static int[]? _gScore;
        [ThreadStatic] private static int[]? _cameFrom;

        public static void Initialize(int mapSize)
        {
            if (_openSet != null) return;
            _openSet = new PriorityQueue<int, int>(mapSize / 4);
            _closedSet = new BitArray(mapSize);
            _gScore = new int[mapSize];
            _cameFrom = new int[mapSize];
        }

        public static List<int> FindPath(GridMap map, IntVector2 start, IntVector2 end)
        {
            int mapSize = map.Width * map.Height;
            if (_openSet == null) Initialize(mapSize);

            int startIdx = map.GetIndex(start);
            int endIdx = map.GetIndex(end);

            var openSet = _openSet ?? throw new InvalidOperationException("Pathfinder not initialized.");
            var closedSet = _closedSet ?? throw new InvalidOperationException("Pathfinder not initialized.");
            var gScore = _gScore ?? throw new InvalidOperationException("Pathfinder not initialized.");
            var cameFrom = _cameFrom ?? throw new InvalidOperationException("Pathfinder not initialized.");

            openSet.Clear();
            closedSet.SetAll(false);
            Array.Fill(gScore, int.MaxValue);

            gScore[startIdx] = 0;
            openSet.Enqueue(startIdx, 0);
            cameFrom[startIdx] = -1;

            while (openSet.Count > 0)
            {
                int current = openSet.Dequeue();
                if (current == endIdx) return ReconstructPath(current, cameFrom);

                if (closedSet[current]) continue;
                closedSet[current] = true;

                IntVector2 cPos = map.GetCoord(current);

                ProcessNeighbor(map, new IntVector2(cPos.X, cPos.Y - 1), current, endIdx, openSet, closedSet, gScore, cameFrom);
                ProcessNeighbor(map, new IntVector2(cPos.X, cPos.Y + 1), current, endIdx, openSet, closedSet, gScore, cameFrom);
                ProcessNeighbor(map, new IntVector2(cPos.X - 1, cPos.Y), current, endIdx, openSet, closedSet, gScore, cameFrom);
                ProcessNeighbor(map, new IntVector2(cPos.X + 1, cPos.Y), current, endIdx, openSet, closedSet, gScore, cameFrom);
            }

            return new List<int>();
        }

        private static void ProcessNeighbor(GridMap map, IntVector2 nPos, int current, int endIdx,
            PriorityQueue<int, int> openSet,
            BitArray closedSet,
            int[] gScore,
            int[] cameFrom)
        {
            int neighborIdx = map.GetIndex(nPos);
            if (neighborIdx == -1 || closedSet[neighborIdx] || !map.IsWalkable(nPos)) return;

            int tentativeG = gScore[current] + 1;
            if (tentativeG < gScore[neighborIdx])
            {
                cameFrom[neighborIdx] = current;
                gScore[neighborIdx] = tentativeG;

                IntVector2 endPos = map.GetCoord(endIdx);
                int heuristic = Math.Abs(nPos.X - endPos.X) + Math.Abs(nPos.Y - endPos.Y);
                openSet.Enqueue(neighborIdx, tentativeG + heuristic);
            }
        }

        private static List<int> ReconstructPath(int current, int[] cameFrom)
        {
            var path = new List<int>();
            while (current != -1)
            {
                path.Add(current);
                current = cameFrom[current];
            }
            path.Reverse();
            return path;
        }
    }
}
