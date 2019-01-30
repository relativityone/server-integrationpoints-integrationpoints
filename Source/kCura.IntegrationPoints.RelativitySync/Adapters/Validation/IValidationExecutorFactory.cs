using kCura.IntegrationPoints.Core.Validation;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	public interface IValidationExecutorFactory
	{
		IValidationExecutor CreateValidationExecutor();
	}
}