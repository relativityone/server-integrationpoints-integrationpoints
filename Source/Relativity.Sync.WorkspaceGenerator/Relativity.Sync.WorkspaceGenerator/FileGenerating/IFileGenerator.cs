using System.IO;
using System.Threading.Tasks;

namespace Relativity.Sync.WorkspaceGenerator.FileGenerating
{
    public interface IFileGenerator
    {
        Task<FileInfo> GenerateAsync();
    }
}