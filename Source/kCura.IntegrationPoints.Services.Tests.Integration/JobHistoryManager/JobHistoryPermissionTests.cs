﻿using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Interfaces.Private.Exceptions;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.Services.Permission;
using Relativity.Testing.Identification;
using Permission = kCura.IntegrationPoint.Tests.Core.Permission;

namespace kCura.IntegrationPoints.Services.Tests.Integration.JobHistoryManager
{
	public class JobHistoryPermissionTests : RelativityProviderTemplate
	{
		private int _groupId;
		private UserModel _userModel;

		public JobHistoryPermissionTests() : base($"JH_source_{Utils.FormattedDateTimeNow}", $"JH_dest_{Utils.FormattedDateTimeNow}")
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
			_groupId = Group.CreateGroup($"group_{Utils.FormattedDateTimeNow}");
			_userModel = User.CreateUser("firstname", "lastname", $"test_{Utils.FormattedDateTimeNow}@relativity.com", new List<int> {_groupId});
		}

		public override void TestTeardown()
		{
			base.TestTeardown();
			Group.DeleteGroup(_groupId);
			User.DeleteUser(_userModel.ArtifactID);
		}

		[IdentifiedTest("3e87490e-458c-4501-a6d2-443e45270628")]
		public void MissingSourceWorkspacePermission()
		{
			var jobHistoryRequest = new JobHistoryRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			};
			var client = Helper.CreateUserProxy<IJobHistoryManager>(_userModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetJobHistoryAsync(jobHistoryRequest).Result);
		}

		[IdentifiedTest("051c7ce7-3bf6-4d80-affd-aeabd353a47d")]
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

		[IdentifiedTest("47249b2c-dce4-4d7c-a772-07fe5c0cdb01")]
		[TestInQuarantine(TestQuarantineState.UnderObservation, 
			"Moved back to quarantine cause it failed when pulled into standard tests pool.")]
		
		public void MissingTargetWorkspacePermission()
		{
			Group.AddGroupToWorkspace(SourceWorkspaceArtifactID, _groupId);

			var jobHistoryRequest = new JobHistoryRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactID,
				Page = 0,
				PageSize = 10
			};

			var jobHistoryClient = Helper.CreateUserProxy<IJobHistoryManager>(_userModel.EmailAddress);
			JobHistorySummaryModel jobHistory = jobHistoryClient.GetJobHistoryAsync(jobHistoryRequest).Result;

			Assert.That(jobHistory.Data.Length, Is.EqualTo(0));
		}

		[IdentifiedTest("a18e2360-2a58-41e3-8b69-663f6f0d6c80")]
		public void MissingIntegrationPointPermissionsInSourceWorkspace()
		{
			//Arrange
			Group.AddGroupToWorkspace(SourceWorkspaceArtifactID, _groupId);
			Group.AddGroupToWorkspace(TargetWorkspaceArtifactID, _groupId);

			RemoveIntegrationPointPermissionsFromSourceWorkspace();
			
			var jobHistoryRequest = new JobHistoryRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactID,
				Page = 0,
				PageSize = 10
			};

			var jobHistoryClient = Helper.CreateUserProxy<IJobHistoryManager>(_userModel.EmailAddress);

			//Act & Assert
			Assert.That(() => jobHistoryClient.GetJobHistoryAsync(jobHistoryRequest).Result,
				Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
					.And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));
		}

		private void RemoveIntegrationPointPermissionsFromSourceWorkspace()
		{
			GroupPermissions permissions = Permission.GetGroupPermissions(SourceWorkspaceArtifactID, _groupId);

			RemoveViewPermission(permissions, ObjectTypes.IntegrationPoint);
			RemoveViewPermission(permissions, ObjectTypes.IntegrationPointProfile);
			RemoveViewPermission(permissions, ObjectTypes.IntegrationPointType);
			RemoveViewPermission(permissions, ObjectTypes.SourceProvider);
			RemoveViewPermission(permissions, ObjectTypes.DestinationProvider);
			RemoveViewPermission(permissions, ObjectTypes.JobHistoryError);

			Permission.SavePermission(SourceWorkspaceArtifactID, permissions);
		}

		private void RemoveViewPermission(GroupPermissions permissions, string objectType)
		{
			var permissionsForIntegrationPoint = permissions.ObjectPermissions.FindPermission(objectType);
			permissionsForIntegrationPoint.ViewSelected = false;
		}

		private void RunDefaultIntegrationPoint()
		{
			Core.Models.IntegrationPointModel ipModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, $"ip_{Utils.FormattedDateTimeNow}", "Append Only");
			ipModel.Destination = CreateSerializedDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum.AppendOnly, TargetWorkspaceArtifactID);
			Core.Models.IntegrationPointModel ip = CreateOrUpdateIntegrationPoint(ipModel);

			var client = Helper.CreateAdminProxy<IIntegrationPointManager>();
			client.RunIntegrationPointAsync(SourceWorkspaceArtifactID, ip.ArtifactID).Wait();

			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, ip.ArtifactID);
		}

		private void ModifyJobHistoryItem()
		{
			//This is needed, as Integration Point, which has been run, doesn't contain any documents
			var dbContext = Helper.GetDBContext(SourceWorkspaceArtifactID);
			dbContext.ExecuteNonQuerySQLStatement(@"UPDATE [JobHistory] SET [ItemsTransferred] = 1, [TotalItems] = 1");
		}
	}
}