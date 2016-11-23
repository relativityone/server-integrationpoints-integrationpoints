using System;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace kCura.IntegrationPoints.Services.Tests.Integration.Permissions
{
	public abstract class KeplerServiceMissingPermissionTests : SourceProviderTemplate
	{
		protected static string Identifier => $"{DateTime.Now:yyyyMMddHHmmss}";

		protected string Username;

		protected KeplerServiceMissingPermissionTests() : base($"Kepler_Service_{Identifier}")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			CreatePermissionAndSetUsername();
		}

		protected abstract void CreatePermissionAndSetUsername();

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
			var client = Helper.CreateUserProxy<IDocumentManager>(Username);
			AssertPermissionErrorMessage(() => client.GetCurrentPromotionStatusAsync(request).Result);
		}

		[Test]
		public void IDocumentManager_GetHistoricalPromotionStatusAsync_AccessDenied()
		{
			HistoricalPromotionStatusRequest request = new HistoricalPromotionStatusRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			};
			var client = Helper.CreateUserProxy<IDocumentManager>(Username);
			AssertPermissionErrorMessage(() => client.GetHistoricalPromotionStatusAsync(request).Result);
		}

		[Test]
		public void IDocumentManager_GetPercentagePushedToReviewAsync_AccessDenied()
		{
			PercentagePushedToReviewRequest request = new PercentagePushedToReviewRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			};
			var client = Helper.CreateUserProxy<IDocumentManager>(Username);
			AssertPermissionErrorMessage(() => client.GetPercentagePushedToReviewAsync(request).Result);
		}

		[Test]
		public void IIntegrationPointManager_CreateIntegrationPointAsync_AccessDenied()
		{
			CreateIntegrationPointRequest request = new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			};
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(Username);
			AssertPermissionErrorMessage(() => client.CreateIntegrationPointAsync(request).Result);
		}

		[Test]
		public void IIntegrationPointManager_GetAllIntegrationPointsAsync_AccessDenied()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(Username);
			AssertPermissionErrorMessage(() => client.GetAllIntegrationPointsAsync(WorkspaceArtifactId).Result);
		}

		[Test]
		public void IIntegrationPointManager_GetIntegrationPointArtifactTypeIdAsync_AccessDenied()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(Username);
			AssertPermissionErrorMessage(() => client.GetIntegrationPointArtifactTypeIdAsync(WorkspaceArtifactId).Result);
		}

		[Test]
		public void IIntegrationPointManager_GetIntegrationPointAsync_AccessDenied()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(Username);
			AssertPermissionErrorMessage(() => client.GetIntegrationPointAsync(WorkspaceArtifactId, 1).Result);
		}

		[Test]
		public void IIntegrationPointManager_GetSourceProviderArtifactIdAsync_AccessDenied()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(Username);
			AssertPermissionErrorMessage(() => client.GetSourceProviderArtifactIdAsync(WorkspaceArtifactId, Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString()).Result);
		}

		[Test]
		public void IIntegrationPointManager_RunIntegrationPointAsync_AccessDenied()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(Username);
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
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(Username);
			AssertPermissionErrorMessage(() => client.UpdateIntegrationPointAsync(request).Result);
		}

		[Test]
		public void IJobHistoryManager_GetJobHistoryAsync_AccessDenied()
		{
			var jobHistoryRequest = new JobHistoryRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			};
			var client = Helper.CreateUserProxy<IJobHistoryManager>(Username);
			AssertPermissionErrorMessage(() => client.GetJobHistoryAsync(jobHistoryRequest).Result);
		}
	}
}