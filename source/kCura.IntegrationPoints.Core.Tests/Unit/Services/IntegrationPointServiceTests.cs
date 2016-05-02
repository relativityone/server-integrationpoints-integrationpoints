using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Services
{
	[TestFixture]
	public class IntegrationPointServiceTests
	{
		private ICaseServiceContext _caseServiceManager;
		private IPermissionService _permissionService;
		private IJobManager _jobManager;
		private ISerializer _serializer;
		private IJobHistoryService _jobHistoryService;
		private IntegrationPointService _instance;
		private Data.IntegrationPoint _integrationPoint;
		private SourceProvider _sourceProvider;

		private readonly int _workspaceArtifactId = 789;
		private readonly int _integrationPointArtifactId = 741;
		private readonly int _userId = 951;

		[SetUp]
		public void Setup()
		{
			_caseServiceManager = NSubstitute.Substitute.For<ICaseServiceContext>();
			_permissionService = NSubstitute.Substitute.For<IPermissionService>();
			_serializer = NSubstitute.Substitute.For<ISerializer>();
			_jobManager = NSubstitute.Substitute.For<IJobManager>();
			_jobHistoryService = NSubstitute.Substitute.For<IJobHistoryService>();

			_instance = new IntegrationPointService(_caseServiceManager, _permissionService, _serializer, null, _jobManager,
				_jobHistoryService);

			_caseServiceManager.RsapiService = NSubstitute.Substitute.For<IRSAPIService>();
			_caseServiceManager.RsapiService.IntegrationPointLibrary = NSubstitute.Substitute.For<IGenericLibrary<Data.IntegrationPoint>>();
			_caseServiceManager.RsapiService.SourceProviderLibrary = NSubstitute.Substitute.For<IGenericLibrary<Data.SourceProvider>>();

			_integrationPoint = new Data.IntegrationPoint();
			_sourceProvider = new Data.SourceProvider();
			_integrationPoint.SourceProvider = 321;
			_integrationPoint.SourceConfiguration = "{ TargetWorkspaceArtifactId : 123 }";

			_caseServiceManager.RsapiService.IntegrationPointLibrary.Read(Arg.Any<int>()).Returns(_integrationPoint);
			_caseServiceManager.RsapiService.SourceProviderLibrary.Read(Arg.Any<int>()).Returns(_sourceProvider);

		}

		[Test]
		public void RunIntegrationPoint_GoldFlow_RelativityProvider()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_permissionService.UserCanImport(123).Returns(true);
			_permissionService.UserCanEditDocuments(Arg.Any<int>()).Returns(true);

			// act
			_instance.RunIntegrationPoint(_workspaceArtifactId, _integrationPointArtifactId, _userId);


			// assert
			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.ExportService, _workspaceArtifactId, _integrationPointArtifactId, _userId);
		}

		[Test]
		public void RunIntegrationPoint_RelativityProvider_UserHasNoPermissionToExportToTheTargetWorkspace()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_permissionService.UserCanImport(123).Returns(false);

			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_workspaceArtifactId, _integrationPointArtifactId, _userId), Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT);


			// assert
			_jobHistoryService.Received(0).CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), null);
			_jobManager.Received(0).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
		}

		[Test]
		public void RunIntegrationPoint_RelativityProvider_UserIdZero()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_permissionService.UserCanImport(123).Returns(true);
			_permissionService.UserCanEditDocuments(Arg.Any<int>()).Returns(true);
			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_workspaceArtifactId, _integrationPointArtifactId, 0), Constants.IntegrationPoints.NO_USERID);


			// assert
			_jobHistoryService.Received(0).CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), null);
			_jobManager.Received(0).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
		}

		[Test]
		public void RunIntegrationPoint_RelativityProvider_UserHasNoPermissionToMassEditDocs()
		{
			// arrange
			_sourceProvider.Identifier = Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID;
			_permissionService.UserCanImport(123).Returns(true);
			_permissionService.UserCanEditDocuments(Arg.Any<int>()).Returns(false);
			// act
			Assert.Throws<Exception>(() => _instance.RunIntegrationPoint(_workspaceArtifactId, _integrationPointArtifactId, 0), Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS);


			// assert
			_jobHistoryService.Received(0).CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), null);
			_jobManager.Received(0).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), Arg.Any<TaskType>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
		}

		[Test]
		public void RunIntegrationPoint_GoldFlow_OtherProviders()
		{
			// arrange
			_sourceProvider.Identifier = "some thing else";
			_permissionService.UserCanImport(Arg.Any<int>()).Returns(true);
			_permissionService.UserCanEditDocuments(Arg.Any<int>()).Returns(true);

			// act
			_instance.RunIntegrationPoint(_workspaceArtifactId, _integrationPointArtifactId, _userId);


			// assert
			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.SyncManager, _workspaceArtifactId, _integrationPointArtifactId, _userId);
		}

		[Test]
		public void RunIntegrationPoint_GoldFlow_OtherProviders_NoImportCheck()
		{
			// arrange
			_sourceProvider.Identifier = "some thing else";
			_permissionService.UserCanImport(Arg.Any<int>()).Returns(false);

			// act
			_instance.RunIntegrationPoint(_workspaceArtifactId, _integrationPointArtifactId, _userId);


			// assert
			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.SyncManager, _workspaceArtifactId, _integrationPointArtifactId, _userId);
		}

		[Test]
		public void RunIntegrationPoint_GoldFlow_OtherProviders_NoMassEditCheck()
		{
			// arrange
			_sourceProvider.Identifier = "some thing else";
			_permissionService.UserCanImport(Arg.Any<int>()).Returns(true);
			_permissionService.UserCanEditDocuments(Arg.Any<int>()).Returns(false);

			// act
			_instance.RunIntegrationPoint(_workspaceArtifactId, _integrationPointArtifactId, _userId);


			// assert
			_jobHistoryService.Received(1).CreateRdo(_integrationPoint, Arg.Any<Guid>(), null);
			_jobManager.Received(1).CreateJobOnBehalfOfAUser(Arg.Any<TaskParameters>(), TaskType.SyncManager, _workspaceArtifactId, _integrationPointArtifactId, _userId);
		}
	}
}