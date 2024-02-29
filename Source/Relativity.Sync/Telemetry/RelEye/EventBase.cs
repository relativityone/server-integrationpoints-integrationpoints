using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Relativity.Sync.Telemetry.RelEye
{
    internal abstract class EventBase<T> : IEvent
        where T : IEvent, new()
    {
        private static Dictionary<PropertyInfo, RelEyeAttribute> _metricCacheProperties;

        public abstract string EventName { get; }

        public Dictionary<string, object> GetValues()
        {
            Dictionary<PropertyInfo, RelEyeAttribute> metricProperties = GetEventProperties();
            return metricProperties
                .Select(kvp => new { Key = kvp.Value.Name, Value = GetValue(kvp.Key) })
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private Dictionary<PropertyInfo, RelEyeAttribute> GetEventProperties()
        {
            return _metricCacheProperties ??
                   (_metricCacheProperties = GetType()
                       .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                       .Where(p => p.GetMethod != null && p.GetCustomAttribute<RelEyeAttribute>() != null)
                       .ToDictionary(p => p, p => p.GetCustomAttribute<RelEyeAttribute>()));
        }

        private object GetValue(PropertyInfo p)
        {
            Type underlyingType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

            object value = p.GetValue(this, null);
            if (underlyingType.IsEnum == true)
            {
                return value?.ToString();
            }
            else if (underlyingType == typeof(Guid))
            {
                return value?.ToString();
            }

            return p.GetValue(this);
        }
    }
}
