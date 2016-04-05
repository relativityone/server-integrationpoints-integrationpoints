using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class ExportJobErrorService
	{
		private readonly ITempDocTableHelper _docTableHelper;

		public ExportJobErrorService(ITempDocTableHelper docTableHelper)
		{
			_docTableHelper = docTableHelper;
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
			_docTableHelper.RemoveErrorDocument(documentIdentifier);
		}
	}
}
