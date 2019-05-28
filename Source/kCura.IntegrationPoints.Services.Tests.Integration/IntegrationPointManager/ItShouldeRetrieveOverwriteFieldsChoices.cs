using System;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	public class ItShouldeRetrieveOverwriteFieldsChoices : SourceProviderTemplate
	{
		public ItShouldeRetrieveOverwriteFieldsChoices() : base($"choices_{Utils.FormattedDateTimeNow}")
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

		[IdentifiedTest("fedaa01b-6688-4275-bb5a-8ed17aa2f3dc")]
		public void Execute()
		{
			var expectedChoices = ChoicesHelper.GetAllChoiceUsingFieldGuid(IntegrationPointFieldGuids.OverwriteFields, WorkspaceArtifactId, Helper);

			var overwriteFieldModels = _client.GetOverwriteFieldsChoicesAsync(WorkspaceArtifactId).Result;

			Assert.That(overwriteFieldModels,
				Is.EquivalentTo(expectedChoices.Keys).Using(new Func<OverwriteFieldsModel, string, bool>((x, y) => (x.Name == y) && (x.ArtifactId == expectedChoices[y]))));
		}
	}
}