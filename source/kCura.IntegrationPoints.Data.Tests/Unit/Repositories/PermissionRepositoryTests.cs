using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Permission;

namespace kCura.IntegrationPoints.Data.Tests.Unit.Repositories
{
	[TestFixture]
	public class PermissionRepositoryTests
	{
		private IPermissionRepository _instance;
		private IHelper _helper;
		private IServicesMgr _servicesMgr;
		private IPermissionManager _permissionManager;

		private const int _WORKSPACE_ID = 392834;

		[SetUp]
		public void SetUp()
		{
			_helper = NSubstitute.Substitute.For<IHelper>();
			_servicesMgr = NSubstitute.Substitute.For<IServicesMgr>();
			_permissionManager = NSubstitute.Substitute.For<IPermissionManager>();

			_helper.GetServicesManager().Returns(_servicesMgr);
			_servicesMgr.CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser)).Returns(_permissionManager);

			_instance = new PermissionRepository(_helper, _WORKSPACE_ID);
		}

		[Test]
		public void UserHasPermissionToAccessWorkspace_UserHasPermissions_ReturnsTrue()
		{
			// Arrange
			var expectedPermissionRef = new PermissionRef()
			{
				ArtifactType = new ArtifactTypeIdentifier((int) ArtifactType.Case),
				PermissionType = PermissionType.View
			};

			var permissionValues = new List<PermissionValue>()
			{
				new PermissionValue()
				{
					Selected = true
				}
			};

			_permissionManager.GetPermissionSelectedAsync(
				Arg.Is(-1),
				Arg.Is<List<PermissionRef>>(
					x => x.Count() == 1
					     && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
					     && x.First().PermissionType == expectedPermissionRef.PermissionType),
				Arg.Is(_WORKSPACE_ID))
				.Returns(permissionValues);

			// Act
			bool result = _instance.UserHasPermissionToAccessWorkspace();

			// Assert
			Assert.IsTrue(result);
			_permissionManager.Received(1).GetPermissionSelectedAsync(
				Arg.Is(-1),
				Arg.Is<List<PermissionRef>>(
					x => x.Count() == 1
					     && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
					     && x.First().PermissionType == expectedPermissionRef.PermissionType),
				Arg.Is(_WORKSPACE_ID));
		}

		[Test]
		public void UserHasPermissionToAccessWorkspace_InvalidPermissions_ReturnsFalse()
		{
			// Arrange
			var expectedPermissionRef = new PermissionRef()
			{
				ArtifactType = new ArtifactTypeIdentifier((int)ArtifactType.Case),
				PermissionType = PermissionType.View
			};

			var permissionValues = new List<PermissionValue>()
			{
				new PermissionValue()
				{
					Selected = false
				}
			};

			_permissionManager.GetPermissionSelectedAsync(
				Arg.Is(-1),
				Arg.Is<List<PermissionRef>>(
					x => x.Count() == 1
						 && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
						 && x.First().PermissionType == expectedPermissionRef.PermissionType),
				Arg.Is(_WORKSPACE_ID))
				.Returns(permissionValues);

			// Act
			bool result = _instance.UserHasPermissionToAccessWorkspace();

			// Assert
			Assert.IsFalse(result);
			_permissionManager.Received(1).GetPermissionSelectedAsync(
				Arg.Is(-1),
				Arg.Is<List<PermissionRef>>(
					x => x.Count() == 1
						 && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
						 && x.First().PermissionType == expectedPermissionRef.PermissionType),
				Arg.Is(_WORKSPACE_ID));
		}

		[Test]
		public void UserHasPermissionToAccessWorkspace_NoPermissionsReturned_ReturnsFalse()
		{
			// Arrange
			var expectedPermissionRef = new PermissionRef()
			{
				ArtifactType = new ArtifactTypeIdentifier((int)ArtifactType.Case),
				PermissionType = PermissionType.View
			};

			var permissionValues = new List<PermissionValue>()
			{
			};

			_permissionManager.GetPermissionSelectedAsync(
				Arg.Is(-1),
				Arg.Is<List<PermissionRef>>(
					x => x.Count() == 1
						 && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
						 && x.First().PermissionType == expectedPermissionRef.PermissionType),
				Arg.Is(_WORKSPACE_ID))
				.Returns(permissionValues);

			// Act
			bool result = _instance.UserHasPermissionToAccessWorkspace();

			// Assert
			Assert.IsFalse(result);
			_permissionManager.Received(1).GetPermissionSelectedAsync(
				Arg.Is(-1),
				Arg.Is<List<PermissionRef>>(
					x => x.Count() == 1
						 && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
						 && x.First().PermissionType == expectedPermissionRef.PermissionType),
				Arg.Is(_WORKSPACE_ID));
		}
	}
}