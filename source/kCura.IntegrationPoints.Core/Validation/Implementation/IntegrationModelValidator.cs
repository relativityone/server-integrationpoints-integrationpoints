using kCura.IntegrationPoints.Core.Models;

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
			var validators = _factory.CreateIntegrationModelValidators(model);

			foreach (var validator in validators)
			{
				validator.Validate();
			}
		}
	}
}
