﻿using Banzai;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Unit.Stubs
{
	internal sealed class NodeWithProgressStub : Node<SyncExecutionContext>
	{
		protected override Task<NodeResultStatus> PerformExecuteAsync(IExecutionContext<SyncExecutionContext> context)
		{
			context.Subject.Progress.Report(SyncJobState.Start(Id, string.Empty));
			return Task.FromResult(NodeResultStatus.Succeeded);
		}
	}
}