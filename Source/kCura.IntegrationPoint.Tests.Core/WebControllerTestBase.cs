
using NSubstitute;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
	public abstract class WebControllerTestBase : TestBase
	{
		protected ICPHelper Helper;
		protected ILogFactory LogFactory;
		protected IAPILog Logger;

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
