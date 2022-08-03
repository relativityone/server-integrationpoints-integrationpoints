
using NSubstitute;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
    public abstract class WebControllerTestBase : TestBase
    {
        protected ICPHelper Helper { get; private set; }
        protected ILogFactory LogFactory { get; private set; }
        protected IAPILog Logger { get; private set; }

        public override void SetUp()
        {
            Helper = Substitute.For<ICPHelper>();
            LogFactory = Substitute.For<ILogFactory>();
            Logger = Substitute.For<IAPILog>();

            Helper.GetLoggerFactory().Returns(LogFactory);
            LogFactory.GetLogger().Returns(Logger);
        }
    }
}
