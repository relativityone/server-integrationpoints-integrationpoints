using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Interfaces.Private.Exceptions;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.Services.Permission;
using Permission = kCura.IntegrationPoint.Tests.Core.Permission;

namespace kCura.IntegrationPoints.Services.Tests.Integration.JobHistoryManager
{
	public class JobHistoryPermissionTests : RelativityProviderTemplate
	{
		private int _groupId;
		private UserModel _userModel;

		public JobHistoryPermissionTests() : base($"JH_source_{Utils.FormatedDateTimeNow}", $"JH_dest_{Utils.FormatedDateTimeNow}")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			RunDefaultIntegrationPoint();
			ModifyJobHistoryItem();
		}

		public override void TestSetup()
		{
			base.TestSetup();
			_groupId = Group.CreateGroup($"group_{Utils.FormatedDateTimeNow}");
			_userModel = User.CreateUser("firstname", "lastname", $"test_{Utils.FormatedDateTimeNow}@relativity.com", new List<int> {_groupId});
		}

		public override void TestTeardown()
		{
			base.TestTeardown();
			Group.DeleteGroup(_groupId);
			User.DeleteUser(_userModel.ArtifactId);
		}

		[Test]
		public void MissingSourceWorkspacePermission()
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

		[Test]
		public void MissingTargetWorkspacePermission()
		{
			Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, _groupId);

			var jobHistoryRequest = new JobHistoryRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactId,
				Page = 0,
				PageSize = 10
			};

			var jobHistoryClient = Helper.CreateUserProxy<IJobHistoryManager>(_userModel.EmailAddress);
			JobHistorySummaryModel jobHistory = jobHistoryClient.GetJobHistoryAsync(jobHistoryRequest).Result;

			Assert.That(jobHistory.Data.Length, Is.EqualTo(0));
		}

		[Test]
		public void MissingIntegrationPointPermissionsInSourceWorkspace()
		{
			//Arrange
			Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, _groupId);
			Group.AddGroupToWorkspace(TargetWorkspaceArtifactId, _groupId);

			RemoveIntegrationPointPermissionsFromSourceWorkspace();
			
			var jobHistoryRequest = new JobHistoryRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactId,
				Page = 0,
				PageSize = 10
			};

			var jobHistoryClient = Helper.CreateUserProxy<IJobHistoryManager>(_userModel.EmailAddress);

			//Act & Assert
			Assert.That(() => jobHistoryClient.GetJobHistoryAsync(jobHistoryRequest).Result, Throws.TypeOf<InternalServerErrorException>().With.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));
		}

		private void RemoveIntegrationPointPermissionsFromSourceWorkspace()
		{
			GroupPermissions permissions = Permission.GetGroupPermissions(SourceWorkspaceArtifactId, _groupId);

			RemoveViewPermission(permissions, ObjectTypes.IntegrationPoint);
			RemoveViewPermission(permissions, ObjectTypes.IntegrationPointProfile);
			RemoveViewPermission(permissions, ObjectTypes.IntegrationPointType);
			RemoveViewPermission(permissions, ObjectTypes.SourceProvider);
			RemoveViewPermission(permissions, ObjectTypes.DestinationProvider);
			RemoveViewPermission(permissions, ObjectTypes.JobHistoryError);

			Permission.SavePermission(SourceWorkspaceArtifactId, permissions);
		}

		private void RemoveViewPermission(GroupPermissions permissions, string objectType)
		{
			var permissionsForIntegrationPoint = permissions.ObjectPermissions.FindPermission(objectType);
			permissionsForIntegrationPoint.ViewSelected = false;
		}

		private void RunDefaultIntegrationPoint()
		{
			Core.Models.IntegrationPointModel ipModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, $"ip_{Utils.FormatedDateTimeNow}", "Append Only");
			ipModel.Destination = CreateDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum.AppendOnly, TargetWorkspaceArtifactId);
			Core.Models.IntegrationPointModel ip = CreateOrUpdateIntegrationPoint(ipModel);

			var client = Helper.CreateAdminProxy<IIntegrationPointManager>();
			client.RunIntegrationPointAsync(SourceWorkspaceArtifactId, ip.ArtifactID).Wait();

			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, ip.ArtifactID);
		}

		private void ModifyJobHistoryItem()
		{
			//This is needed, as Integration Point, which has been run, doesn't contain any documents
			var dbContext = Helper.GetDBContext(SourceWorkspaceArtifactId);
			dbContext.ExecuteNonQuerySQLStatement(@"UPDATE [JobHistory] SET [ItemsTransferred] = 1, [TotalItems] = 1");
		}
	}
}