using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services.Syncronizer
{
	public class RdoSyncronizer : IDataSyncronizer
	{
		public IEnumerable<FieldEntry> GetFields()
		{
			throw new NotImplementedException();
		}

		public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			throw new NotImplementedException();
		}
	}
}
