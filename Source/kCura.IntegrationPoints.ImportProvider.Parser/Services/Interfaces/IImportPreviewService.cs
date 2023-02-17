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
        int CreatePreviewJob(ImportPreviewSettings settings);

        void StartPreviewJob(int jobId);

        ImportPreviewStatus CheckProgress(int jobId);

        bool IsJobComplete(int jobId);

        ImportPreviewTable RetrievePreviewTable(int jobId);

    }
}
