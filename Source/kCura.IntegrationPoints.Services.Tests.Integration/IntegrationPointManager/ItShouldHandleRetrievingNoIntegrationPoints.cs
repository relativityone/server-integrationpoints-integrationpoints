using System;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	[TestFixture]
	public class ItShouldHandleRetrievingNoIntegrationPoints : RelativityProviderTemplate
	{
		public ItShouldHandleRetrievingNoIntegrationPoints() : base($"KeplerService_{Utils.FormattedDateTimeNow}", $"KeplerService_Target_{Utils.FormattedDateTimeNow}")
		{
		}

		private IIntegrationPointManager _client;

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_client = Helper.CreateAdminProxy<IIntegrationPointManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client.Dispose();
		}

		[Test]
		public void ItShouldHandleEmptyList()
		{
			var result = _client.GetAllIntegrationPointsAsync(WorkspaceArtifactId).Result;
			Assert.That(result.Count, Is.EqualTo(0));
		}

		[Test]
		public void ItShouldThrowExceptionForNonExistingIntegrationPoint()
		{
			int integrationPointId = 142123;

			Assert.That(() => _client.GetIntegrationPointAsync(WorkspaceArtifactId, integrationPointId).Wait(),
				Throws.TypeOf<AggregateException>().With.InnerException.With.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));
		}
	}
}