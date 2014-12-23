using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using NSubstitute;

namespace kCura.IntegrationPoints.LDAPProvider
{
	public class LDAPProvider : IDataSourceProvider
	{
		public System.Data.IDataReader GetData(IEnumerable<Contracts.Models.FieldEntry> entries, IEnumerable<string> entryIds, string options)
		{
			throw new NotImplementedException();
		}

		public System.Data.IDataReader GetBatchableIds(Contracts.Models.FieldEntry identifier, string options)
		{
			throw new NotImplementedException();
			
			LDAPSettings settings = new JSONSerializer().Deserialize<LDAPSettings>(options);
			
			LDAPService ldapService = new LDAPService(settings);
			ldapService.InitializeConnection();
			IEnumerable<SearchResult> items = ldapService.FetchItems(settings.GetPropertiesItemSearchLimit);
			return new LDAPDataReader(items, new List<string>() { identifier.FieldIdentifier });
		}

		public IEnumerable<Contracts.Models.FieldEntry> GetFields(string options)
		{
			LDAPSettings settings = new JSONSerializer().Deserialize<LDAPSettings>(options);

			LDAPService ldapService = new LDAPService(settings);
			ldapService.InitializeConnection();
			IEnumerable<SearchResult> items = ldapService.FetchItems(settings.GetPropertiesItemSearchLimit);
			List<string> fields = ldapService.GetAllProperties(items);

			return fields.Select(f => new FieldEntry() { DisplayName = f, FieldIdentifier = f }).AsEnumerable();
		}
	}
}
