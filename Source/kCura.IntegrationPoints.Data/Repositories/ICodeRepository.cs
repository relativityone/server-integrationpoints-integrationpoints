using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface ICodeRepository
    {
        /// <summary>
        /// Retrieves all choices from the given code field name
        /// </summary>
        /// <param name="name">the name of the relativity code object</param>
        /// <returns>all codes that associates with the given code name</returns>
        Task<ArtifactDTO[]> RetrieveCodeAsync(string name);
    }
}