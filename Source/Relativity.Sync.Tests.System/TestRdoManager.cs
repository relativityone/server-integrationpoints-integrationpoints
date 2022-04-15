using Relativity.API;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Tests.System.Core.Stubs;

namespace Relativity.Sync.Tests.System
{
    internal class TestRdoManager : RdoManager
    {
        public TestRdoManager(IAPILog logger) : base(logger, new SourceServiceFactoryStub(), new RdoGuidProvider())
        {
        }
    }
}
