using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services.Provider
{
	public class LdapDataProvider : IDataSourceProvider
	{
		public IEnumerable<FieldEntry> GetFields()
		{
			throw new NotImplementedException();
		}

		public IDataReader GetData(IEnumerable<FieldEntry> entries, IEnumerable<string> entryIds, string options)
		{
			throw new NotImplementedException();
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			throw new NotImplementedException();
		}
	}
}
