using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Services
{
	/// <summary>
	/// Get information about the documents in ECA case such as pushed to review, included, excluded, untagged, etc.
	/// </summary>
	public class DocumentManager : IDocumentManager
	{
		public async Task<bool> PingAsync()
		{
			return await Task.Run(() => true).ConfigureAwait(false);
		}

		public void Dispose() { }
	}
}