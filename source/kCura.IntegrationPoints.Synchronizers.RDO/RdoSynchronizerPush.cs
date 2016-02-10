using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RdoSynchronizerPush : RdoSynchronizerBase
	{
		public RdoSynchronizerPush(IRelativityFieldQuery fieldQuery, IImportApiFactory factory)
			: base(fieldQuery, factory)
		{
		}

		protected override List<Relativity.Client.Artifact> GetRelativityFields(ImportSettings settings)
		{
			List<Artifact> fields = FieldQuery.GetFieldsForRdo(settings.ArtifactTypeId, settings.CaseArtifactId);
			HashSet<int> mappableArtifactIds = new HashSet<int>(GetImportApi(settings).GetWorkspaceFields(settings.CaseArtifactId, settings.ArtifactTypeId).Select(x => x.ArtifactID));
			return fields.Where(x => mappableArtifactIds.Contains(x.ArtifactID)).ToList();
		}
	}
}