using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Implementation
{
	public class IntegrationModelValidator : IIntegrationModelValidator
	{
		private readonly ILookup<string, IValidator> _validatorsMap;

		public IntegrationModelValidator(IEnumerable<IValidator> validators)
		{
			_validatorsMap = validators.ToLookup(x => x.Key);
		}

		public ValidationResult Validate(IntegrationModel model, SourceProvider sourceProvider, DestinationProvider destinationProvider)
		{
			var result = new ValidationResult();

			if (model.Scheduler.EnableScheduler)
			{
				foreach (var validator in _validatorsMap[Constants.IntegrationPoints.Validation.EMAIL])
				{
					result.Add(validator.Validate(model.NotificationEmails));
				}

				foreach (var validator in _validatorsMap[Constants.IntegrationPoints.Validation.SCHEDULE])
				{
					result.Add(validator.Validate(model.Scheduler));
				}
			}

			foreach (var validator in _validatorsMap[Constants.IntegrationPoints.Validation.FIELD_MAP])
			{
				result.Add(validator.Validate(model.Map));
			}

			foreach (var validator in _validatorsMap[sourceProvider.Identifier])
			{
				result.Add(validator.Validate(model.SourceConfiguration));
			}

			foreach (var validator in _validatorsMap[destinationProvider.Identifier])
			{
				result.Add(validator.Validate(model.Destination));
			}

			return result;
		}
	}
}