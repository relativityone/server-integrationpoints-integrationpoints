using System.Threading.Tasks;
using kCura.IntegrationPoints.Services.Interfaces.Private;

namespace kCura.IntegrationPoints.Services
{
    public class IntegrationPointsManager : IIntegrationPointsManager
    {
		public async Task<bool> Ping()
		{
			return await Task.Run(() => true).ConfigureAwait(false);
		}

		public void Dispose()
	    {
	    }
    }
}
