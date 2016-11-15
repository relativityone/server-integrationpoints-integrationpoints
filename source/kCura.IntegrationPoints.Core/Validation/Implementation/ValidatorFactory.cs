using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain;

namespace kCura.IntegrationPoints.Core.Validation.Implementation
{
	public class ValidatorFactory : IValidatorFactory
	{
		private readonly List<IProviderValidator> _validators = new List<IProviderValidator>();

		public List<IProviderValidator> CreateIntegrationModelValidators(IntegrationModel model)
		{
			_validators.Add(new EmailValidator(model.NotificationEmails));
			_validators.Add(new FieldMappingsValidator(model.Map));
			_validators.Add(new SchedulerValidator(model.Scheduler));


			return _validators;
		}
	}
}
