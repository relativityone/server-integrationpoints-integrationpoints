using System;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services.Tests.Integration.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.IntegrationPointProfileManager
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class ItShouldeRetrieveOverwriteFieldsChoices : SourceProviderTemplate
	{
		public ItShouldeRetrieveOverwriteFieldsChoices() : base($"choices_{Utils.FormattedDateTimeNow}")
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