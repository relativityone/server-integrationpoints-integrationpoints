using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using NUnit.Framework;
using Constants = kCura.IntegrationPoints.Data.Constants;

namespace kCura.IntegrationPoints.Services.Tests.Integration.Permissions
{
	public abstract class KeplerServiceMissingPermissionTests : SourceProviderTemplate
	{
		protected UserModel UserModel;
		protected int GroupId;

		protected KeplerServiceMissingPermissionTests() : base($"Kepler_Service_{Utils.FormatedDateTimeNow}")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			GroupId = Group.CreateGroup($"group_{Utils.FormatedDateTimeNow}");
			UserModel = User.CreateUser("firstname", "lastname", $"test_{Utils.FormatedDateTimeNow}@kcura.com", new List<int> { GroupId });
			SetPermissions();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			Group.DeleteGroup(GroupId);
			User.DeleteUser(UserModel.ArtifactId);
		}

		protected abstract void SetPermissions();
		
		[Test]
		public void IIntegrationPointManager_CreateIntegrationPointAsync_AccessDenied()
		{
			CreateIntegrationPointRequest request = new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			};
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointAsync(request).Result);
		}

		[Test]
		public void IIntegrationPointManager_GetAllIntegrationPointsAsync_AccessDenied()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetAllIntegrationPointsAsync(WorkspaceArtifactId).Result);
		}

		[Test]
		public void IIntegrationPointManager_GetIntegrationPointArtifactTypeIdAsync_AccessDenied()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetIntegrationPointArtifactTypeIdAsync(WorkspaceArtifactId).Result);
		}

		[Test]
		public void IIntegrationPointManager_GetIntegrationPointAsync_AccessDenied()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetIntegrationPointAsync(WorkspaceArtifactId, 1).Result);
		}

		[Test]
		public void IIntegrationPointManager_GetSourceProviderArtifactIdAsync_AccessDenied()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
#pragma warning disable 618
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetSourceProviderArtifactIdAsync(WorkspaceArtifactId, Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString()).Result);
#pragma warning restore 618
		}

		[Test]
		public void IIntegrationPointManager_RunIntegrationPointAsync_AccessDenied()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() =>
			{
				client.RunIntegrationPointAsync(WorkspaceArtifactId, 1).Wait();
				return null;
			});
		}

		[Test]
		public void IIntegrationPointManager_UpdateIntegrationPointAsync_AccessDenied()
		{
			UpdateIntegrationPointRequest request = new UpdateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			};
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.UpdateIntegrationPointAsync(request).Result);
		}
	}
}