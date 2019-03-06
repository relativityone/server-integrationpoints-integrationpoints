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
		private NotFoundWorkspaceContextService _sut;

		[SetUp]
		public void SetUp()
		{
			_loggerMock = new Mock<IAPILog>();
			_loggerMock
				.Setup(x => x.ForContext<NotFoundWorkspaceContextService>())
				.Returns(_loggerMock.Object);

			_sut = new NotFoundWorkspaceContextService(_loggerMock.Object);
		}
	}
}
;