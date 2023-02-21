using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Kepler;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.InstanceSetting;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.InstanceSettings
{
    public class InstanceSettings : IInstanceSettings
    {
        private const string _INTEGRATION_POINTS_SECTION = "kCura.IntegrationPoints";

        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IAPILog _logger;

        public InstanceSettings(IKeplerServiceFactory serviceFactory, IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _logger = logger;
        }

        public async Task<int> GetCustomProviderBatchSizeAsync()
        {
            return await GetAsync<int>("CustomProviderBatchSize", _INTEGRATION_POINTS_SECTION, 10000);
        }

        public async Task<T> GetAsync<T>(string name, string section, T defaultValue)
        {
            InstanceSettingQueryResultSet resultSet = await TryReadInstanceSettingAsync(name, section).ConfigureAwait(false);

            if (!resultSet.Success)
            {
                LogRetrieveSettingValueFailed(section, name, defaultValue, $"Failed to query for '{name}' instance setting. Response message: {resultSet.Message}");
                return defaultValue;
            }

            if (resultSet.TotalCount == 0)
            {
                LogRetrieveSettingValueFailed(section, name, defaultValue, $"Query for '{name}' instance setting from section '{section}' returned empty results. Make sure instance setting exists.");
                return defaultValue;
            }

            if (resultSet.TotalCount < 0)
            {
                _logger.LogError($"Query for instance setting returned negative value ({resultSet.TotalCount}). It can potentially cause problems");
                return defaultValue;
            }

            string instanceSettingValue = resultSet.Results.Single().Artifact.Value;
            return TryConvertValue<T>(instanceSettingValue, out T outVal)
                ? outVal
                : defaultValue;
        }

        private async Task<InstanceSettingQueryResultSet> TryReadInstanceSettingAsync(string name, string section)
        {
            try
            {
                using (IInstanceSettingManager instanceSettingManager = await _serviceFactory.CreateProxyAsync<IInstanceSettingManager>().ConfigureAwait(false))
                {
                    Query query = BuildInstanceSettingQuery(name, section);
                    InstanceSettingQueryResultSet resultSet = await instanceSettingManager.QueryAsync(query).ConfigureAwait(false);

                    return resultSet;
                }
            }
            catch (InvalidOperationException ex)
            {
                return new InstanceSettingQueryResultSet()
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private static Query BuildInstanceSettingQuery(string name, string section)
        {
            return new Query
            {
                Condition = $"'Name' == '{name}' AND 'Section' == '{section}'"
            };
        }

        private bool TryConvertValue<T>(object value, out T outVal)
        {
            outVal = default(T);
            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                outVal = (T)converter.ConvertFrom(value);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, $"Error occured while converting instance setting value ({value.GetType()} -> {typeof(T)})");
                return false;
            }

            return true;
        }

        private void LogRetrieveSettingValueFailed<T>(string section, string name, T defaultValue, string errorMessage)
        {
            _logger.LogInformation($"Warning: Retrieve '{name}' setting from section '{section}' failed, default value was returned: '{defaultValue}' (Error Message: {errorMessage})");
        }
    }
}
