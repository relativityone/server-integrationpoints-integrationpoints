using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using Relativity.API;

namespace kCura.IntegrationPoints.LDAPProvider
{
	[Contracts.DataSourceProvider("5bf1f2c2-9670-4d6e-a3e9-dbc83db6c232")]
	public class LDAPProvider : IDataSourceProvider
	{
		private readonly IAPILog _logger;
	    private readonly ILDAPSettingsReader _reader;
	    private readonly IHelper _helper;
	    private readonly ILDAPServiceFactory _ldapServiceFactory;


	    public LDAPProvider(ILDAPSettingsReader reader, ILDAPServiceFactory ldapServiceFactory, IHelper helper)
		{
		    _reader = reader;
		    _helper = helper;
		    _ldapServiceFactory = ldapServiceFactory;
		    _logger = helper.GetLoggerFactory().GetLogger().ForContext<LDAPProvider>();
		}

		public System.Data.IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds,
			string options)
		{
			LogRetrievingData(entryIds);

			LDAPSettings settings = _reader.GetSettings(options);
			List<string> fieldsToLoad = fields.Select(f => f.FieldIdentifier).ToList();
			string identifier = fields.Where(f => f.IsIdentifier).Select(f => f.FieldIdentifier).First();

			ILDAPService ldapService = _ldapServiceFactory.Create(_logger, settings, fieldsToLoad);
			ldapService.InitializeConnection();
			IEnumerable<SearchResult> items = ldapService.FetchItems();
			return new LDAPServiceDataReader(ldapService, entryIds, identifier, fieldsToLoad,
				new LDAPDataFormatterDefault(settings, _helper));
		}

		public System.Data.IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
		    if (identifier == null)
		    {
		        throw new ArgumentNullException($"Argument named '{nameof(identifier)}' cannot be null.");
		    }
			LogRetrievingBatchableIds(identifier);

			LDAPSettings settings = _reader.GetSettings(options);
			var fieldsToLoad = new List<string> {identifier.FieldIdentifier};

			ILDAPService ldapService = _ldapServiceFactory.Create(_logger, settings, fieldsToLoad);
			ldapService.InitializeConnection();
			IEnumerable<SearchResult> items = ldapService.FetchItems();
			return new LDAPDataReader(items, fieldsToLoad, new LDAPDataFormatterForBatchableIDs(settings, _helper));
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			LogRetrievingFields();

			LDAPSettings settings = _reader.GetSettings(options);
			settings.PropertyNamesOnly = true;

			ILDAPService ldapService = _ldapServiceFactory.Create(_logger, settings);
			ldapService.InitializeConnection();
		    List<string> fields = ldapService.FetchAllProperties(settings.GetPropertiesItemSearchLimit);

			return fields.Select(f => new FieldEntry {DisplayName = f, FieldIdentifier = f});
		}

		#region Logging

		private void LogRetrievingFields()
		{
			_logger.LogInformation("Attempting to retrieve fields in LDAP Provider.");
		}

		private void LogRetrievingData(IEnumerable<string> entryIds )
		{
			_logger.LogInformation("Attempting to retrieve data in LDAP Provider for ids: {Ids}.", string.Join(",", entryIds));
		}

		private void LogRetrievingBatchableIds(FieldEntry entry)
		{
			_logger.LogInformation("Attempting to retrieve batchable ids in LDAP Provider for field {FieldIdentifier}",
				entry.FieldIdentifier);
		}

		#endregion
	}
}