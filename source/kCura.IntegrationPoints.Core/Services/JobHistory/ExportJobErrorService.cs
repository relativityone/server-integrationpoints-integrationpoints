using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class ExportJobErrorService
	{
		private readonly IScratchTableRepository[] _scratchTable;

		public ExportJobErrorService(IScratchTableRepository[] scratchTable)
		{
			_scratchTable = scratchTable;
		}

		public void SubscribeToBatchReporterEvents(object batchReporter)
		{
			if (batchReporter is IBatchReporter)
			{
				((IBatchReporter)batchReporter).OnDocumentError += new RowError(OnRowError);
			}
		}

		private void OnRowError(string documentIdentifier, string errorMessage)
		{
			foreach (IScratchTableRepository table in _scratchTable)
			{
				table.RemoveErrorDocument(documentIdentifier);
			}
		}
	}
}