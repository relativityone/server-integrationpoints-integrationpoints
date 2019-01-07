using Banzai;
using Relativity.Sync.Configuration;
using Relativity.Sync.Nodes;

namespace Relativity.Sync.Tests.Unit.Stubs
{
	internal sealed class SyncNodeStub : SyncNode<IConfiguration>
	{
		public SyncNodeStub(ICommand<IConfiguration> command, ISyncLog logger, string name) : base(command, logger)
		{
			Id = name;
		}

		public SyncNodeStub(ExecutionOptions localOptions, ICommand<IConfiguration> command, ISyncLog logger, string name) : base(localOptions, command, logger)
		{
			Id = name;
		}
	}
}