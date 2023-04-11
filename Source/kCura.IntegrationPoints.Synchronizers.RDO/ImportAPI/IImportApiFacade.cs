﻿using System.Collections.Generic;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
	public interface IImportApiFacade
	{
		HashSet<int> GetMappableArtifactIdsWithNotIdentifierFieldCategory(int workspaceArtifactID, int artifactTypeID);
		Dictionary<int, string> GetWorkspaceFieldsNames(int workspaceArtifactId, int artifactTypeId);
		Dictionary<int, string> GetWorkspaceNames();
	}
}