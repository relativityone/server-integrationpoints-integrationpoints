using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RDOCustodianSynchronizer : RdoSynchronizer
	{
		public RDOCustodianSynchronizer(RelativityFieldQuery fieldQuery, RelativityRdoQuery rdoQuery)
			: base(fieldQuery, rdoQuery)
		{
		}

		public override IEnumerable<FieldEntry> GetFields(string options)
		{
			var relativityFields = FieldQuery.GetFieldsForRDO(GetSettings(options).ArtifactTypeId);
			var fields = ParseFields(relativityFields);
			var fieldLookup = relativityFields.ToDictionary(x => x.ArtifactID.ToString(), x => x);

			foreach (var fieldEntry in fields)
			{
				fieldEntry.IsIdentifier = fieldEntry.DisplayName.Equals("UniqueID");
				if (fieldEntry.DisplayName.Equals("firstname"))
				{
					continue;
				}
				yield return fieldEntry;
			}
		}
	}
}
