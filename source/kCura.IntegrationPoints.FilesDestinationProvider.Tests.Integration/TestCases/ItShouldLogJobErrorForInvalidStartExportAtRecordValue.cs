using System.Data;
using System.IO;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NSubstitute;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldLogJobErrorForInvalidStartExportAtRecordValue : ExportTestCaseBase
	{
		private readonly IJobHistoryErrorService _jobHistoryErrorService;

		public ItShouldLogJobErrorForInvalidStartExportAtRecordValue(IJobHistoryErrorService jobHistoryErrorService)
		{
			_jobHistoryErrorService = jobHistoryErrorService;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			_jobHistoryErrorService.ClearReceivedCalls();

			settings.CopyFileFromRepository = true;
			settings.IncludeNativeFilesPath = true;

			settings.StartExportAtRecord = int.MaxValue;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			string expectedMessage =
				$"The chosen start item number ({ExportSettings.StartExportAtRecord}) exceeds the number of \r\n items in the export ({documents.Rows.Count}).  Export halted.";
			_jobHistoryErrorService.Received().AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, expectedMessage, string.Empty);
		}
	}
}