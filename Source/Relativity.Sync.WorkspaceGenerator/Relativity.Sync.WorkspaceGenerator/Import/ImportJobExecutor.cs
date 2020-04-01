using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
	internal static class ImportJobExecutor
	{
		public static async Task<ImportJobResult> ExecuteAsync<T>(T job) where T : ImportBulkArtifactJob, IImportNotifier
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
			job.OnMessage += status =>
			{
				Console.WriteLine(status.Message);
			};

			await Task.Run(job.Execute).ConfigureAwait(false);

			var jobResult = new ImportJobResult(errorMessages);
			return jobResult;
		}
	}
}