using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using kCura.Relativity.DataReaderClient;
using Newtonsoft.Json;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
	internal static class ImportJobExecutor
	{
		public static Task<ImportJobErrors> ExecuteAsync<T>(T job) where T : IImportNotifier, IImportBulkArtifactJob
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

			try
			{
				job.Execute();
			}
			catch (SoapException ex)
			{
				global::System.Console.WriteLine($"Exception occurred when executing IAPI job: {JsonConvert.SerializeObject(ex)}");
			}

			var jobResult = new ImportJobErrors(errorMessages);
			return Task.FromResult(jobResult);
		}
	}
}
