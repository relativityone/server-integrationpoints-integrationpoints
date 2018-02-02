using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Validation
{
	public class ExportToLoadFileProviderValidator : BaseUiValidator
	{
		public void ValidateSummaryPage(IntegrationPointDetailsPage integrationPointDetailsPage, IntegrationPointGeneralModel integrationPointModel)
		{
			//TODO finish
			Assert.AreEqual("Relativity (.dat); Unicode", integrationPointDetailsPage.SelectGeneralPropertiesTable().Properties["Load file format:"]);
		}
	}
}
