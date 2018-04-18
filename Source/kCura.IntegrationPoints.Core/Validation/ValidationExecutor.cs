using System;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;


namespace kCura.IntegrationPoints.Core.Validation
{
	public class ValidationExecutor : IValidationExecutor
	{
		private readonly IIntegrationPointPermissionValidator _permissionValidator;

		private readonly IIntegrationPointProviderValidator _integrationModelValidator;

		private enum OperationType
		{
			Run,
			Save,
			Stop
		}

		public ValidationExecutor(IIntegrationPointProviderValidator integrationModelValidator,
			IIntegrationPointPermissionValidator permissionValidator)
		{
			_integrationModelValidator = integrationModelValidator;
			_permissionValidator = permissionValidator;
		}

		public void ValidateOnRun(ValidationContext validationContext)
		{
			CheckPermissions(validationContext, OperationType.Run);
			CheckExecutionConstraints(validationContext); 
		}

		public void ValidateOnSave(ValidationContext validationContext)
		{
			CheckPermissions(validationContext, OperationType.Save);
			CheckExecutionConstraints(validationContext);
		}

		public void ValidateOnStop(ValidationContext validationContext)
		{
			CheckPermissions(validationContext, OperationType.Stop);
		}

		private void CheckExecutionConstraints(ValidationContext validationContext)
		{
			ValidationResult validationResult = _integrationModelValidator.Validate(validationContext.Model, validationContext.SourceProvider, validationContext.DestinationProvider, 
				validationContext.IntegrationPointType, validationContext.ObjectTypeGuid);

			if (!validationResult.IsValid)
			{
				throw new IntegrationPointProviderValidationException(validationResult);
			}
		}

		private void CheckPermissions(ValidationContext validationContext, OperationType operationType)
		{
			ValidationResult permissionCheck = Validate(validationContext, operationType);

			if (validationContext.UserId == 0)
			{
				permissionCheck.Add(Constants.IntegrationPoints.NO_USERID);
			}

			if (!permissionCheck.IsValid)
			{
				throw new PermissionException(string.Join(Environment.NewLine, permissionCheck.Messages));
			}
		}

		private ValidationResult Validate(ValidationContext validationContext, OperationType operationType)
		{
			switch (operationType)
			{
				case OperationType.Stop:
					return _permissionValidator.ValidateStop(validationContext.Model, validationContext.SourceProvider,
						validationContext.DestinationProvider, validationContext.IntegrationPointType, validationContext.ObjectTypeGuid);
				case OperationType.Save:
					return _permissionValidator.ValidateSave(validationContext.Model, validationContext.SourceProvider,
						validationContext.DestinationProvider, validationContext.IntegrationPointType, validationContext.ObjectTypeGuid);
				default:
					return _permissionValidator.Validate(validationContext.Model, validationContext.SourceProvider,
						validationContext.DestinationProvider, validationContext.IntegrationPointType, validationContext.ObjectTypeGuid);
			}
		}
	}
}
