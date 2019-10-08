using System;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.IntegrationPointProfileManager
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class ItShouldHandleRetrievingNoIntegrationPointProfiles : RelativityProviderTemplate
	{
		public ItShouldHandleRetrievingNoIntegrationPointProfiles() : base($"KeplerService_{Utils.FormattedDateTimeNow}", $"KeplerService_Target_{Utils.FormattedDateTimeNow}")
		{
		}

		private IIntegrationPointProfileManager _client;

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_client = Helper.CreateProxy<IIntegrationPointProfileManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client.Dispose();
		}

		[IdentifiedTest("8fce8bdc-f3f1-4f03-9c30-3a6c911af6bc")]
		public void ItShouldHandleEmptyList()
		{
			var result = _client.GetAllIntegrationPointProfilesAsync(WorkspaceArtifactId).Result;
			Assert.That(result.Count, Is.EqualTo(0));
		}

		[IdentifiedTest("14fe3521-2942-494a-bd36-93c3bc9947b1")]
		public void ItShouldThrowExceptionForNonExistingIntegrationPoint()
		{
			int integrationPointId = 413328;

			Assert.That(() => _client.GetIntegrationPointProfileAsync(WorkspaceArtifactId, integrationPointId).Wait(),
				Throws.TypeOf<AggregateException>().With.InnerException.With.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));
		}
	}
}