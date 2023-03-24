using System.Collections.Generic;
using Relativity.Toggles;

namespace Relativity.IntegrationPoints.Tests.Integration
{
    public class ToggleValues
    {
        private IDictionary<string, bool> _values { get; } = new Dictionary<string, bool>();

        public void SetValue<T>(bool value) where T : IToggle
        {
            _values[typeof(T).FullName] = value;
        }

        public bool GetValue<T>() where T : IToggle
        {
            return GetValue(typeof(T).FullName);
        }

        public bool GetValue(string name)
        {
            if (_values.ContainsKey(name))
            {
                return _values[name];
            }

            return false;
        }
    }
}
