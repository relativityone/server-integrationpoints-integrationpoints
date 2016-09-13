
namespace kCura.IntegrationPoints.EventHandlers
{
	public interface IPreCascadeDeleteEventHandlerValidator
	{
		void Validate(int wkspId, int integrationPointId);
	}
}
