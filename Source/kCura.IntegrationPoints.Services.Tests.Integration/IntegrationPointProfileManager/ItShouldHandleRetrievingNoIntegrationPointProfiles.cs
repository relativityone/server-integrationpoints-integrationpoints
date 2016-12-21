using System;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointProfileManager
{
	[TestFixture]
	public class ItShouldHandleRetrievingNoIntegrationPointProfiles : RelativityProviderTemplate
	{
		public ItShouldHandleRetrievingNoIntegrationPointProfiles() : base($"KeplerService_{Utils.FormatedDateTimeNow}", $"KeplerService_Target_{Utils.FormatedDateTimeNow}")
		{
		}

		private IIntegrationPointProfileManager _client;

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_client = Helper.CreateAdminProxy<IIntegrationPointProfileManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client.Dispose();
		}

		[Test]
		public void ItShouldHandleEmptyList()
		{
			var result = _client.GetAllIntegrationPointProfilesAsync(WorkspaceArtifactId).Result;
			Assert.That(result.Count, Is.EqualTo(0));
		}

		[Test]
		public void ItShouldThrowExceptionForNonExistingIntegrationPoint()
		{
			int integrationPointId = 413328;

			Assert.That(() => _client.GetIntegrationPointProfileAsync(WorkspaceArtifactId, integrationPointId).Wait(),
				Throws.TypeOf<AggregateException>().With.InnerException.With.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));
		}
	}
}