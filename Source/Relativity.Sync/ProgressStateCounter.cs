using System.Collections.Generic;

namespace Relativity.Sync
{
    internal sealed class ProgressStateCounter : IProgressStateCounter
    {
        private int _current;
        private readonly IDictionary<string, int> _orderDictionary = new Dictionary<string, int>();

        public ProgressStateCounter() : this(0)
        {
        }

        public ProgressStateCounter(int initial)
        {
            _current = initial;
        }

        public int Next()
        {
            int value = _current;
            _current += 1;
            return value;
        }

        public int GetOrderForGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                int nextId = Next();
                return nextId;
            }
            else
            {
                if (!_orderDictionary.ContainsKey(groupName))
                {
                    int nextId = Next();
                    _orderDictionary.Add(groupName, nextId);
                }

                return _orderDictionary[groupName];
            }
        }
    }
}
