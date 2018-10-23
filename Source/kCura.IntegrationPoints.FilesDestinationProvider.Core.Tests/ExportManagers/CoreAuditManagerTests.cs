using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using NSubstitute;
using NUnit.Framework;
using ExportStatistics = Relativity.API.Foundation.ExportStatistics;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.ExportManagers
{
	[TestFixture]
	public class CoreAuditManagerTests
	{
		private IAuditRepository _auditRepository;
		private IRepositoryFactory _repositoryFactory;

		private const int _APP_ID = 123;

		[SetUp]
		public void SetUp()
		{
			_auditRepository = Substitute.For<IAuditRepository>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_repositoryFactory.GetAuditRepository(_APP_ID).Returns(_auditRepository);
		}

		[Test]
		public void AuditExportTest(
			[Values(true, false)] bool isFatalError,
			[Values(true, false)] bool expectedResult)
		{
			var auditManager = new CoreAuditManager(_repositoryFactory);
			var exportStats = new EDDS.WebAPI.AuditManagerBase.ExportStatistics();
			_auditRepository.AuditExport(Arg.Any<ExportStatistics>()).Returns(expectedResult);

			bool result = auditManager.AuditExport(_APP_ID, isFatalError, exportStats);

			_auditRepository.Received(1).AuditExport(Arg.Any<ExportStatistics>());
			Assert.AreEqual(expectedResult, result);
		}
	}
}
