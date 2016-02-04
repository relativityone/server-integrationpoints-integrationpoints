using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;

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
			List<Artifact> fields = FieldQuery.GetFieldsForRdo(settings.ArtifactTypeId, settings.CaseArtifactId);
			HashSet<int> mappableArtifactIds = new HashSet<int>(GetImportApi(settings).GetWorkspaceFields(settings.CaseArtifactId, settings.ArtifactTypeId).Select(x => x.ArtifactID));
			return fields.Where(x => mappableArtifactIds.Contains(x.ArtifactID)).ToList();
		}

		public override IEnumerable<FieldEntry> GetFields(string options)
		{
			ImportSettings settings = GetSettings(options);
			var fields = GetRelativityFields(settings);
			return ParseFields(fields);
		}
	}
}