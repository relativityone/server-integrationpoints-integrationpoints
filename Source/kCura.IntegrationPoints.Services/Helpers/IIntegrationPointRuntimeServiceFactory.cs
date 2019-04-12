using kCura.IntegrationPoints.Core.Services.IntegrationPoint;

namespace kCura.IntegrationPoints.Services.Helpers
{
	public interface IIntegrationPointRuntimeServiceFactory
	{
		IIntegrationPointService CreateIntegrationPointRuntimeService(Core.Models.IntegrationPointModel model);
	}
}
