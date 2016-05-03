using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Services
{
	public abstract class KelperServiceBase : IKelperService
	{
		public async Task<bool> PingAsync()
		{
			return await Task.Run(() => true).ConfigureAwait(false);
		}
	}
}