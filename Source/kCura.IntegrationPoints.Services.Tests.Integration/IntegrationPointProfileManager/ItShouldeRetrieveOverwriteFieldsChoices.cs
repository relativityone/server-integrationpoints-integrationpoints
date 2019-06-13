using System;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointProfileManager
{
	[TestFixture]
	public class ItShouldeRetrieveOverwriteFieldsChoices : SourceProviderTemplate
	{
		public ItShouldeRetrieveOverwriteFieldsChoices() : base($"choices_{Utils.FormattedDateTimeNow}")
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

		[IdentifiedTest("d32ae21d-35f7-49d3-b9a7-1937588b8752")]
		public void Execute()
		{
			var expectedChoices = ChoicesHelper.GetAllChoiceUsingFieldGuid(IntegrationPointProfileFieldGuids.OverwriteFieldsGuid, WorkspaceArtifactId, Helper);

			var overwriteFieldModels = _client.GetOverwriteFieldsChoicesAsync(WorkspaceArtifactId).Result;

			Assert.That(overwriteFieldModels,
				Is.EquivalentTo(expectedChoices.Keys).Using(new Func<OverwriteFieldsModel, string, bool>((x, y) => (x.Name == y) && (x.ArtifactId == expectedChoices[y]))));
		}
	}
}