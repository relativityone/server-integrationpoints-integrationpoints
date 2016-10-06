using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using kCura.WinEDDS;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.ImportProvider.Parser.Services.Interfaces;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Services
{
    public class ImportPreviewService : IImportPreviewService
    {
        private ICredentialProvider _credentialProvider;
        private Dictionary<int, PreviewJob> _loadFilePreviewers;

        public ImportPreviewService(ICredentialProvider credentialProvider)
        {
            _credentialProvider = credentialProvider;
            _loadFilePreviewers = new Dictionary<int, PreviewJob>();
        }
        
        public int CreatePreviewJob(string loadFile, int workspaceId)
        {
            int handlerNum = _loadFilePreviewers.Count + 1;
            _loadFilePreviewers.Add(handlerNum, new PreviewJob(_credentialProvider, loadFile, workspaceId));

            return handlerNum;
        }

        public void StartPreviewJob(int jobId)
        {
            Task.Run(()=>{
                _loadFilePreviewers[jobId].StartRead();
            });
        }

        public ImportPreviewStatus CheckProgress(int jobId)
        {

            ImportPreviewStatus status = new ImportPreviewStatus
            {
                TotalBytes = _loadFilePreviewers[jobId].TotalBytes,
                BytesRead = _loadFilePreviewers[jobId].BytesRead,
                IsComplete = _loadFilePreviewers[jobId].IsComplete,
                StepSize = _loadFilePreviewers[jobId].StepSize
            };

            return status;
        }

        public bool IsJobComplete(int jobId)
        {
            return _loadFilePreviewers[jobId].IsComplete;
        }

        public ImportPreviewTable RetrievePreviewTable(int jobId)
        {
            if (!_loadFilePreviewers.ContainsKey(jobId))
            {
                return null;
            }
            ImportPreviewTable table = _loadFilePreviewers[jobId].PreviewTable;
            if (table != null )//|| IsJobComplete(jobId))
            {
                //Dispose here if job is complete
                _loadFilePreviewers[jobId].DisposePreviewJob();
                _loadFilePreviewers.Remove(jobId);
            }

            return table;
        }

        public ImportPreviewTable PreviewErrors()
        {
            throw new NotImplementedException();
        }

        public ImportPreviewTable PreviewChoicesFolders()
        {
            throw new NotImplementedException();
        }
    }    
}
