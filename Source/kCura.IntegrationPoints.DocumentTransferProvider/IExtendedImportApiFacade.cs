using System.Collections.Generic;

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
	public interface IExtendedImportApiFacade
	{
		HashSet<int> GetMappableArtifactIdsExcludeFields(int workspaceArtifactID, int artifactTypeID,
			HashSet<string> ignoredFields);
	}
}
