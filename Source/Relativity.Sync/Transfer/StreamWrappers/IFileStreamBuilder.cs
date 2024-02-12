using System.IO;

namespace Relativity.Sync.Transfer.StreamWrappers
{
    internal interface IFileStreamBuilder
    {
        FileStream Create(IFile file);
    }
}
