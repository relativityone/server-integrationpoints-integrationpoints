using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using NUnit.Framework;
using NUnit.Framework.Constraints;
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

		private void AssertPermissionErrorMessage(ActualValueDelegate<object> action)
		{
			Assert.That(action, Throws.TypeOf<AggregateException>().With.InnerException.With.Message.EqualTo("You do not have permission to access this service."));
		}

		[Test]
		public void IDocumentManager_GetCurrentPromotionStatusAsync_AccessDenied()
		{
			CurrentPromotionStatusRequest request = new CurrentPromotionStatusRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			};
			var client = Helper.CreateUserProxy<IDocumentManager>(UserModel.EmailAddress);
			AssertPermissionErrorMessage(() => client.GetCurrentPromotionStatusAsync(request).Result);
		}

		[Test]
		public void IDocumentManager_GetHistoricalPromotionStatusAsync_AccessDenied()
		{
			HistoricalPromotionStatusRequest request = new HistoricalPromotionStatusRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			};
			var client = Helper.CreateUserProxy<IDocumentManager>(UserModel.EmailAddress);
			AssertPermissionErrorMessage(() => client.GetHistoricalPromotionStatusAsync(request).Result);
		}

		[Test]
		public void IDocumentManager_GetPercentagePushedToReviewAsync_AccessDenied()
		{
			PercentagePushedToReviewRequest request = new PercentagePushedToReviewRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			};
			var client = Helper.CreateUserProxy<IDocumentManager>(UserModel.EmailAddress);
			AssertPermissionErrorMessage(() => client.GetPercentagePushedToReviewAsync(request).Result);
		}

		[Test]
		public void IIntegrationPointManager_CreateIntegrationPointAsync_AccessDenied()
		{
			CreateIntegrationPointRequest request = new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			};
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			AssertPermissionErrorMessage(() => client.CreateIntegrationPointAsync(request).Result);
		}

		[Test]
		public void IIntegrationPointManager_GetAllIntegrationPointsAsync_AccessDenied()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			AssertPermissionErrorMessage(() => client.GetAllIntegrationPointsAsync(WorkspaceArtifactId).Result);
		}

		[Test]
		public void IIntegrationPointManager_GetIntegrationPointArtifactTypeIdAsync_AccessDenied()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			AssertPermissionErrorMessage(() => client.GetIntegrationPointArtifactTypeIdAsync(WorkspaceArtifactId).Result);
		}

		[Test]
		public void IIntegrationPointManager_GetIntegrationPointAsync_AccessDenied()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			AssertPermissionErrorMessage(() => client.GetIntegrationPointAsync(WorkspaceArtifactId, 1).Result);
		}

		[Test]
		public void IIntegrationPointManager_GetSourceProviderArtifactIdAsync_AccessDenied()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			AssertPermissionErrorMessage(() => client.GetSourceProviderArtifactIdAsync(WorkspaceArtifactId, Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString()).Result);
		}

		[Test]
		public void IIntegrationPointManager_RunIntegrationPointAsync_AccessDenied()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			AssertPermissionErrorMessage(() =>
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
			AssertPermissionErrorMessage(() => client.UpdateIntegrationPointAsync(request).Result);
		}

		[Test]
		public void IJobHistoryManager_GetJobHistoryAsync_AccessDenied()
		{
			var jobHistoryRequest = new JobHistoryRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			};
			var client = Helper.CreateUserProxy<IJobHistoryManager>(UserModel.EmailAddress);
			AssertPermissionErrorMessage(() => client.GetJobHistoryAsync(jobHistoryRequest).Result);
		}
	}
}