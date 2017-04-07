using System.IO;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NSubstitute;
using Directory = kCura.Utility.Directory;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldLogJobErrorWhenNotOverwritingExistingFiles : ExportTestCaseBase
	{
		private readonly IJobHistoryErrorService _jobHistoryErrorService;

		public ItShouldLogJobErrorWhenNotOverwritingExistingFiles(IJobHistoryErrorService jobHistoryErrorService)
		{
			_jobHistoryErrorService = jobHistoryErrorService;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			_jobHistoryErrorService.ClearReceivedCalls();

			settings.OverwriteFiles = false;
			settings = base.Prepare(settings);

			if (!Directory.Instance.Exists(settings.ExportFilesLocation, false))
			{
				Directory.Instance.CreateDirectory(settings.ExportFilesLocation);
			}
			File.Create(Path.Combine(settings.ExportFilesLocation, "All Documents_export.dat")).Dispose();

			return settings;
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			const string expectedMessageBeginning = "Overwrite not selected";
			_jobHistoryErrorService.Received()
				.AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, Arg.Is<string>(x => x.StartsWith(expectedMessageBeginning)), string.Empty);
		}
	}
}