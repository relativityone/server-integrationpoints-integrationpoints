using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface ICodeRepository
	{
		Task<ArtifactDTO[]> RetrieveCodeAsync(string name);
	}
}