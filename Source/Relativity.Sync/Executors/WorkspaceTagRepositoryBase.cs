using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Executors
{
	internal abstract class WorkspaceTagRepositoryBase
	{


		protected static IEnumerable<RelativityObjectRef> ToMultiObjectValue(params int[] artifactIds)
		{
			return artifactIds.Select(x => new RelativityObjectRef { ArtifactID = x });
		}
	}
}