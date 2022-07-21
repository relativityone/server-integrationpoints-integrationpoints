using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
    internal interface ITagSavedSearchFolder
    {
        Task<int> GetFolderIdAsync(int workspaceArtifactId);
    }
}