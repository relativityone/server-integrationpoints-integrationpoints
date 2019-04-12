﻿using System.Threading.Tasks;
using Banzai;
using kCura.Relativity.Client;

namespace Relativity.Sync.Tests.Unit.Stubs
{
	internal sealed class NodeWithResultStub : Node<SyncExecutionContext>
	{
		public NodeResultStatus ResultStatus { get; set; }
		
		protected override Task<NodeResultStatus> PerformExecuteAsync(IExecutionContext<SyncExecutionContext> context)
		{
			
			return Task.FromResult(ResultStatus);
		}
	}
}