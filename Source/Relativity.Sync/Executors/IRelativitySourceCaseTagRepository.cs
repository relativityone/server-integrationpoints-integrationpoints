﻿using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal interface IRelativitySourceCaseTagRepository
	{
		Task<RelativitySourceCaseTag> CreateAsync(int destinationWorkspaceArtifactId, int sourceWorkspaceArtifactTypeId, RelativitySourceCaseTag sourceCaseTag);
		Task<RelativitySourceCaseTag> ReadAsync(int destinationWorkspaceArtifactId, int sourceWorkspaceArtifactTypeId, int sourceWorkspaceArtifactId, string sourceInstanceName, CancellationToken token);
		Task<RelativitySourceCaseTag> UpdateAsync(int sourceWorkspaceArtifactTypeId, RelativitySourceCaseTag sourceCaseTag, CancellationToken token);
	}
}
