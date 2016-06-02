using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Tests.Helpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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
		private IRepositoryFactory _repositoryFactory;
		private IPermissionRepository _sourcePermissionRepository;
		private IPermissionRepository _targetPermissionRepository;
		private IContextContainerFactory _contextContainerFactory;
		private IJobManager _jobManager;
		private IQueueManager _queueManager;
		private ISerializer _serializer;
		private IJobHistoryService _jobHistoryService;
		private IManagerFactory _managerFactory;
		private Data.IntegrationPoint _integrationPoint;
		private IntegrationPointDTO _integrationPointDto;
		private SourceProvider _sourceProvider;
		private IIntegrationPointManager _integrationPointManager;
		private IErrorManager _errorManager;

		private IntegrationPointService _instance;
		private IChoiceQuery _choiceQuery;

		[SetUp]
		public void Setup()
		{
			_helper = Substitute.For<IHelper>();
			_caseServiceManager = Substitute.For<ICaseServiceContext>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_sourcePermissionRepository = Substitute.For<IPermissionRepository>();
			_targetPermissionRepository = Substitute.For<IPermissionRepository>();
			_contextContainer = Substitute.For<IContextContainer>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_serializer = Substitute.For<ISerializer>();
			_jobManager = Substitute.For<IJobManager>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_queueManager = Substitute.For<IQueueManager>();
			_choiceQuery = Substitute.For<IChoiceQuery>();
			_integrationPointManager = Substitute.For<IIntegrationPointManager>();
			_errorManager = Substitute.For<IErrorManager>();
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);

			_instance = Substitute.ForPartsOf<IntegrationPointService>(_helper, _caseServiceManager,
				_contextContainerFactory, _repositoryFactory, _serializer, _choiceQuery, _jobManager,
				_jobHistoryService, _managerFactory);

			_caseServiceManager.RsapiService = Substitute.For<IRSAPIService>();
			_caseServiceManager.RsapiService.IntegrationPointLibrary = Substitute.For<IGenericLibrary<Data.IntegrationPoint>>();
			_caseServiceManager.RsapiService.SourceProviderLibrary = Substitute.For<IGenericLibrary<SourceProvider>>();

			_repositoryFactory.GetPermissionRepository(_sourceWorkspaceArtifactId).Returns(_sourcePermissionRepository);
			_repositoryFactory.GetPermissionRepository(_targetWorkspaceArtifactId).Returns(_targetPermissionRepository);
			_managerFactory.CreateIntegrationPointManager(Arg.Is(_contextContainer)).Returns(_integrationPointManager);
			_managerFactory.CreateErrorManager(Arg.Is(_contextContainer)).Returns(_errorManager);

			_integrationPoint = new Data.IntegrationPoint { ArtifactId = _integrationPointArtifactId, EnableScheduler = false };

			_integrationPoint = new Data.IntegrationPoint
			{
				ArtifactId = _integrationPointArtifactId,
				Name = "IP Name",
				DestinationConfiguration = "",
				DestinationProvider = 0,
				EmailNotificationRecipients = "emails",
				EnableScheduler = false,
				FieldMappings = "",
				HasErrors = false,
				JobHistory = null,
				LastRuntimeUTC = null,
				LogErrors = false,
				SourceProvider = _sourceProviderId,
				SourceConfiguration = $"{{ TargetWorkspaceArtifactId : {_targetWorkspaceArtifactId}, SourceWorkspaceArtifactId : {_sourceWorkspaceArtifactId}, SavedSearchArtifactId: {_savedSearchArtifactId} }}",
				NextScheduledRuntimeUTC = null,
				//				OverwriteFields = integrationPoint.OverwriteFields, -- This would require further transformation
				ScheduleRule = ""
			};
			_sourceProvider = new SourceProvider();
			_integrationPointDto = new IntegrationPointDTO()
			{
				ArtifactId = _integrationPoint.ArtifactId,
				Name = _integrationPoint.Name,
				DestinationConfiguration = _integrationPoint.DestinationConfiguration,
				DestinationProvider = _integrationPoint.DestinationProvider,
				EmailNotificationRecipients = _integrationPoint.EmailNotificationRecipients,
				EnableScheduler = _integrationPoint.EnableScheduler,
				FieldMappings = _integrationPoint.FieldMappings,
				HasErrors = _integrationPoint.HasErrors,
				JobHistory = _integrationPoint.JobHistory,
				LastRuntimeUTC = _integrationPoint.LastRuntimeUTC,
				LogErrors = _integrationPoint.LogErrors,
				SourceProvider = _integrationPoint.SourceProvider,
				SourceConfiguration = _integrationPoint.SourceConfiguration,
				NextScheduledRuntimeUTC = _integrationPoint.NextScheduledRuntimeUTC,
				//				OverwriteFields = _integrationPoint.OverwriteFields, -- This would require further transformation
				ScheduleRule = _integrationPoint.ScheduleRule
			};

			_caseServiceManager.RsapiService.IntegrationPointLibrary.Read(_integrationPointArtifactId).Returns(_integrationPoint);
			_caseServiceManager.RsapiService.SourceProviderLibrary.Read(_sourceProviderId).Returns(_sourceProvider);
		}

		[Test]
		public void RunIntegrationPoint_GoldFlow_RelativityProvider()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId), 
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity))
				.Returns(new PermissionCheckDTO() {Success = true});
			_queueManager.HasJobsExecutingOrInQueue(_sourceWorkspaceArtifactId, _integrationPointArtifactId).Returns(false);

			// act
			_instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId);

			// assert
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity));
			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRunNow, null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.ExportService, _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
			_managerFactory.Received().CreateQueueManager(_contextContainer);
		}

		[Test]
		public void RunIntegrationPoint_RelativityProvider_InvalidPermissions_ThrowsException()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;

			string[] errorMessages = {"Uh", "oh"};
			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity))
				.Returns(new PermissionCheckDTO() { Success = false, ErrorMessages = errorMessages });

			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId), Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT_CURRENTWORKSPACE);

			// assert
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity));
			_jobHistoryService.DidNotReceive().CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
		}

		[Test]
		public void RunIntegrationPoint_RelativityProvider_UserIdZero()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;

			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, 0), Constants.IntegrationPoints.NO_USERID);

			// assert
			_integrationPointManager.Received(0).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity));
			_jobHistoryService.DidNotReceive().CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
		}

		[Test]
		public void RunIntegrationPoint_RelativityProvider_JobsCurrentlyRunning()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_managerFactory.CreateQueueManager(_contextContainer).Returns(_queueManager);

			_integrationPointManager.UserHasPermissionToRunJob(
					Arg.Is(_sourceWorkspaceArtifactId),
					Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
					Arg.Is(Constants.SourceProvider.Relativity))
					.Returns(new PermissionCheckDTO() { Success = true });
			_queueManager.HasJobsExecutingOrInQueue(_sourceWorkspaceArtifactId, _integrationPointArtifactId).Returns(true);

			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, 12345), Constants.IntegrationPoints.JOBS_ALREADY_RUNNING);

			// assert
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity));
			_queueManager.Received(1).HasJobsExecutingOrInQueue(_sourceWorkspaceArtifactId, _integrationPointArtifactId);
			_jobHistoryService.DidNotReceive().CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
			_managerFactory.Received().CreateQueueManager(_contextContainer);
		}

		[Test]
		public void RunIntegrationPoint_GoldFlow_OtherProviders()
		{
			// arrange
			_sourceProvider.Identifier = "some thing else";
			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Other))
				.Returns(new PermissionCheckDTO() {Success = true});

			// act
			_instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId);

			// assert
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Other));
			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRunNow, null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.SyncManager, _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RunIntegrationPoint_GoldFlow_OtherProviders_InvalidPermissions()
		{
			// arrange
			_sourceProvider.Identifier = "some thing else";
			string[] errorMessages = {"Uh", "oh"};
			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Other))
				.Returns(new PermissionCheckDTO() { Success = false, ErrorMessages = errorMessages});

			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, 12345), String.Join("<br/>", errorMessages));

			// assert
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Other));

			var expectedErrorMessage = new ErrorDTO()
			{
				Message = Core.Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE,
				FullText = $"User is missing the following permissions:{System.Environment.NewLine}{String.Join(System.Environment.NewLine, errorMessages)}"
			};

			_errorManager.Received(1).Create(Arg.Is(_sourceWorkspaceArtifactId), Arg.Is<IEnumerable<ErrorDTO>>(x => MatchHelper.Matches(new [] {expectedErrorMessage}, x)));
			_jobHistoryService.DidNotReceive().CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRunNow, null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.SyncManager, _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}
		

		[Test]
		public void RetryIntegrationPoint_IntegrationPointHasNoErrors_ThrowsException_Test()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_integrationPoint.HasErrors = null;
			_integrationPointDto.HasErrors = null;
			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity))
				.Returns(new PermissionCheckDTO() { Success = true });

			// Act
			Assert.Throws<Exception>(() =>
				_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId),
				Constants.IntegrationPoints.RETRY_NO_EXISTING_ERRORS);

			// Assert
			_caseServiceManager.RsapiService.SourceProviderLibrary.Received(1).Read(_integrationPoint.SourceProvider.Value);
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity));

			_jobHistoryService.DidNotReceive().CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRetryErrors, null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RetryIntegrationPoint_InvalidPermissions_ThrowsException_Test()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			string[] errorMessages = { "Uh", "oh" };
			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity))
				.Returns(new PermissionCheckDTO() { Success = false, ErrorMessages = errorMessages });

			// Act
			Assert.Throws<Exception>(() =>
				_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId),
				String.Join("<br/>", errorMessages));

			// Assert
			_caseServiceManager.RsapiService.SourceProviderLibrary.Received(1).Read(_integrationPoint.SourceProvider.Value);
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity));

			var expectedErrorMessage = new ErrorDTO()
			{
				Message = Core.Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE,
				FullText = $"User is missing the following permissions:{System.Environment.NewLine}{String.Join(System.Environment.NewLine, errorMessages)}"
			};
			_errorManager.Received(1).Create(Arg.Is(_sourceWorkspaceArtifactId), Arg.Is<IEnumerable<ErrorDTO>>(x => MatchHelper.Matches(new[] { expectedErrorMessage }, x)));
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

			_integrationPointManager.DidNotReceiveWithAnyArgs().UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity));
			_jobHistoryService.DidNotReceive().CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRetryErrors, null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void RetryIntegrationPoint_GoldFlow_Test()
		{
			// Arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_integrationPointManager.UserHasPermissionToRunJob(
				Arg.Is(_sourceWorkspaceArtifactId),
				Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
				Arg.Is(Constants.SourceProvider.Relativity))
				.Returns(new PermissionCheckDTO() { Success = true });
			_integrationPoint.HasErrors = true;
			_integrationPointDto.HasErrors = true;

			// Act
			_instance.RetryIntegrationPoint(_sourceWorkspaceArtifactId, _integrationPointArtifactId, _userId);

			// Assert
			_caseServiceManager.RsapiService.SourceProviderLibrary.Received(1).Read(_integrationPoint.SourceProvider.Value);
			_integrationPointManager.Received(1).UserHasPermissionToRunJob(
							Arg.Is(_sourceWorkspaceArtifactId),
							Arg.Is<IntegrationPointDTO>(x => MatchHelper.Matches(_integrationPointDto, x)),
							Arg.Is(Constants.SourceProvider.Relativity));
			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRetryErrors, null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
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
			_targetPermissionRepository.DidNotReceive().UserCanImport();
			_sourcePermissionRepository.DidNotReceive().UserCanEditDocuments();
			_jobHistoryService.DidNotReceive().CreateRdo(_integrationPoint, Arg.Any<Guid>(), JobTypeChoices.JobHistoryRunNow, null);
			_jobManager.DidNotReceive().CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), _sourceWorkspaceArtifactId, _integrationPoint.ArtifactId, _userId);
		}

		[Test]
		public void Update_SourceProviderReadFails_Excepts()
		{
			// Arrange
			int targetWorkspaceArtifactId = 9302;
			var model = new IntegrationModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830,
				SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactId }),
				LastRun = DateTime.Now
			};

			var existingModel = new IntegrationModel()
			{
				ArtifactID = model.ArtifactID,
				SourceProvider = model.SourceProvider,
				SourceConfiguration = model.SourceConfiguration,
				LastRun = model.LastRun
			};

			_instance.When(instance => instance.ReadIntegrationPoint(Arg.Any<int>())).DoNotCallBase();
			_instance.ReadIntegrationPoint(Arg.Is(model.ArtifactID)).Returns(existingModel);

			const string exceptionMessage = "UH OH!";
			_caseServiceManager.RsapiService.SourceProviderLibrary
					.Read(Arg.Is(model.SourceProvider))
					.Throws(new Exception(exceptionMessage));

			// Act
			Assert.Throws<Exception>(() => _instance.SaveIntegration(model), "Unable to save Integration Point: Unable to retrieve source provider");

			// Assert
			_instance.Received(1).ReadIntegrationPoint(Arg.Is(model.ArtifactID));
			_caseServiceManager.RsapiService.SourceProviderLibrary
				.Received(1)
				.Read(Arg.Is(model.SourceProvider));
		}

		[Test]
		public void Update_IPReadFails_Excepts()
		{
			int targetWorkspaceArtifactId = 9302;
			var model = new IntegrationModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830,
				SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactId })
			};

			const string exceptionMessage = "UH OH!";
			_instance.ReadIntegrationPoint(Arg.Is(model.ArtifactID))
				.Throws(new Exception(exceptionMessage));

			// Act
			Assert.Throws<Exception>(() => _instance.SaveIntegration(model), "Unable to save Integration Point: Unable to retrieve Integration Point");

			_instance.Received(1).ReadIntegrationPoint(Arg.Is(model.ArtifactID));
		}

		[Test]
		[TestCase(false, new string[] { "Name" })]
		[TestCase(false, new string[] { "Destination Provider" })]
		[TestCase(false, new string[] { "Destination RDO" })]
		[TestCase(false, new string[] { "Case" })]
		[TestCase(false, new string[] { "Source Provider" })]
		[TestCase(false, new string[] { "Name", "Destination Provider", "Destination RDO", "Case" })]
		[TestCase(false, new string[] { "Name", "Destination Provider", "Destination RDO", "Case", "Source Provider" })]
		[TestCase(false, new string[] { "Name", "Source Configuration" })] // normal providers will only throw with "Name" in list
		[TestCase(true, new string[] { "Name", "Destination Provider", "Destination RDO", "Case", "Source Configuration" })]
		[TestCase(true, new string[] { "Source Configuration" })]
		[TestCase(true, new string[] { "Name", "Destination Provider", "Destination RDO", "Case", "Source Configuration" })] // If relativity provider and no permissions, throw permissions error first
		public void Update_InvalidProperties_Excepts(bool isRelativityProvider, string[] propertyNames)
		{
			// Arrange
			var propertyNameHashSet = new HashSet<string>(propertyNames);
			const int targetWorkspaceArtifactId = 12329;
			const int sourceWorkspaceArtifactId = 92321;
			int existingTargetWorkspaceArtifactId = propertyNameHashSet.Contains("Source Configuration")
				? 12324
				: targetWorkspaceArtifactId;
			var model = new IntegrationModel()
			{
				ArtifactID = 123,
				Name = "My Name",
				DestinationProvider = 4909,
				SourceProvider = 9830,
				Destination = JsonConvert.SerializeObject(new { artifactTypeID = 10, CaseArtifactId = 7891232 }),
				SourceConfiguration = JsonConvert.SerializeObject(new
				{
					TargetWorkspaceArtifactId = targetWorkspaceArtifactId,
					SourceWorkspaceArtifactId = sourceWorkspaceArtifactId
				})
			};

			var existingModel = new IntegrationModel()
			{
				ArtifactID = model.ArtifactID,
				LastRun = DateTime.Now,
				Name = propertyNameHashSet.Contains("Name") ? "Diff Name" : model.Name,
				DestinationProvider = propertyNameHashSet.Contains("Destination Provider") ? 12343 : model.DestinationProvider,
				SourceProvider = propertyNameHashSet.Contains("Source Provider") ? 391232 : model.SourceProvider,
				Destination = JsonConvert.SerializeObject(new
				{
					artifactTypeID = propertyNameHashSet.Contains("Destination RDO") ? 13 : 10,
					CaseArtifactId = propertyNameHashSet.Contains("Case") ? 18392 : 7891232
				}),
				SourceConfiguration = JsonConvert.SerializeObject(new
				{
					TargetWorkspaceArtifactId = existingTargetWorkspaceArtifactId
				})
			};

			_instance.ReadIntegrationPoint(Arg.Is(model.ArtifactID))
				.Returns(existingModel);

			// Source Provider is special, if this changes we except earlier
			if (!propertyNameHashSet.Contains("Source Provider"))
			{
				var sourceProvider = new SourceProvider()
				{
					Identifier = isRelativityProvider
						? Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID
						: "YODUDE"
				};
				_caseServiceManager.RsapiService.SourceProviderLibrary
					.Read(Arg.Is(model.SourceProvider))
					.Returns(sourceProvider);
			}

			string filteredNames = String.Join(",", propertyNames.Where(x => isRelativityProvider || x != "Source Configuration").Select(x => $" {x}"));
			string expectedErrorString =
				$"Unable to save Integration Point:{filteredNames} cannot be changed once the Integration Point has been run";

			// Act
			Assert.Throws<Exception>(() => _instance.SaveIntegration(model), expectedErrorString);

			// Assert
			_instance.Received(1).ReadIntegrationPoint(Arg.Is(model.ArtifactID));
			_caseServiceManager.RsapiService.SourceProviderLibrary
				.Received(!propertyNameHashSet.Contains("Source Provider") ? 1 : 0)
				.Read(Arg.Is(model.SourceProvider));
		}
	}
}