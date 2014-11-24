using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RdoSynchronizer : kCura.IntegrationPoints.Core.Services.Syncronizer.IDataSyncronizer
	{
		private RelativityFieldQuery _fieldQuery;
		public RdoSynchronizer(RelativityFieldQuery fieldQuery)
		{
			_fieldQuery = fieldQuery;
		}

		private List<string> IgnoredList
		{
			get
			{
				// fields don't have any space in between words 
				var list = new List<string>
			    {
					"IsSystemArtifact",
					"SystemCreatedBy",
					"SystemCreatedOn",
					"SystemGenerated",
					"SystemLastModifiedBy",
					"SystemLastModifiedOn",
					"ArtifactID"
			    };
				return list;
			}
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			var json = JsonConvert.DeserializeObject<Core.Models.SyncConfiguration.RelativityConfiguration>(options);
			var fields = _fieldQuery.GetFieldsForRDO(json.ArtifactTypeID);
			var allFieldsForRdo = new List<FieldEntry>();
			foreach (var result in fields)
			{
				if (!IgnoredList.Contains(result.Name))
				{
					allFieldsForRdo.Add(new FieldEntry() { DisplayName = result.Name, FieldIdentifier = result.ArtifactID.ToString() });
				}
			}
			return allFieldsForRdo;
		}

		public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options)
		{
			throw new NotImplementedException();
		}
	}
}
