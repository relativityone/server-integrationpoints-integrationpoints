using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Security;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.LDAPProvider
{
	[Contracts.DataSourceProvider("5bf1f2c2-9670-4d6e-a3e9-dbc83db6c232")]
	public class LDAPProvider : IDataSourceProvider
	{
		private readonly IEncryptionManager _encryptionManager;
		private readonly IAPILog _logger;

		public LDAPProvider(IEncryptionManager encryptionManager, IHelper helper)
		{
			_encryptionManager = encryptionManager;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<LDAPProvider>();
		}

		public System.Data.IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds,
			string options)
		{
			LogRetrievingData(options, entryIds);

			LDAPSettings settings = GetSettings(options);
			List<string> fieldsToLoad = fields.Select(f => f.FieldIdentifier).ToList();
			string identifier = fields.Where(f => f.IsIdentifier).Select(f => f.FieldIdentifier).First();

			LDAPService ldapService = new LDAPService(_logger, settings, fieldsToLoad);
			ldapService.InitializeConnection();
			IEnumerable<SearchResult> items = ldapService.FetchItems();
			return new LDAPServiceDataReader(ldapService, entryIds, identifier, fieldsToLoad,
				new LDAPDataFormatterDefault(settings));
		}

		public System.Data.IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			LogRetrievingBatchableIds(options, identifier);

			LDAPSettings settings = GetSettings(options);
			List<string> fieldsToLoad = new List<string>() {identifier.FieldIdentifier};

			LDAPService ldapService = new LDAPService(_logger, settings, fieldsToLoad);
			ldapService.InitializeConnection();
			IEnumerable<SearchResult> items = ldapService.FetchItems();
			return new LDAPDataReader(items, fieldsToLoad, new LDAPDataFormatterForBatchableIDs(settings));
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			LogRetrievingFields(options);

			LDAPSettings settings = GetSettings(options);
			settings.PropertyNamesOnly = true;

			LDAPService ldapService = new LDAPService(_logger, settings);
			ldapService.InitializeConnection();
			IEnumerable<SearchResult> items = ldapService.FetchItems(settings.GetPropertiesItemSearchLimit);
			List<string> fields = ldapService.GetAllProperties(items);

			return fields.Select(f => new FieldEntry() {DisplayName = f, FieldIdentifier = f});
		}

		public LDAPSettings GetSettings(string options)
		{
			try
			{
				options = JsonConvert.DeserializeObject<string>(options);
			}
			catch (JsonReaderException exception)
			{
				LogSettingsDeserializationError(exception, options);
			}

			options = _encryptionManager.Decrypt(options);
			LDAPSettings settings = JsonConvert.DeserializeObject<LDAPSettings>(options);

			if (String.IsNullOrWhiteSpace(settings.Filter))
			{
				settings.Filter = LDAPSettings.FILTER_DEFAULT;
			}

			if (settings.PageSize < 1)
			{
				settings.PageSize = 1000;
			}

			if (settings.GetPropertiesItemSearchLimit < 1)
			{
				settings.GetPropertiesItemSearchLimit = 100;
			}

			if (!settings.MultiValueDelimiter.HasValue || settings.MultiValueDelimiter.ToString() == string.Empty)
			{
				//not knowing what data can look like we will assume 
				//blank entry (" ") is possible user entry as legit delimiter
				LogUsageOfDefaultMultiValueDelimiter(options);
				settings.MultiValueDelimiter = char.Parse(LDAPSettings.MULTIVALUEDELIMITER_DEFAULT);
			}

			return settings;
		}

		#region Logging

		private void LogRetrievingFields(string options)
		{
			_logger.LogInformation("Attempting to retrieve fields in LDAP Provider with {Options}.", options);
		}

		private void LogRetrievingData(string options, IEnumerable<string> entryIds )
		{
			_logger.LogInformation("Attempting to retrieve data in LDAP Provider with {Options} for ids: {Ids}.", options,
				string.Join(",", entryIds));
		}

		private void LogRetrievingBatchableIds(string options, FieldEntry entry)
		{
			_logger.LogInformation(
				"Attempting to retrieve batchable ids in LDAP Provider with {Options} for field {FieldIdentifier}", options,
				entry.FieldIdentifier);
		}

		private void LogUsageOfDefaultMultiValueDelimiter(string options)
		{
			_logger.LogWarning(
				"LDAPSettings does not contain Multivalue delimiter. Using default delimiter: ({DefaultDelimiter}) for {Options}",
				LDAPSettings.MULTIVALUEDELIMITER_DEFAULT, options);
		}

		private void LogSettingsDeserializationError(Exception ex, string options)
		{
			_logger.LogError(ex, "Error occured in {MethodName} while deserializing LDAP settings with {Options}.",
				nameof(GetSettings), options);
		}

		#endregion
	}
}