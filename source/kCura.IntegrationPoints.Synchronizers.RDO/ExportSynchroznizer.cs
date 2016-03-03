using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Synchronizer;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class ExportSynchroznizer : IDataSynchronizer
	{
		public IEnumerable<FieldEntry> GetFields(string options)
		{
			return new List<FieldEntry>();
		}

		public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options)
		{
		}

		public void SyncData(IDataReader data, IEnumerable<FieldMap> fieldMap, string options)
		{
		}
	}
}