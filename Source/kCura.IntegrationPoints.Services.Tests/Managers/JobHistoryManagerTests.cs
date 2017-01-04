using System;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Interfaces.Private.Exceptions;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Logging;
using IJobHistoryRepository = kCura.IntegrationPoints.Services.Repositories.IJobHistoryRepository;

namespace kCura.IntegrationPoints.Services.Tests.Managers
{
	public class JobHistoryManagerTests : TestBase
	{
		private const int _WORKSPACE_ID = 784838;
		private JobHistoryManager _jobHistoryManager;
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

			_jobHistoryManager = new JobHistoryManagerWithContainerMock(_logger, permissionRepositoryFactory, _container);
		}

		[Test]
		public void ItShouldGrantAccess()
		{
			MockValidPermissions();

			_jobHistoryManager.GetJobHistoryAsync(new JobHistoryRequest
			{
				WorkspaceArtifactId = _WORKSPACE_ID
			}).Wait();

			_permissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
			_permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.View);
		}

		[Test]
		[TestCase(false, false)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		public void ItShouldDenyAccess(bool workspaceAccess, bool jobHistoryAccess)
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.View).Returns(jobHistoryAccess);

			var jobHistoryRequest = new JobHistoryRequest
			{
				WorkspaceArtifactId = _WORKSPACE_ID
			};

			Assert.That(() => _jobHistoryManager.GetJobHistoryAsync(jobHistoryRequest).Wait(),
				Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
					.And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

			_permissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
			_permissionRepository.Received(1).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.View);
		}

		[Test]
		[TestCase(false, false, "Workspace, JobHistory - View")]
		[TestCase(true, false, "JobHistory - View")]
		[TestCase(false, true, "Workspace")]
		public void ItShouldLogDenyingAccess(bool workspaceAccess, bool jobHistoryAccess, string missingPermissions)
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.View).Returns(jobHistoryAccess);

			var jobHistoryRequest = new JobHistoryRequest
			{
				WorkspaceArtifactId = _WORKSPACE_ID
			};

			try
			{
				_jobHistoryManager.GetJobHistoryAsync(jobHistoryRequest).Wait();
			}
			catch (Exception)
			{
				//Ignore as this test checks logging only
			}

			_logger.Received(1)
				.LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetJobHistoryAsync", missingPermissions);
		}

		[Test]
		public void ItShouldGetJobHistory()
		{
			MockValidPermissions();

			var jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
			_container.Resolve<IJobHistoryRepository>().Returns(jobHistoryRepository);

			var expectedResult = new JobHistorySummaryModel();

			var jobHistoryRequest = new JobHistoryRequest
			{
				WorkspaceArtifactId = _WORKSPACE_ID
			};
			jobHistoryRepository.GetJobHistory(jobHistoryRequest).Returns(expectedResult);

			var actualResult = _jobHistoryManager.GetJobHistoryAsync(jobHistoryRequest).Result;

			jobHistoryRepository.Received(1).GetJobHistory(jobHistoryRequest);

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ItShouldHideException()
		{
			MockValidPermissions();

			var jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
			jobHistoryRepository.GetJobHistory(Arg.Any<JobHistoryRequest>()).Throws(new ArgumentException());
			_container.Resolve<IJobHistoryRepository>().Returns(jobHistoryRepository);

			JobHistoryRequest jobHistoryRequest = new JobHistoryRequest
			{
				WorkspaceArtifactId = _WORKSPACE_ID
			};
			Assert.That(() => _jobHistoryManager.GetJobHistoryAsync(jobHistoryRequest).Wait(),
				Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
					.And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));
		}

		[Test]
		public void ItShouldLogException()
		{
			MockValidPermissions();

			var expectedException = new ArgumentException();

			var jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
			jobHistoryRepository.GetJobHistory(Arg.Any<JobHistoryRequest>()).Throws(expectedException);
			_container.Resolve<IJobHistoryRepository>().Returns(jobHistoryRepository);

			JobHistoryRequest jobHistoryRequest = new JobHistoryRequest
			{
				WorkspaceArtifactId = _WORKSPACE_ID
			};

			try
			{
				_jobHistoryManager.GetJobHistoryAsync(jobHistoryRequest).Wait();
			}
			catch (Exception)
			{
				//Ignore as this test checks logging only
			}

			_logger.Received(1).LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetJobHistoryAsync");
		}

		private void MockValidPermissions()
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.View).Returns(true);
		}
	}

	internal class JobHistoryManagerWithContainerMock : JobHistoryManager
	{
		private readonly IWindsorContainer _windsorContainer;

		public JobHistoryManagerWithContainerMock(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer windsorContainer)
			: base(logger, permissionRepositoryFactory)
		{
			_windsorContainer = windsorContainer;
		}

		public JobHistoryManagerWithContainerMock(ILog logger, IWindsorContainer windsorContainer) : base(logger)
		{
			_windsorContainer = windsorContainer;
		}

		protected override IWindsorContainer GetDependenciesContainer(int workspaceArtifactId)
		{
			return _windsorContainer;
		}
	}
}