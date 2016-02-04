using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RdoSynchronizerPush : RdoSynchronizer
	{
		public RdoSynchronizerPush(RelativityFieldQuery fieldQuery, ImportApiFactory factory)
			: base(fieldQuery, factory)
		{
		}

		private new List<Relativity.Client.Artifact> GetRelativityFields(ImportSettings settings)
		{
			var fields = FieldQuery.GetFieldsForRdo(settings.ArtifactTypeId, settings.CaseArtifactId);
			var mappableFields = GetImportApi(settings).GetWorkspaceFields(settings.CaseArtifactId, settings.ArtifactTypeId);
			return fields.Where(x => mappableFields.Any(y => y.ArtifactID == x.ArtifactID)).ToList();
		}

		public override IEnumerable<FieldEntry> GetFields(string options)
		{
			ImportSettings settings = GetSettings(options);
			var fields = GetRelativityFields(settings);
			return ParseFields(fields);
		}
	}
}