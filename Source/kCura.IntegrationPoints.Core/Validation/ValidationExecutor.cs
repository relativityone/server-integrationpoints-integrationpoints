using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Validation
{
    public class ValidationExecutor : IValidationExecutor
    {
        private readonly IAPILog _logger;
        private readonly IIntegrationPointPermissionValidator _permissionValidator;
        private readonly IIntegrationPointProviderValidator _integrationModelValidator;
        private enum OperationType
        {
            Run,
            Save,
            Stop,
            LoadProfile
        }

        public ValidationExecutor(IIntegrationPointProviderValidator integrationModelValidator,
            IIntegrationPointPermissionValidator permissionValidator, IHelper helper)
        {
            _integrationModelValidator = integrationModelValidator;
            _permissionValidator = permissionValidator;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<ValidationExecutor>();
        }

        public void ValidateOnRun(ValidationContext validationContext)
        {
            CheckPermissions(validationContext, OperationType.Run);
            CheckExecutionConstraints(validationContext, OperationType.Run);
        }

        public void ValidateOnSave(ValidationContext validationContext)
        {
            CheckPermissions(validationContext, OperationType.Save);
            CheckExecutionConstraints(validationContext, OperationType.Save);
        }

        public void ValidateOnStop(ValidationContext validationContext)
        {
            CheckPermissions(validationContext, OperationType.Stop);
        }

        public ValidationResult ValidateOnProfile(ValidationContext validationContext)
        {
            return ValidateExecutionConstraints(validationContext, OperationType.LoadProfile);
        }

        private void CheckExecutionConstraints(ValidationContext validationContext, OperationType operationType)
        {
            ValidationResult validationResult = ValidateExecutionConstraints(validationContext, operationType);
            if (!validationResult.IsValid)
            {
                throw new IntegrationPointValidationException(validationResult);
            }
        }

        private ValidationResult ValidateExecutionConstraints(ValidationContext validationContext, OperationType operationType)
        {
            ValidationResult validationResult = _integrationModelValidator.Validate(
                validationContext.Model,
                validationContext.SourceProvider,
                validationContext.DestinationProvider,
                validationContext.IntegrationPointType,
                validationContext.ObjectTypeGuid,
                validationContext.UserId);

            LogValidationErrors(operationType, validationResult);
            return validationResult;
        }

        private void CheckPermissions(ValidationContext validationContext, OperationType operationType)
        {
            ValidationResult permissionCheck;
            if (validationContext.UserId == 0)
            {
                permissionCheck = new ValidationResult();
                permissionCheck.Add(Constants.IntegrationPoints.NO_USERID);
            }
            else
            {
                permissionCheck = ValidatePermission(validationContext, operationType);
            }

            if (!permissionCheck.IsValid)
            {
                throw new IntegrationPointValidationException(permissionCheck);
            }
        }

        private ValidationResult ValidatePermission(ValidationContext validationContext, OperationType operationType)
        {
            ValidationResult validationResult;
            switch (operationType)
            {
                case OperationType.Stop:
                    validationResult = _permissionValidator.ValidateStop(validationContext.Model, validationContext.SourceProvider, validationContext.DestinationProvider,
                        validationContext.IntegrationPointType, validationContext.ObjectTypeGuid, validationContext.UserId);
                    break;
                case OperationType.Save:
                    validationResult = _permissionValidator.ValidateSave(validationContext.Model, validationContext.SourceProvider, validationContext.DestinationProvider,
                        validationContext.IntegrationPointType, validationContext.ObjectTypeGuid, validationContext.UserId);
                    break;
                default:
                    validationResult = _permissionValidator.Validate(validationContext.Model, validationContext.SourceProvider, validationContext.DestinationProvider,
                        validationContext.IntegrationPointType, validationContext.ObjectTypeGuid, validationContext.UserId);
                    break;
            }

            LogValidationErrors(operationType, validationResult);
            return validationResult;
        }

        private void LogValidationErrors(OperationType operationType, ValidationResult validationResult)
        {
            if (validationResult.Messages == null)
            {
                return;
            }

            foreach (ValidationMessage validationMessage in validationResult.Messages)
            {
                _logger.LogError("Integration Point validation failed. Operation: {operationType}, ErrorCode: {errorCode}, ShortMessage: {shortMessage}",
                    operationType, validationMessage.ErrorCode, validationMessage.ShortMessage);
            }
        }
    }
}
