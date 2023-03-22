using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Telemetry.APM;
using Relativity.Toggles;
using static kCura.IntegrationPoints.Common.Monitoring.Constants.RelEye;

namespace kCura.IntegrationPoints.Common.Toggles
{
    public class RipToggleProvider : IRipToggleProvider
    {
        private readonly ConcurrentDictionary<string, bool> _cache = new ConcurrentDictionary<string, bool>();

        private readonly IToggleProvider _toggleProvider;
        private readonly IAPM _apm;
        private readonly IAPILog _logger;

        public RipToggleProvider(IToggleProvider toggleProvider, IAPM apm, IAPILog logger)
        {
            _toggleProvider = toggleProvider;
            _apm = apm;
            _logger = logger;
        }

        public bool IsEnabled<T>() where T : IToggle
        {
            return IsEnabledAsync<T>().GetAwaiter().GetResult();
        }

        public async Task<bool> IsEnabledAsync<T>() where T : IToggle
        {
            return await IsEnabledByNameAsync(typeof(T).FullName).ConfigureAwait(false);
        }

        public bool IsEnabledByName(string name)
        {
            return IsEnabledByNameAsync(name).GetAwaiter().GetResult();
        }

        public async Task<bool> IsEnabledByNameAsync(string name)
        {
            if (!_cache.ContainsKey(name))
            {
                bool isEnabled = await _toggleProvider.IsEnabledByNameAsync(name).ConfigureAwait(false);
                _cache[name] = isEnabled;
                _logger.LogInformation("Toggle {toggleName} is enabled: {isEnabled}", name, isEnabled);

                SendToggleEvent(name, isEnabled);
            }

            return _cache[name];
        }

        private void SendToggleEvent(string toggleName, bool toggleValue)
        {
            try
            {
                Dictionary<string, object> attrs = new Dictionary<string, object>
                {
                    { Names.R1TeamID, Values.R1TeamID },
                    { Names.ServiceName, Values.ServiceName },
                    { Names.ToggleName, toggleName },
                    { Names.ToggleValue, toggleValue.ToString() },
                };

                _apm.CountOperation(EventNames.Toggle, customData: attrs).Write();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred during Toggle Read event send - {toggleName}.", toggleName);
            }
        }
    }
}
