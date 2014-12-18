using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Provider;

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
//			DirectorySearcher src = new DirectorySearcher("…");
//src.PropertiesToLoad = new string[] {ntSecurityDescriptor,…};
//src.ExtendedDN = ExtendedDN.HexString;
//SearchResultCollection res = src.FindAll();

		}

		public IEnumerable<Contracts.Models.FieldEntry> GetFields(string options)
		{
			throw new NotImplementedException();
		}
	}
}
