using System;
using System.Collections.Concurrent;
using Relativity.API;
using Relativity.Toggles;

namespace Relativity.Sync.Toggles.Service
{
	internal class SyncToggles : ISyncToggles
    {
        private readonly ConcurrentDictionary<Type, bool> _cache;

		private readonly IToggleProvider _toggleProvider;
		private readonly IAPILog _logger;

		public SyncToggles(IToggleProvider toggleProvider, IAPILog logger)
		{
			_toggleProvider = toggleProvider;
			_logger = logger;

            _cache = new ConcurrentDictionary<Type, bool>();
        }
		
        public bool IsEnabled<T>() where T: IToggle
        {
            Type type = typeof(T);

            if (!_cache.ContainsKey(type))
            {
                bool isEnabled = _toggleProvider.IsEnabled<T>();
                _cache[type] = isEnabled;
                _logger.LogInformation("Toggle {toggleName} is enabled: {isEnabled}", nameof(T), isEnabled);
            }

            return _cache[type];
        }
    }
}