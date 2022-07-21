using System.IO;

namespace Relativity.Sync.Transfer.StreamWrappers
{
    internal interface IImportStreamBuilder
    {
        Stream Create(IRetriableStreamBuilder streamBuilder, StreamEncoding encoding, int documentArtifactID);
    }
}
