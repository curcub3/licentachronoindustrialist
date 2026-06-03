namespace Core.Simulation.Logic
{
    internal sealed class DeterministicRandom
    {
        private uint _state;

        public DeterministicRandom(int seed)
        {
            _state = seed == 0 ? 0x6D2B79F5u : (uint)seed;
        }

        public int Next(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
                return minInclusive;

            uint range = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextUInt() % range);
        }

        public bool ChanceBasisPoints(int basisPoints)
        {
            if (basisPoints <= 0)
                return false;
            if (basisPoints >= 10_000)
                return true;

            return Next(0, 10_000) < basisPoints;
        }

        private uint NextUInt()
        {
            _state = unchecked((_state * 1664525u) + 1013904223u);
            return _state;
        }
    }
}
