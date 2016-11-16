using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain;

namespace kCura.IntegrationPoints.Core.Validation.Implementation
{
	public class IntegrationModelValidator : IIntegrationModelValidator
	{
		private readonly IValidatorFactory _factory;

		public IntegrationModelValidator(IValidatorFactory factory)
		{
			_factory = factory;
		}

		public void Validate(IntegrationModel model)
		{
			List<IProviderValidator> validators = _factory.CreateIntegrationModelValidators(model);

			foreach (IProviderValidator validator in validators)
			{
				validator.Validate();
			}
		}
	}
}
