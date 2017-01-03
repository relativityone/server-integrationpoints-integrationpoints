using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.JobHistoryManager
{
	public class JobHistoryPermissionTests : SourceProviderTemplate
	{
		private UserModel _userModel;
		private int _groupId;

		public JobHistoryPermissionTests() : base($"jobhistory_{Utils.FormatedDateTimeNow}")
		{
		}

		public override void TestSetup()
		{
			base.TestSetup();
			_groupId = Group.CreateGroup($"group_{Utils.FormatedDateTimeNow}");
			_userModel = User.CreateUser("firstname", "lastname", $"test_{Utils.FormatedDateTimeNow}@kcura.com", new List<int> {_groupId});
		}

		public override void TestTeardown()
		{
			base.TestTeardown();
			Group.DeleteGroup(_groupId);
			User.DeleteUser(_userModel.ArtifactId);
		}

		[Test]
		public void MissingWorkspacePermission()
		{
			var jobHistoryRequest = new JobHistoryRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			};
			var client = Helper.CreateUserProxy<IJobHistoryManager>(_userModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetJobHistoryAsync(jobHistoryRequest).Result);
		}

		[Test]
		public void MissingJobHistoryViewPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, _groupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, _groupId);
			var permissionsForJobHistory = permissions.ObjectPermissions.FindPermission(ObjectTypes.JobHistory);
			permissionsForJobHistory.ViewSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var jobHistoryRequest = new JobHistoryRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			};
			var client = Helper.CreateUserProxy<IJobHistoryManager>(_userModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetJobHistoryAsync(jobHistoryRequest).Result);
		}
	}
}