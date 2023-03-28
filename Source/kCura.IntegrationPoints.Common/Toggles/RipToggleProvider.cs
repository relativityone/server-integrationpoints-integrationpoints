using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Telemetry.APM;
using Relativity.Toggles;
using static kCura.IntegrationPoints.Common.Monitoring.Constants.RelEye;

namespace kCura.IntegrationPoints.Common.Toggles
{
    public class RipToggleProvider : IRipToggleProvider
    {
        private readonly Dictionary<string, bool> _cache = new Dictionary<string, bool>();

        private readonly IToggleProvider _toggleProvider;
        private readonly IAPM _apm;
        private readonly IAPILog _logger;
        private readonly SemaphoreSlim _dictionarySemaphore = new SemaphoreSlim(1, 1);

        public RipToggleProvider(IToggleProvider toggleProvider, IAPM apm, IAPILog logger)
        {
            _toggleProvider = toggleProvider;
            _apm = apm;
            _logger = logger;
        }

        public bool IsEnabled<T>() where T : IToggle
        {
            return IsEnabledByName(typeof(T).FullName);
        }

        public async Task<bool> IsEnabledAsync<T>() where T : IToggle
        {
            return await IsEnabledByNameAsync(typeof(T).FullName).ConfigureAwait(false);
        }

        public bool IsEnabledByName(string toggleName)
        {
            _dictionarySemaphore.WaitAsync().GetAwaiter().GetResult();
            try
            {
                bool toggleValue;
                if (!_cache.TryGetValue(toggleName, out toggleValue))
                {
                    toggleValue = _toggleProvider.IsEnabledByName(toggleName);
                    SendToggleLog(toggleName, toggleValue);
                    _cache.Add(toggleName, toggleValue);
                }

                return toggleValue;
            }
            finally
            {
                _dictionarySemaphore.Release();
            }
        }

        public async Task<bool> IsEnabledByNameAsync(string toggleName)
        {
            await _dictionarySemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                bool toggleValue;
                if (!_cache.TryGetValue(toggleName, out toggleValue))
                {
                    toggleValue = await _toggleProvider.IsEnabledByNameAsync(toggleName).ConfigureAwait(false);
                    SendToggleLog(toggleName, toggleValue);
                    _cache.Add(toggleName, toggleValue);
                }

                return toggleValue;
            }
            finally
            {
                _dictionarySemaphore.Release();
            }
        }

        private void SendToggleLog(string toggleName, bool toggleValue)
        {
            try
            {
                _logger.LogInformation("Toggle {toggleName} value: {isEnabled}", toggleName, toggleValue);

                Dictionary<string, object> attrs = new Dictionary<string, object>
                {
                    { Names.R1TeamID, Values.R1TeamID },
                    { Names.ServiceName, Values.ServiceName },
                    { Names.ToggleName, toggleName },
                    { Names.ToggleValue, toggleValue.ToString() },
                };

                _apm.CountOperation(EventNames.ToggleRead, customData: attrs).Write();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred during Toggle Read event send - {toggleName}.", toggleName);
            }
        }
    }
}
