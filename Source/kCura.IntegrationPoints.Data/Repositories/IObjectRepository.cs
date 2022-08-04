using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IObjectRepository
    {
        /// <summary>
        /// Retrieves all fields from the object of which specified during the repository creation.
        /// </summary>
        /// <param name="fields">an array of fields with in the Relativity Field to be retrieved</param>
        /// <returns>all relativity field objects from the specified object type.</returns>
        Task<ArtifactDTO[]> GetFieldsFromObjects(string[] fields);
    }
}