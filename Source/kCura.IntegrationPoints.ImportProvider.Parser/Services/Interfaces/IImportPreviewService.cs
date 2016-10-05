using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Services.Interfaces
{
    public interface IImportPreviewService
    {
        ImportPreviewTable PreviewLoadFile(string filePath, int workspaceID);

        ImportPreviewTable PreviewErrors();

        ImportPreviewTable PreviewChoicesFolders();

    }
}
