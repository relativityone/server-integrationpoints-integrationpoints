using System;

namespace Relativity.Sync.Utils
{
    internal class WrapperForRandom : IRandom
    {
        private readonly Random _random;

        public WrapperForRandom()
        {
            _random = new Random();
        }

        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }
    }
}
