using System;
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

namespace kCura.IntegrationPoints.Services.Tests
{
	[TestFixture]
	public class KeplerServiceTests : TestBase
	{
		private KeplerService _keplerServiceBase;
		private ILog _logger;
		private IPermissionRepository _permissionRepository;

		[SetUp]
		public override void SetUp()
		{
			_logger = Substitute.For<ILog>();
			_permissionRepository = Substitute.For<IPermissionRepository>();
			var permissionRepositoryFactory = Substitute.For<IPermissionRepositoryFactory>();
			permissionRepositoryFactory.Create(Arg.Any<IHelper>(), Arg.Any<int>()).Returns(_permissionRepository);
			_keplerServiceBase = new KeplerService(_logger, permissionRepositoryFactory);
		}

		[Test]
		public void ItShouldDenyAccessForUserWithoutWorkspacePermission()
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(false);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View).Returns(true);

			Assert.Throws<InsufficientPermissionException>(() => { _keplerServiceBase.CheckPermissions(367); });
		}

		[Test]
		public void ItShouldDenyAccessForUserWithoutIntegrationPointViewPermission()
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View).Returns(false);

			Assert.Throws<InsufficientPermissionException>(() => { _keplerServiceBase.CheckPermissions(367); });
		}

		[Test]
		public void ItShouldAllowAccessForUserWithSufficientPermission()
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View).Returns(true);

			_keplerServiceBase.CheckPermissions(367);
		}

		[Test]
		public void ItShouldLogAnyExceptionOccurredDuringPermissionCheck()
		{
			var expectedException = new Exception();
			_permissionRepository.UserHasPermissionToAccessWorkspace().Throws(expectedException);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View).Returns(false);

			Assert.Throws<InsufficientPermissionException>(() => { _keplerServiceBase.CheckPermissions(367); });

			_logger.Received().LogError(expectedException, Arg.Any<string>());
		}
	}

	internal class KeplerService : KeplerServiceBase
	{
		public KeplerService(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory) : base(logger, permissionRepositoryFactory)
		{
		}

		public KeplerService(ILog logger) : base(logger)
		{
		}

		public new void CheckPermissions(int workspaceId)
		{
			base.CheckPermissions(workspaceId);
		}
	}
}