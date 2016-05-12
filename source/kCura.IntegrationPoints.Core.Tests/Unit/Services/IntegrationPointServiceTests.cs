using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Services
{
	[TestFixture]
	public class IntegrationPointServiceTests
	{
		private readonly int _sourceWorkspaceArtifactId = 789;
		private readonly int _targetWorkspaceArtifactId = 9954;
		private readonly int _integrationPointArtifactId = 741;
		private readonly int _savedSearchArtifactId = 93032;
		private readonly int _sourceProviderId = 321;
		private readonly int _userId = 951;

		private IHelper _helper;
		private ICaseServiceContext _caseServiceManager;
		private IContextContainer _contextContainer;
		private IPermissionRepository _permissionRepository;
		private IContextContainerFactory _contextContainerFactory;
		private IJobManager _jobManager;
		private IQueueManager _queueManager;
		private ISerializer _serializer;
		private IJobHistoryService _jobHistoryService;
		private IManagerFactory _managerFactory;
		private Data.IntegrationPoint _integrationPoint;
		private SourceProvider _sourceProvider;

		private IntegrationPointService _instance;

		[SetUp]
		public void Setup()
		{
			_helper = Substitute.For<IHelper>();
			_caseServiceManager = Substitute.For<ICaseServiceContext>();
			_permissionRepository = Substitute.For<IPermissionRepository>();
			_contextContainer = Substitute.For<IContextContainer>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_serializer = Substitute.For<ISerializer>();
			_jobManager = Substitute.For<IJobManager>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_queueManager = Substitute.For<IQueueManager>();

			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);

			_instance = new IntegrationPointService(_helper, _caseServiceManager, _permissionRepository, _contextContainerFactory, _serializer, null, _jobManager,
				_jobHistoryService, _managerFactory);

			_caseServiceManager.RsapiService = Substitute.For<IRSAPIService>();
			_caseServiceManager.RsapiService.IntegrationPointLibrary = Substitute.For<IGenericLibrary<Data.IntegrationPoint>>();
			_caseServiceManager.RsapiService.SourceProviderLibrary = Substitute.For<IGenericLibrary<SourceProvider>>();

			_integrationPoint = new Data.IntegrationPoint { ArtifactId = _integrationPointArtifactId, EnableScheduler = false };
			_sourceProvider = new SourceProvider();
			_integrationPoint.SourceProvider = _sourceProviderId;
			_integrationPoint.SourceConfiguration = $"{{ TargetWorkspaceArtifactId : {_targetWorkspaceArtifactId}, SourceWorkspaceArtifactId : {_sourceWorkspaceArtifactId}, SavedSearchArtifactId: {_savedSearchArtifactId} }}";

			_caseServiceManager.RsapiService.IntegrationPointLibrary.Read(_integrationPointArtifactId).Returns(_integrationPoint);
			_caseServiceManager.RsapiService.SourceProviderLibrary.Read(_sourceProviderId).Returns(_sourceProvider);
		}

		[Test]
		public void RunIntegrationPoint_GoldFlow_RelativityProvider()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_permissionRepository.UserCanImport(_targetWorkspaceArtifactId).Returns(true);
			_permissionRepository.UserCanEditDocuments(_sourceWorkspaceArtifactId).Returns(true);
			_permissionRepository.UserCanViewArtifact(Arg.Is(_sourceWorkspaceArtifactId), Arg.Is((int)kCura.Relativity.Client.ArtifactType.Search), Arg.Is(_savedSearchArtifactId)).Returns(true); 
			_queueManager.HasJobsExecutingOrInQueue(_sourceWorkspaceArtifactId, _integrationPointArtifactId).Returns(false);

			// act
			_instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId);

			// assert
			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRunNow, null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.ExportService, _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
			_managerFactory.Received().CreateQueueManager(_contextContainer);
		}

		[Test]
		public void RunIntegrationPoint_RelativityProvider_UserHasNoPermissionToExportToTheTargetWorkspace()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_permissionRepository.UserCanImport(_targetWorkspaceArtifactId).Returns(false);

			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId), Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT);

			// assert
			_jobHistoryService.DidNotReceive().CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
		}

		[Test]
		public void RunIntegrationPoint_RelativityProvider_UserIdZero()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_permissionRepository.UserCanImport(_targetWorkspaceArtifactId).Returns(true);
			_permissionRepository.UserCanEditDocuments(Arg.Any<int>()).Returns(true);

			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, 0), Constants.IntegrationPoints.NO_USERID);

			// assert
			_jobHistoryService.DidNotReceive().CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
		}

		[Test]
		public void RunIntegrationPoint_RelativityProvider_UserHasNoPermissionToMassEditDocs()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_permissionRepository.UserCanImport(_targetWorkspaceArtifactId).Returns(true);
			_permissionRepository.UserCanEditDocuments(Arg.Any<int>()).Returns(false);

			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, 0), Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS);

			// assert
			_jobHistoryService.Received(0).CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), null);
			_jobManager.Received(0).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
		}

		[Test]
		public void RunIntegrationPoint_RelativityProvider_JobsCurrentlyRunning()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_managerFactory.CreateQueueManager(_contextContainer).Returns(_queueManager);

			_permissionRepository.UserCanImport(_targetWorkspaceArtifactId).Returns(true);
			_permissionRepository.UserCanEditDocuments(Arg.Any<int>()).Returns(true);
			_permissionRepository.UserCanViewArtifact(Arg.Is(_sourceWorkspaceArtifactId), Arg.Is((int)kCura.Relativity.Client.ArtifactType.Search), Arg.Is(_savedSearchArtifactId)).Returns(true); 
			_queueManager.HasJobsExecutingOrInQueue(_sourceWorkspaceArtifactId, _integrationPointArtifactId).Returns(true);
			
			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, 12345), Constants.IntegrationPoints.JOBS_ALREADY_RUNNING);

			// assert
			_queueManager.Received().HasJobsExecutingOrInQueue(_sourceWorkspaceArtifactId, _integrationPointArtifactId);
			_jobHistoryService.DidNotReceive().CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
			_managerFactory.Received().CreateQueueManager(_contextContainer);
		}

		[Test]
		public void RunIntegrationPoint_GoldFlow_OtherProviders()
		{
			// arrange
			_sourceProvider.Identifier = "some thing else";
			_permissionRepository.UserCanImport(Arg.Any<int>()).Returns(true);
			_permissionRepository.UserCanEditDocuments(Arg.Any<int>()).Returns(true);

			// act
			_instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId);

			// assert
			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRunNow, null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.SyncManager, _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RunIntegrationPoint_GoldFlow_OtherProviders_NoImportCheck()
		{
			// arrange
			_sourceProvider.Identifier = "some thing else";
			_permissionRepository.UserCanImport(Arg.Any<int>()).Returns(false);

			// act
			_instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId);

			// assert
			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRunNow, null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.SyncManager, _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RunIntegrationPoint_GoldFlow_OtherProviders_NoMassEditCheck()
		{
			// arrange
			_sourceProvider.Identifier = "some thing else";
			_permissionRepository.UserCanImport(Arg.Any<int>()).Returns(true);
			_permissionRepository.UserCanEditDocuments(Arg.Any<int>()).Returns(false);

			// act
			_instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId);

			// assert
			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRunNow, null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.SyncManager, _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RetryIntegrationPoint_IntegrationPointHasNoErrors_ThrowsException_Test()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_permissionRepository.UserCanImport(_targetWorkspaceArtifactId).Returns(true);
			_permissionRepository.UserCanEditDocuments(_sourceWorkspaceArtifactId).Returns(true);
			_integrationPoint.HasErrors = null;

			// Act
			Assert.Throws<Exception>(() =>
				_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId),
				Constants.IntegrationPoints.RETRY_NO_EXISTING_ERRORS);

			// Assert
			_caseServiceManager.RsapiService.SourceProviderLibrary.Received(1).Read(_integrationPoint.SourceProvider.Value);
			_permissionRepository.Received(1).UserCanImport(_targetWorkspaceArtifactId);
			_permissionRepository.Received(1).UserCanEditDocuments(_sourceWorkspaceArtifactId);
			
			_jobHistoryService.DidNotReceive().CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRetryErrors, null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RetryIntegrationPoint_UserHasNoImportPermission_ThrowsException_Test()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_permissionRepository.UserCanImport(_targetWorkspaceArtifactId).Returns(false);

			// Act
			Assert.Throws<Exception>(() =>
				_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId),
				Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT);

			// Assert
			_caseServiceManager.RsapiService.SourceProviderLibrary.Received(1).Read(_integrationPoint.SourceProvider.Value);
			_permissionRepository.Received(1).UserCanImport(_targetWorkspaceArtifactId);

			_permissionRepository.DidNotReceive().UserCanEditDocuments(_sourceWorkspaceArtifactId);
			_jobHistoryService.DidNotReceive().CreateRdo(_integrationPoint, Arg.Any<Guid>(),JobTypeChoices.JobHistoryRetryErrors, null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RetryIntegrationPoint_UserHasNoEditDocumentsPermission_ThrowsException_Test()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_permissionRepository.UserCanImport(_targetWorkspaceArtifactId).Returns(true);
			_permissionRepository.UserCanEditDocuments(_sourceWorkspaceArtifactId).Returns(false);

			// Act
			Assert.Throws<Exception>(() =>
				_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId),
				Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS);

			// Assert
			_caseServiceManager.RsapiService.SourceProviderLibrary.Received(1).Read(_integrationPoint.SourceProvider.Value);
			_permissionRepository.Received(1).UserCanImport(_targetWorkspaceArtifactId);
			_permissionRepository.Received(1).UserCanEditDocuments(_sourceWorkspaceArtifactId);
			
			_jobHistoryService.DidNotReceive().CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRetryErrors, null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RetryIntegrationPoint_SourceProviderIsNotRelativity_ThrowsException_Test()
		{
			// Arrange
			_sourceProvider.Identifier = "Not a Relativity Provider";

			// Act
			Assert.Throws<Exception>(() =>
				_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId),
				Constants.IntegrationPoints.RETRY_IS_NOT_RELATIVITY_PROVIDER);

			// Assert
			_caseServiceManager.RsapiService.SourceProviderLibrary.Received(1).Read(_integrationPoint.SourceProvider.Value);

			_permissionRepository.DidNotReceive().UserCanImport(_targetWorkspaceArtifactId);
			_permissionRepository.DidNotReceive().UserCanEditDocuments(_sourceWorkspaceArtifactId);
			_jobHistoryService.DidNotReceive().CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRetryErrors, null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RetryIntegrationPoint_GoldFlow_Test()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_permissionRepository.UserCanImport(_targetWorkspaceArtifactId).Returns(true);
			_permissionRepository.UserCanEditDocuments(_sourceWorkspaceArtifactId).Returns(true);
			_permissionRepository.UserCanViewArtifact(Arg.Is(_sourceWorkspaceArtifactId), Arg.Is((int)kCura.Relativity.Client.ArtifactType.Search), Arg.Is(_savedSearchArtifactId)).Returns(true);
			_integrationPoint.HasErrors = true;

			// Act
			_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId);

			// Assert
			_caseServiceManager.RsapiService.SourceProviderLibrary.Received(1).Read(_integrationPoint.SourceProvider.Value);
			_permissionRepository.Received(1).UserCanImport(_targetWorkspaceArtifactId);
			_permissionRepository.Received(1).UserCanEditDocuments(_sourceWorkspaceArtifactId);
			_permissionRepository.Received(1).UserCanViewArtifact(Arg.Is(_sourceWorkspaceArtifactId), Arg.Is((int)kCura.Relativity.Client.ArtifactType.Search), Arg.Is(_savedSearchArtifactId));
			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRetryErrors, null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RetryIntegrationPoint_InvalidSavedSearchPermissions_Excepts()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_permissionRepository.UserCanImport(_targetWorkspaceArtifactId).Returns(true);
			_permissionRepository.UserCanEditDocuments(_sourceWorkspaceArtifactId).Returns(true);
			_permissionRepository.UserCanViewArtifact(Arg.Is(_sourceWorkspaceArtifactId), Arg.Is((int)kCura.Relativity.Client.ArtifactType.Search), Arg.Is(_savedSearchArtifactId)).Returns(false);
			_integrationPoint.HasErrors = true;

			// Act
			Assert.Throws<Exception>(() => _instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId), Constants.IntegrationPoints.NO_PERMISSION_TO_ACCESS_SAVEDSEARCH);

			// Assert
			_caseServiceManager.RsapiService.SourceProviderLibrary.Received(1).Read(_integrationPoint.SourceProvider.Value);
			_permissionRepository.Received(1).UserCanImport(_targetWorkspaceArtifactId);
			_permissionRepository.Received(1).UserCanEditDocuments(_sourceWorkspaceArtifactId);
			_permissionRepository.Received(1).UserCanViewArtifact(Arg.Is(_sourceWorkspaceArtifactId), Arg.Is((int)kCura.Relativity.Client.ArtifactType.Search), Arg.Is(_savedSearchArtifactId));
			_jobManager.Received(0).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}


		[Test]
		public void RunIntegrationPoint_GetSourceProvider_ProviderIsNull_ThrowsException_Test()
		{
			// Arrange
			_integrationPoint.SourceProvider = null;

			// Act
			Assert.Throws<Exception>(() =>
			_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId),
				Constants.IntegrationPoints.NO_SOURCE_PROVIDER_SPECIFIED);

			// Assert
			_caseServiceManager.RsapiService.SourceProviderLibrary.DidNotReceiveWithAnyArgs().Read(Arg.Any<int>());
			_permissionRepository.DidNotReceive().UserCanImport(_targetWorkspaceArtifactId);
			_permissionRepository.DidNotReceive().UserCanEditDocuments(_sourceWorkspaceArtifactId);
			_jobHistoryService.DidNotReceive().CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRunNow, null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}
	}
}