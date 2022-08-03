using System;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.Logging;

namespace Relativity.IntegrationPoints.Services.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class DocumentManagerTests : TestBase
    {
        private const int _WORKSPACE_ID = 849966;
        private DocumentManager _documentManager;
        private IPermissionRepository _permissionRepository;
        private ILog _logger;
        private IWindsorContainer _container;

        public override void SetUp()
        {
            _logger = Substitute.For<ILog>();
            _permissionRepository = Substitute.For<IPermissionRepository>();
            _container = Substitute.For<IWindsorContainer>();

            var permissionRepositoryFactory = Substitute.For<IPermissionRepositoryFactory>();
            permissionRepositoryFactory.Create(Arg.Any<IHelper>(), _WORKSPACE_ID).Returns(_permissionRepository);

            _documentManager = new DocumentManager(_logger, permissionRepositoryFactory, _container);
        }

        [Test]
        public void ItShouldGrantAccess()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);

            _documentManager.GetCurrentPromotionStatusAsync(new CurrentPromotionStatusRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            }).Wait();
            _documentManager.GetHistoricalPromotionStatusAsync(new HistoricalPromotionStatusRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            }).Wait();
            _documentManager.GetPercentagePushedToReviewAsync(new PercentagePushedToReviewRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            }).Wait();

            _permissionRepository.Received(3).UserHasPermissionToAccessWorkspace();
        }

        [Test]
        public void ItShouldDenyAccessAndLogIt()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(false);

            var currentPromotionStatusRequest = new CurrentPromotionStatusRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };
            var historicalPromotionStatusRequest = new HistoricalPromotionStatusRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };
            var percentagePushedToReviewRequest = new PercentagePushedToReviewRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };

            Assert.That(() => _documentManager.GetCurrentPromotionStatusAsync(currentPromotionStatusRequest).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            Assert.That(() => _documentManager.GetHistoricalPromotionStatusAsync(historicalPromotionStatusRequest).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            Assert.That(() => _documentManager.GetPercentagePushedToReviewAsync(percentagePushedToReviewRequest).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            _permissionRepository.Received(3).UserHasPermissionToAccessWorkspace();

            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetCurrentPromotionStatusAsync", "Workspace");
            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetHistoricalPromotionStatusAsync",
                    "Workspace");
            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetPercentagePushedToReviewAsync", "Workspace");
        }

        [Test]
        public void ItShouldReturnPercentagePushedToReview()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);

            var expectedResult = new PercentagePushedToReviewModel();

            var request = new PercentagePushedToReviewRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };
            var documentRepository = Substitute.For<Services.Repositories.IDocumentRepository>();
            documentRepository.GetPercentagePushedToReviewAsync(request).Returns(expectedResult);
            _container.Resolve<Services.Repositories.IDocumentRepository>().Returns(documentRepository);

            var actualResult = _documentManager.GetPercentagePushedToReviewAsync(request).Result;

            documentRepository.Received(1).GetPercentagePushedToReviewAsync(request);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldReturnCurrentPromotionStatus()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);

            var expectedResult = new CurrentPromotionStatusModel();

            var request = new CurrentPromotionStatusRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };
            var documentRepository = Substitute.For<Services.Repositories.IDocumentRepository>();
            documentRepository.GetCurrentPromotionStatusAsync(request).Returns(expectedResult);
            _container.Resolve<Services.Repositories.IDocumentRepository>().Returns(documentRepository);

            var actualResult = _documentManager.GetCurrentPromotionStatusAsync(request).Result;

            documentRepository.Received(1).GetCurrentPromotionStatusAsync(request);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldReturnHistoricalPromotionStatus()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);

            var expectedResult = new HistoricalPromotionStatusSummaryModel();

            var request = new HistoricalPromotionStatusRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };
            var documentRepository = Substitute.For<Services.Repositories.IDocumentRepository>();
            documentRepository.GetHistoricalPromotionStatusAsync(request).Returns(expectedResult);
            _container.Resolve<Services.Repositories.IDocumentRepository>().Returns(documentRepository);

            var actualResult = _documentManager.GetHistoricalPromotionStatusAsync(request).Result;

            documentRepository.Received(1).GetHistoricalPromotionStatusAsync(request);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldHideAndLogException()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);

            var expectedException = new ArgumentException();

            var documentRepository = Substitute.For<Services.Repositories.IDocumentRepository>();
            documentRepository.GetCurrentPromotionStatusAsync(Arg.Any<CurrentPromotionStatusRequest>()).Throws(expectedException);
            documentRepository.GetHistoricalPromotionStatusAsync(Arg.Any<HistoricalPromotionStatusRequest>()).Throws(expectedException);
            documentRepository.GetPercentagePushedToReviewAsync(Arg.Any<PercentagePushedToReviewRequest>()).Throws(expectedException);

            _container.Resolve<Services.Repositories.IDocumentRepository>().Returns(documentRepository);

            var currentPromotionStatusRequest = new CurrentPromotionStatusRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };
            var historicalPromotionStatusRequest = new HistoricalPromotionStatusRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };
            var percentagePushedToReviewRequest = new PercentagePushedToReviewRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };

            Assert.That(() => _documentManager.GetCurrentPromotionStatusAsync(currentPromotionStatusRequest).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _documentManager.GetHistoricalPromotionStatusAsync(historicalPromotionStatusRequest).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _documentManager.GetPercentagePushedToReviewAsync(percentagePushedToReviewRequest).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetCurrentPromotionStatusAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetHistoricalPromotionStatusAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetPercentagePushedToReviewAsync");
        }
    }
}