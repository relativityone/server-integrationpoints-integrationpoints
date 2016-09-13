
namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Validators
{
	public interface IPreCascadeDeleteEventHandlerValidator
	{
		void Validate(int wkspId, int integrationPointId);
	}
}
