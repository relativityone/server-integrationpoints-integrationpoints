using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	internal interface IRelativityExportBatcher
	{
		Guid Start(Guid runId, int workspaceArtifactId, int syncConfigurationArtifactId);
		Task<RelativityObjectSlim[]> GetNextAsync(Guid token);
	}
}
