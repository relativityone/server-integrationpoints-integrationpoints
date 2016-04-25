using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IObjectRepository
	{
		Task<ArtifactDTO[]> GetFieldsFromObjects(string[] fields);
	}
}