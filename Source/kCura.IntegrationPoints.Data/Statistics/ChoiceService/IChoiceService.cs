using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Statistics
{
    public interface IChoiceService
    {
        /// <summary>
        /// Returns the ArtifactID of the 'Yes' choice belonging to the 'HasImages' field of 'Document' object type
        /// </summary>
        /// <param name="workspaceArtifactId">The workspace ArtifactID where to read from.</param>
        Task<int> GetGuidOfYesChoiceOnHasImagesAsync(int workspaceArtifactId);
    }
}
