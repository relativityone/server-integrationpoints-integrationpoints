using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Nodes;

namespace Relativity.Sync.Tests.Unit.Stubs
{
    internal sealed class SyncNodeStub : SyncNode<IConfiguration>
    {
        public SyncNodeStub(ICommand<IConfiguration> command, IAPILog logger, string name) : base(command, logger)
        {
            Id = name;
        }
    }
}
