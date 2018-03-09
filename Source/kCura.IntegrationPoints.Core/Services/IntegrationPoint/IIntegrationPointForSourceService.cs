using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
	public interface IIntegrationPointForSourceService
	{
		IList<Data.IntegrationPoint> GetAllForSourceProvider(string sourceProviderGuid);
	}
}