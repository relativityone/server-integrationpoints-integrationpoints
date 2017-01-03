using System;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Interfaces.Private.Exceptions;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using NSubstitute;
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

			_jobHistoryManager = new JobHistoryManagerContainerMocked(_logger, permissionRepositoryFactory, _container);
		}

		[Test]
		public void ItShouldGrantAccess()
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.View).Returns(true);

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

			_logger.Received(1).LogError($"User doesn't have permission to access endpoint GetJobHistoryAsync. Missing permissions {missingPermissions}.");
		}

		[Test]
		public void ItShouldGetJobHistory()
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.View).Returns(true);

			var jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
			_container.Resolve<IJobHistoryRepository>().Returns(jobHistoryRepository);

			var jobHistoryRequest = new JobHistoryRequest
			{
				WorkspaceArtifactId = _WORKSPACE_ID
			};
			_jobHistoryManager.GetJobHistoryAsync(jobHistoryRequest).Wait();

			jobHistoryRepository.Received(1).GetJobHistory(jobHistoryRequest);
		}
	}

	internal class JobHistoryManagerContainerMocked : JobHistoryManager
	{
		private readonly IWindsorContainer _windsorContainer;

		public JobHistoryManagerContainerMocked(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer windsorContainer)
			: base(logger, permissionRepositoryFactory)
		{
			_windsorContainer = windsorContainer;
		}

		public JobHistoryManagerContainerMocked(ILog logger, IWindsorContainer windsorContainer) : base(logger)
		{
			_windsorContainer = windsorContainer;
		}

		protected override IWindsorContainer GetDependenciesContainer(int workspaceArtifactId)
		{
			return _windsorContainer;
		}
	}
}