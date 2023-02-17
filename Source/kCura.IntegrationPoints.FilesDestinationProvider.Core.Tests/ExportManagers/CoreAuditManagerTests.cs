using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using NSubstitute;
using NUnit.Framework;
using ExportStatistics = Relativity.API.Foundation.ExportStatistics;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.ExportManagers
{
    [TestFixture, Category("Unit")]
    public class CoreAuditManagerTests
    {
        private IAuditRepository _auditRepository;
        private IRepositoryFactory _repositoryFactory;
        private CurrentUser _currentUser;
        private const int _APP_ID = 123;
        private const int _USER_ID = 9;

        [SetUp]
        public void SetUp()
        {
            _auditRepository = Substitute.For<IAuditRepository>();
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _repositoryFactory.GetAuditRepository(_APP_ID).Returns(_auditRepository);

            _currentUser = new CurrentUser(_USER_ID);

        }

        [Test]
        public void AuditExportTest(
            [Values(true, false)] bool isFatalError,
            [Values(true, false)] bool expectedResult)
        {
            var auditManager = new CoreAuditManager(_repositoryFactory, _currentUser);
            var exportStats = new EDDS.WebAPI.AuditManagerBase.ExportStatistics();
            _auditRepository.AuditExport(Arg.Any<ExportStatistics>(), _USER_ID).Returns(expectedResult);

            bool result = auditManager.AuditExport(_APP_ID, isFatalError, exportStats);

            _auditRepository.Received(1).AuditExport(Arg.Any<ExportStatistics>(), _USER_ID);
            Assert.AreEqual(expectedResult, result);
        }
    }
}
