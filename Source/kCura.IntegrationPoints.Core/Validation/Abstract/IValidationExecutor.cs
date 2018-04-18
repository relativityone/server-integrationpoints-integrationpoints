

namespace kCura.IntegrationPoints.Core.Validation
{
	public interface IValidationExecutor
	{
		void ValidateOnRun(ValidationContext validationContext);

		void ValidateOnSave(ValidationContext validationContext);

		void ValidateOnStop(ValidationContext validationContext);
	}
}
