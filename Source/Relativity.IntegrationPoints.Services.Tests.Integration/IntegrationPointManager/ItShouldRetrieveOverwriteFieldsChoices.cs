using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services.Tests.Integration.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	[Feature.DataTransfer.IntegrationPoints]
	public class ItShouldRetrieveOverwriteFieldsChoices : SourceProviderTemplate
	{
		public ItShouldRetrieveOverwriteFieldsChoices() : base($"choices_{Utils.FormattedDateTimeNow}")
		{
		}

		private IIntegrationPointManager _sut;

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_sut = Helper.CreateProxy<IIntegrationPointManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_sut.Dispose();
		}

		[IdentifiedTest("fedaa01b-6688-4275-bb5a-8ed17aa2f3dc")]
		public void GetOverwriteFieldsChoicesAsync_ShouldRetrieveOverwriteFieldsChoices()
		{
			IDictionary<string, int> expectedChoices = ChoicesHelper.GetAllChoiceUsingFieldGuid(IntegrationPointFieldGuids.OverwriteFieldsGuid, WorkspaceArtifactId, Helper);

			IList<OverwriteFieldsModel> overwriteFieldModels = _sut.GetOverwriteFieldsChoicesAsync(WorkspaceArtifactId).Result;

			Assert.That(overwriteFieldModels,
				Is.EquivalentTo(expectedChoices.Keys).Using(new Func<OverwriteFieldsModel, string, bool>((x, y) => (x.Name == y) && (x.ArtifactId == expectedChoices[y]))));
		}
	}
}