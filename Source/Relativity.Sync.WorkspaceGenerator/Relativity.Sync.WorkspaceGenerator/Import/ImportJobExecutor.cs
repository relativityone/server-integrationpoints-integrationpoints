using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
    internal static class ImportJobExecutor
    {
        public static async Task<ImportJobResult> ExecuteAsync<T>(T job) where T : IImportBulkArtifactJob, IImportNotifier
        {
            var errorMessages = new List<string>();
            job.OnComplete += report =>
            {
                errorMessages.AddRange(report.ErrorRows.Select(e => e.Message));
            };
            job.OnFatalException += report =>
            {
                errorMessages.Add(report.FatalException.ToString());
            };
        

            await Task.Run(job.Execute).ConfigureAwait(false);

            var jobResult = new ImportJobResult(errorMessages);
            return jobResult;
        }
    }
}