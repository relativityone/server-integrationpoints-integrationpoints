using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Context.WorkspaceContext
{
	[TestFixture]
	public class LastWorkspaceContextServiceTests
	{
		private Mock<IAPILog> _loggerMock;
		private LastWorkspaceContextService _sut;

		[SetUp]
		public void SetUp()
		{
			_loggerMock = new Mock<IAPILog>();
			_loggerMock
				.Setup(x => x.ForContext<LastWorkspaceContextService>())
				.Returns(_loggerMock.Object);

			_sut = new LastWorkspaceContextService(_loggerMock.Object);
		}
	}
}
;