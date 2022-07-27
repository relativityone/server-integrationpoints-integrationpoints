using System.IO;

namespace Relativity.Sync.Transfer.StreamWrappers
{
    internal class FileStreamBuilder : IFileStreamBuilder
    {
        public FileStream Create(IFile file)
        {
            // https://git.kcura.com/projects/DTX/repos/transfer-api-legacy/browse/Source/Relativity.Transfer.Client.FileShare/FileShareTransferCommand.cs#661
            return new FileStream(file.Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
    }
}
