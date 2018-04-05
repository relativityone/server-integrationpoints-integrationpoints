using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using NUnit.Framework;
using TestContext = kCura.IntegrationPoints.UITests.Configuration.TestContext;

namespace kCura.IntegrationPoints.UITests.Validation.RelativityProviderValidation
{
	public class SavedSearchToProductionSetValidator : RelativityProviderValidatorBase
	{
		protected override void ValidateGeneralModel(Dictionary<string, string> propertiesTableDictionary, RelativityProviderModel model,
			TestContext sourceContext, TestContext destinationContext)
		{
			base.ValidateGeneralModel(propertiesTableDictionary, model, sourceContext, destinationContext);
			Assert.AreEqual(ImagePrecedenceEnumToString(model.GetValueOrDefault(x => x.ImagePrecedence)), propertiesTableDictionary["Image Precedence:"]);
			Assert.AreEqual(model.GetValueOrDefault(x => x.DestinationProductionName), propertiesTableDictionary["Destination Production Set:"]);
		}
	}
}