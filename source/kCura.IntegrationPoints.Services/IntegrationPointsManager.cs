using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Services
{
    public class IntegrationPointsManager : IIntegrationPointsManager
    {
		public async Task<bool> PingAsync()
		{
			return await Task.Run(() => true).ConfigureAwait(false);
		}

		public void Dispose()
	    {
	    }
    }
}
