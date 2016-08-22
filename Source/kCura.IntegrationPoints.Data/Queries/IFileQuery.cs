using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Queries
{
    public interface IFileQuery
    {
        IEnumerable<FileInfo> GetDocumentFiles(string documentArtifactIDs, int fileType);
    }
}