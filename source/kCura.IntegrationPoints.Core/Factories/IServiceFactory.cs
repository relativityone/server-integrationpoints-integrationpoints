using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories
{
	public interface IServiceFactory
	{
		IIntegrationPointService CreateIntegrationPointService(IHelper helper,
			IHelper targetHelper);

		IFieldCatalogService CreateFieldCatalogService(IHelper targetHelper);

		IJobHistoryService CreateJobHistoryService(IHelper helper, IHelper targetHelper);
	}
}