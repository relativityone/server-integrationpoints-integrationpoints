using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Tests.System.Core.Stubs;

namespace Relativity.Sync.Tests.System
{
    internal class TestRdoManager : RdoManager
    {
        public TestRdoManager(ISyncLog logger) : base(logger, new ServicesManagerStub(), new RdoGuidProvider())
        {
        }
    }
}