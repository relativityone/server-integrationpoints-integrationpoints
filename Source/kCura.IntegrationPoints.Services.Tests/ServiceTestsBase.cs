using System;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services.Tests
{
	public abstract class ServiceTestsBase
	{
		protected ILog Logger;
		protected IPermissionRepositoryFactory PermissionRepositoryFactory;

		[SetUp]
		public void SetUp()
		{
			Logger = Substitute.For<ILog>();
			PermissionRepositoryFactory = Substitute.For<IPermissionRepositoryFactory>();
			IPermissionRepository permissionRepository = Substitute.For<IPermissionRepository>();
			permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
			permissionRepository.UserHasArtifactTypePermission(Arg.Any<Guid>(), Arg.Any<ArtifactPermission>()).Returns(true);
			PermissionRepositoryFactory.Create(Arg.Any<IHelper>(), Arg.Any<int>()).Returns(permissionRepository);
		}
	}
}