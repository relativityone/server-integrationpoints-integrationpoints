﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal interface IRelativityExportBatcher
	{
		string Start(Guid runId, int workspaceArtifactId, int syncConfigurationArtifactId);
		Task<RelativityObjectSlim[]> GetNextAsync(string token);
	}
}
