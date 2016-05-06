using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Services
{
	public abstract class KeplerServiceBase : IKeplerService
	{
		public async Task<bool> PingAsync()
		{
			return await Task.Run(() => true).ConfigureAwait(false);
		}
	}
}