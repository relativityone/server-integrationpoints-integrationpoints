using System.Collections.Generic;
using Relativity.Sync.WorkspaceGenerator.Import;

namespace Relativity.Sync.WorkspaceGenerator.FileGenerating
{
    public interface IImageGenerator
    {
        IEnumerable<ImageFileDTO> GetImagesForDocument(Document document);
        int SetPerDocumentCount { get; }
    }
}