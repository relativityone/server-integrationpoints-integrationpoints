using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Exceptions;
using Relativity.Toggles;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
    public class TestSyncToggleProvider : IToggleProvider
    {
        public Dictionary<Type, bool> _overridenToggles = new Dictionary<Type, bool>();

        public bool IsEnabled<T>() where T : IToggle
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsEnabledAsync<T>() where T : IToggle
        {
            if (_overridenToggles.ContainsKey(typeof(T)))
            {
                return Task.FromResult(_overridenToggles[typeof(T)]);
            }
            
            throw new NotFoundException($"Not found value for Key: {typeof(T)}");
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
