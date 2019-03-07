using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using NUnit.Framework;
using TestContext = kCura.IntegrationPoints.UITests.Configuration.TestContext;

namespace kCura.IntegrationPoints.UITests.Validation.RelativityProviderValidation
{
	public class ImagesProductionToProductionSetValidator : RelativityProviderValidatorBase
	{
		protected override void ValidateGeneralModel(Dictionary<string, string> propertiesTableDictionary, RelativityProviderModel model,
			TestContext sourceContext, TestContext destinationContext)
		{
			base.ValidateGeneralModel(propertiesTableDictionary, model, sourceContext, destinationContext);
		}
	}
}
