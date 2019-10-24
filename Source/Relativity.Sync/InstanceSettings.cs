using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.InstanceSetting;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync
{
	internal sealed class InstanceSettings : IInstanceSettings
	{
		private const string _INTEGRATION_POINTS_SETTING_SECTION = "kCura.IntegrationPoints";
		private const string _RELATIVITY_CORE_SETTING_SECTION = "Relativity.Core";
		
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly ISyncLog _logger;

		public InstanceSettings(ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<string> GetWebApiPathAsync(string defaultValue = default(string)) =>
			await GetAsync<string>("WebAPIPath", _INTEGRATION_POINTS_SETTING_SECTION, defaultValue)
				.ConfigureAwait(false);

		public async Task<bool> GetRestrictReferentialFileLinksOnImportAsync(bool defaultValue = default(bool)) => 
			await GetAsync<bool>("RestrictReferentialFileLinksOnImport", _RELATIVITY_CORE_SETTING_SECTION, defaultValue)
				.ConfigureAwait(false);


		private async Task<T> GetAsync<T>(string name, string section, T defaultValue)
		{
			InstanceSettingQueryResultSet resultSet = await TryReadInstanceSettingAsync(name, section).ConfigureAwait(false);

			if (!resultSet.Success)
			{
				LogWarningRetrieveSettingValueFailed(name, $"Failed to query for '{name}' instance setting. Response message: {resultSet.Message}");
				return defaultValue;
			}

			if (resultSet.TotalCount == 0)
			{
				LogWarningRetrieveSettingValueFailed(name, $"Query for '{name}' instance setting from section '{section}' returned empty results. Make sure instance setting exists.");
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
					Services.Query query = BuildInstanceSettingQuery(name, section);
					InstanceSettingQueryResultSet resultSet = await instanceSettingManager.QueryAsync(query).ConfigureAwait(false);

					return resultSet;
				}
			}
			catch(InvalidOperationException ex)
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

		private void LogWarningRetrieveSettingValueFailed(string name, string message)
		{
			_logger.LogWarning($"Warning: Retrieve {name} setting failed, default value was returned (Error Message: {message})");
		}
	}
}