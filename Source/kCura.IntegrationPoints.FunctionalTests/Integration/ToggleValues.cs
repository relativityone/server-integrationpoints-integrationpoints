using Relativity.Toggles;
using System;
using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Tests.Integration
{
    public class ToggleValues
    {
        private IDictionary<Type, bool> _values { get; } = new Dictionary<Type, bool>();

        public void SetValue<T>(bool value) where T: IToggle
        {
            _values[typeof(T)] = value;
        }

        public bool? GetValue<T>() where T: IToggle
        {
            if(_values.ContainsKey(typeof(T)))
            {
                return _values[typeof(T)];
            }

            return null;
        }
    }
}
