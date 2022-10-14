using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.InstanceSetting;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync
{
    internal sealed class InstanceSettings : IInstanceSettings
    {
        private const string _RELATIVITY_CORE_SETTING_SECTION = "Relativity.Core";
        private const string _SYNC_SECTION = "Relativity.Sync";

        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
        private readonly IAPILog _logger;

        public InstanceSettings(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IAPILog logger)
        {
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
            _logger = logger;
        }

        public async Task<bool> GetRestrictReferentialFileLinksOnImportAsync(bool defaultValue = default(bool))
        {
            return await GetAsync<bool>("RestrictReferentialFileLinksOnImport", _RELATIVITY_CORE_SETTING_SECTION, defaultValue).ConfigureAwait(false);
        }

        public async Task<int> GetSyncBatchSizeAsync(int defaultValue = 25000)
        {
            return await GetAsync<int>("SyncBatchSize", _SYNC_SECTION, defaultValue).ConfigureAwait(false);
        }

        public async Task<int> GetImportApiBatchSizeAsync(int defaultValue = 1000)
        {
            return await GetAsync<int>("ImportApiBatchSize", _SYNC_SECTION, defaultValue).ConfigureAwait(false);
        }

        public async Task<bool> GetShouldForceADFTransferAsync(bool defaultValue = false)
        {
            return await GetAsync<bool>("ForceADFTransfer", _SYNC_SECTION, defaultValue).ConfigureAwait(false);
        }

        public async Task<int> GetSyncMaxThreadsCountAsync(int defaultValue = 4)
        {
            return await GetAsync<int>("SyncMaxThreadsCount", _SYNC_SECTION, defaultValue).ConfigureAwait(false);
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
                using (IInstanceSettingManager instanceSettingManager = await _serviceFactoryForAdmin.CreateProxyAsync<IInstanceSettingManager>().ConfigureAwait(false))
                {
                    Services.Query query = BuildInstanceSettingQuery(name, section);
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

        private static Services.Query BuildInstanceSettingQuery(string name, string section)
        {
            return new Services.Query
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
