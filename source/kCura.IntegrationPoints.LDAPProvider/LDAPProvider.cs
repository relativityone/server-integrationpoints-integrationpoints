using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.LDAPProvider
{
	public class LDAPProvider : IDataSourceProvider
	{
		public System.Data.IDataReader GetData(IEnumerable<Contracts.Models.FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			LDAPSettings settings = new JSONSerializer().Deserialize<LDAPSettings>(options);
			List<string> fieldsToLoad = fields.Select(f => f.FieldIdentifier).ToList();
			string identifier = fields.Where(f => f.IsIdentifier).Select(f => f.FieldIdentifier).First();

			LDAPService ldapService = new LDAPService(settings, fieldsToLoad);
			ldapService.InitializeConnection();
			IEnumerable<SearchResult> items = ldapService.FetchItems();
			return new LDAPServiceDataReader(ldapService, entryIds, identifier, fieldsToLoad, new LDAPDataFormatterDefault(settings));
		}

		public System.Data.IDataReader GetBatchableIds(Contracts.Models.FieldEntry identifier, string options)
		{
			LDAPSettings settings = new JSONSerializer().Deserialize<LDAPSettings>(options);
			List<string> fieldsToLoad = new List<string>() { identifier.FieldIdentifier };

			LDAPService ldapService = new LDAPService(settings, fieldsToLoad);
			ldapService.InitializeConnection();
			IEnumerable<SearchResult> items = ldapService.FetchItems();
			return new LDAPDataReader(items, fieldsToLoad, new LDAPDataFormatterForBatchableIDs(settings));
		}

		public IEnumerable<Contracts.Models.FieldEntry> GetFields(string options)
		{
			LDAPSettings settings = new JSONSerializer().Deserialize<LDAPSettings>(options);
			settings.PropertyNamesOnly = true;

			LDAPService ldapService = new LDAPService(settings);
			ldapService.InitializeConnection();
			IEnumerable<SearchResult> items = ldapService.FetchItems(settings.GetPropertiesItemSearchLimit);
			List<string> fields = ldapService.GetAllProperties(items);

			return fields.Select(f => new FieldEntry() { DisplayName = f, FieldIdentifier = f }).AsEnumerable();
		}
	}
}
