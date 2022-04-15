using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Relativity.Toggles;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
    public class TestSyncToggleProvider : IToggleProvider
    {
        private readonly Dictionary<Type, bool> _overridenToggles = new Dictionary<Type, bool>();

        public bool IsEnabled<T>() where T : IToggle
        {
            return IsEnabledAsync<T>().GetAwaiter().GetResult();
        }

        public Task<bool> IsEnabledAsync<T>() where T : IToggle
        {
            Type toggleType = typeof(T);
            if (_overridenToggles.ContainsKey(toggleType))
            {
                return Task.FromResult(_overridenToggles[toggleType]);
            }

            return Task.FromResult(GetToggleDefaultValue(toggleType));
        }

        private bool GetToggleDefaultValue(Type toggleType)
        {
            DefaultValueAttribute attribute = toggleType.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute;

            return attribute != null && attribute.Value;
        }

        public bool IsEnabledByName(string toggleName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsEnabledByNameAsync(string toggleName)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync<T>(bool enabled) where T : IToggle
        {
            _overridenToggles.Add(typeof(T), enabled);
            return Task.CompletedTask;
        }

        public MissingFeatureBehavior DefaultMissingFeatureBehavior { get; }
        public bool CacheEnabled { get; set; }
        public int CacheTimeoutInSeconds { get; set; }
    }
}
