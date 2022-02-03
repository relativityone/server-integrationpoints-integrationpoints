//using kCura.IntegrationPoint.Tests.Core;
//using kCura.IntegrationPoints.Core.Factories;
//using kCura.IntegrationPoints.Core.Managers;
//using kCura.IntegrationPoints.Core.Models;
//using kCura.IntegrationPoints.Core.Services;
//using kCura.IntegrationPoints.Core.Validation.Abstract;
//using kCura.IntegrationPoints.Data;
//using kCura.IntegrationPoints.Data.Factories;
//using kCura.IntegrationPoints.Data.Repositories;
//using kCura.IntegrationPoints.Domain.Models;
//using kCura.IntegrationPoints.Synchronizers.RDO;
//using kCura.IntegrationPoints.Web.Controllers.API;
//using Newtonsoft.Json;
//using NSubstitute;
//using NUnit.Framework;
//using Relativity.API;
//using System;
//using System.Web.Http;
//using System.Web.Http.Results;

//namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
//{
//    public class ButtonStateControllerTest: TestBase
//    {
//        ButtonStateController _controller;

//        private ICPHelper _helper;
//        private IAPILog _logger;
//        private IManagerFactory _managerFactory;
//        private IRepositoryFactory _repositoryFactory;

//        private IJobHistoryManager _jobHistoryManager;
//        private IQueueManager _queueManager;
//        private IStateManager _stateManager;
//        private IIntegrationPointPermissionValidator _permissionValidator;
//        private IIntegrationPointRepository _integrationPointRepository;
//        private IProviderTypeService _providerTypeService;

//        [SetUp]
//        public override void SetUp()
//        {
//            _helper = Substitute.For<ICPHelper>();
//            _logger = Substitute.For<IAPILog>();
//            _managerFactory = Substitute.For<IManagerFactory>();
//            _repositoryFactory = Substitute.For<IRepositoryFactory>();
//            _jobHistoryManager = Substitute.For<IJobHistoryManager>();
//            _queueManager = Substitute.For<IQueueManager>();
//            _stateManager = Substitute.For<IStateManager>();
//            _permissionValidator = Substitute.For<IIntegrationPointPermissionValidator>();
//            _integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
//            _providerTypeService = Substitute.For<IProviderTypeService>();
//            _repositoryFactory = Substitute.For<IRepositoryFactory>();

//            _managerFactory.CreateJobHistoryManager().Returns(_jobHistoryManager);
//            _managerFactory.CreateQueueManager().Returns(_queueManager);
//            _managerFactory.CreateStateManager().Returns(_stateManager);

//            _controller = new ButtonStateController(_helper, _repositoryFactory, _managerFactory, _integrationPointRepository, _providerTypeService, _repositoryFactory);
//        }

//        [Test]
//        [TestCase(ProviderType.ImportLoadFile, false, true, false, false, false, true)]
//        [TestCase(ProviderType.Relativity, false, false, true, false, true, true)]
//        [TestCase(ProviderType.Relativity, true, false, false, false, false, true)]
//        public void CheckPermissionsShouldReturnButtonStateObject(ProviderType providerType, bool imageImport, bool hasErrors, bool hasJobsInQueue, 
//            bool hasErrorViewPermissions, bool hasStoppableJobs, bool hasProfileAddPermission)
//        {
//            //Arrange
//            int workspaceId = 123456;
//            int integrationPointArtifactId = 234567;
//            int sourceProvider = 123;
//            int destinationProvider = 456;
            
//            var importSettings = new ImportSettings { ImageImport = imageImport };
//            _integrationPointRepository.ReadWithFieldMappingAsync(integrationPointArtifactId).Returns(new Data.IntegrationPoint
//            {
//                HasErrors = hasErrors,
//                SourceProvider = sourceProvider,
//                DestinationProvider = destinationProvider,
//                DestinationConfiguration = JsonConvert.SerializeObject(importSettings)
//            });

//            _providerTypeService.GetProviderType(sourceProvider, destinationProvider).Returns(providerType);

//            ValidationResult validationResult = new ValidationResult()
//            {
//                IsValid = hasErrorViewPermissions
//            };
//            _permissionValidator.ValidateViewErrors(workspaceId).Returns(validationResult);

//            IPermissionRepository permissionRepository = Substitute.For<IPermissionRepository>();
//            _repositoryFactory.GetPermissionRepository(workspaceId).Returns(permissionRepository);
//            permissionRepository.UserHasArtifactTypePermission(Guid.Parse(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.Create).Returns(true);

//            _queueManager.HasJobsExecutingOrInQueue(workspaceId, integrationPointArtifactId).Returns(hasJobsInQueue);
//            StoppableJobHistoryCollection stoppableJobCollection;
//            if (hasStoppableJobs)
//            {
//                stoppableJobCollection = new StoppableJobHistoryCollection()
//                {
//                    PendingJobHistory = new[] { new JobHistory { ArtifactId = 123 }, new JobHistory { ArtifactId = 456 } },
//                    ProcessingJobHistory = new[] { new JobHistory { ArtifactId = 111 } }
//                };
//            }
//            else
//            {
//                stoppableJobCollection = new StoppableJobHistoryCollection()
//                {
//                    PendingJobHistory = Array.Empty<JobHistory>(),
//                    ProcessingJobHistory = Array.Empty<JobHistory>()
//                };
//            }
//            _jobHistoryManager.GetStoppableJobHistory(123456, integrationPointArtifactId).Returns(stoppableJobCollection);
//            ButtonStateDTO buttonState = new ButtonStateDTO
//            {
//                RunButtonEnabled = true,
//                StopButtonEnabled = hasStoppableJobs,
//                RetryErrorsButtonEnabled = false,
//                RetryErrorsButtonVisible = false,
//                ViewErrorsLinkEnabled = hasErrorViewPermissions,
//                ViewErrorsLinkVisible = hasErrors,
//                SaveAsProfileButtonVisible = hasProfileAddPermission,
//                DownloadErrorFileLinkEnabled = true,
//                DownloadErrorFileLinkVisible = true
//            };
//            _stateManager.GetButtonState(providerType, hasJobsInQueue, hasErrors, hasErrorViewPermissions, hasStoppableJobs, hasProfileAddPermission).Returns(buttonState);

//            // Act
//            IHttpActionResult response = _controller.CheckPermissions(workspaceId, integrationPointArtifactId);

//            // Assert
//            Assert.AreEqual(typeof(OkNegotiatedContentResult<ButtonStateDTO>), response.GetType());
//            Assert.AreEqual(buttonState, ((OkNegotiatedContentResult<ButtonStateDTO>)response).Content);
//        }
//    }
//}
