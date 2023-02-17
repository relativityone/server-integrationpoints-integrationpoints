using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.ImportProvider.Parser.Services.Interfaces;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Services
{
    public class ImportPreviewService : IImportPreviewService
    {
        private readonly Dictionary<int, IPreviewJob> _loadFilePreviewers;
        private readonly IPreviewJobFactory _previewJobFactory;

        public ImportPreviewService(IPreviewJobFactory previewJobFactory)
        {
            _previewJobFactory = previewJobFactory;
            _loadFilePreviewers = new Dictionary<int, IPreviewJob>();
        }

        public int CreatePreviewJob(ImportPreviewSettings settings)
        {
            int jobId = _loadFilePreviewers.Count + 1;
            _loadFilePreviewers.Add(jobId, _previewJobFactory.GetPreviewJob(settings));

            return jobId;
        }

        public void StartPreviewJob(int jobId)
        {
            if (!_loadFilePreviewers.ContainsKey(jobId))
            {
                throw new KeyNotFoundException(string.Format("There is no current Preview Job of jobId {0}", jobId));
            }

            Task.Run(() =>
            {
                _loadFilePreviewers[jobId].StartRead();
            });
        }

        public ImportPreviewStatus CheckProgress(int jobId)
        {
            if (!_loadFilePreviewers.ContainsKey(jobId))
            {
                throw new KeyNotFoundException(string.Format("There is no current Preview Job of jobId {0}", jobId));
            }

            ImportPreviewStatus status = new ImportPreviewStatus
            {
                TotalBytes = _loadFilePreviewers[jobId].TotalBytes,
                BytesRead = _loadFilePreviewers[jobId].BytesRead,
                IsComplete = _loadFilePreviewers[jobId].IsComplete,
                StepSize = _loadFilePreviewers[jobId].StepSize,
                IsFailed = _loadFilePreviewers[jobId].IsFailed,
                ErrorMessage = _loadFilePreviewers[jobId].ErrorMessage
            };

            if (status.IsFailed)
            {
                // Dispose here if job has failed
                _loadFilePreviewers[jobId].DisposePreviewJob();
                _loadFilePreviewers.Remove(jobId);
            }

            return status;
        }

        public bool IsJobComplete(int jobId)
        {
            if (!_loadFilePreviewers.ContainsKey(jobId))
            {
                throw new KeyNotFoundException(string.Format("There is no current Preview Job of jobId {0}", jobId));
            }

            return _loadFilePreviewers[jobId].IsComplete;
        }

        public ImportPreviewTable RetrievePreviewTable(int jobId)
        {
            if (!_loadFilePreviewers.ContainsKey(jobId))
            {
                throw new KeyNotFoundException(string.Format("There is no current Preview Job of jobId {0}", jobId));
            }

            ImportPreviewTable table = _loadFilePreviewers[jobId].PreviewTable;
            if (table != null || IsJobComplete(jobId))
            {
                // Dispose here if job is complete
                _loadFilePreviewers[jobId].DisposePreviewJob();
                _loadFilePreviewers.Remove(jobId);
            }

            return table;
        }

    }
}
