using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using NUnit.Framework;
using TestContext = kCura.IntegrationPoints.UITests.Configuration.TestContext;

namespace kCura.IntegrationPoints.UITests.Validation.RelativityProviderValidation
{
	public class LinksSavedSearchToFolderValidator :RelativityProviderValidatorBase
	{
		protected override void ValidateGeneralModel(Dictionary<string, string> propertiesTableDictionary, RelativityProviderModel model,
			TestContext sourceContext, TestContext destinationContext)
		{
			base.ValidateGeneralModel(propertiesTableDictionary, model, sourceContext, destinationContext);
			Assert.AreEqual(model.GetValueOrDefault(x => x.MoveExistingDocuments).AsHtmlString(), propertiesTableDictionary["Move Existing Docs:"]);
			Assert.AreEqual(destinationContext.WorkspaceName, propertiesTableDictionary["Destination Folder:"]); // test selects root item in destination folder dropdown, which is equal to workspace name
		}
	}
}