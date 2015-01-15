using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.LDAPProvider
{
	[kCura.IntegrationPoints.Contracts.DataSourceProvider("5bf1f2c2-9670-4d6e-a3e9-dbc83db6c232")]
	public class LDAPProvider : IDataSourceProvider
	{
		public System.Data.IDataReader GetData(IEnumerable<Contracts.Models.FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			LDAPSettings settings = GetSettings(options);
			List<string> fieldsToLoad = fields.Select(f => f.FieldIdentifier).ToList();
			string identifier = fields.Where(f => f.IsIdentifier).Select(f => f.FieldIdentifier).First();

			LDAPService ldapService = new LDAPService(settings, fieldsToLoad);
			ldapService.InitializeConnection();
			IEnumerable<SearchResult> items = ldapService.FetchItems();
			return new LDAPServiceDataReader(ldapService, entryIds, identifier, fieldsToLoad, new LDAPDataFormatterDefault(settings));
		}

		public System.Data.IDataReader GetBatchableIds(Contracts.Models.FieldEntry identifier, string options)
		{
			LDAPSettings settings = GetSettings(options);
			List<string> fieldsToLoad = new List<string>() { identifier.FieldIdentifier };

			LDAPService ldapService = new LDAPService(settings, fieldsToLoad);
			ldapService.InitializeConnection();
			IEnumerable<SearchResult> items = ldapService.FetchItems();
			return new LDAPDataReader(items, fieldsToLoad, new LDAPDataFormatterForBatchableIDs(settings));
		}

		public IEnumerable<Contracts.Models.FieldEntry> GetFields(string options)
		{
			LDAPSettings settings = GetSettings(options);
			settings.PropertyNamesOnly = true;

			LDAPService ldapService = new LDAPService(settings);
			ldapService.InitializeConnection();
			IEnumerable<SearchResult> items = ldapService.FetchItems(settings.GetPropertiesItemSearchLimit);
			List<string> fields = ldapService.GetAllProperties(items);

			return fields.Select(f => new FieldEntry() { DisplayName = f, FieldIdentifier = f }).AsEnumerable();
		}

		private LDAPSettings GetSettings(string options)
		{
			LDAPSettings settings = (LDAPSettings)JsonConvert.DeserializeObject(options, typeof(LDAPSettings));

			if (String.IsNullOrWhiteSpace(settings.Filter)) { settings.Filter = LDAPSettings.FILTER_DEFAULT; }

			if (settings.PageSize < 1) { settings.PageSize = 1000; }

			if (settings.GetPropertiesItemSearchLimit < 1) { settings.GetPropertiesItemSearchLimit = 100; }

			if (!settings.MultiValueDelimiter.HasValue || settings.MultiValueDelimiter.ToString() == string.Empty)
			{
				//not knowing what data can look like we will assume 
				//blank entry (" ") is possible user entry as legit delimiter
				settings.MultiValueDelimiter = char.Parse(LDAPSettings.MULTIVALUEDELIMITER_DEFAULT);
			}

			return settings;
		}
	}
}




