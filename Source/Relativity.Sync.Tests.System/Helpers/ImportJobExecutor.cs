using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Tests.System.Helpers
{
	internal static class ImportJobExecutor
	{
		public static async Task<ImportJobErrors> ExecuteAsync<T>(T job) where T : IImportNotifier, IImportBulkArtifactJob
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

			await Task.Run(() => job.Execute()).ConfigureAwait(false);

			var jobResult = new ImportJobErrors(errorMessages);
			return jobResult;
		}
	}
}
