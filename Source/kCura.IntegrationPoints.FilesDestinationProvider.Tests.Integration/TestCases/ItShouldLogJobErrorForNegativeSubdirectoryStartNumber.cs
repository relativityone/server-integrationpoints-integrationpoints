using System;
using System.IO;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NSubstitute;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldLogJobErrorForNegativeSubdirectoryStartNumber : ExportTestCaseBase
	{
		private readonly IJobHistoryErrorService _jobHistoryErrorService;

		public ItShouldLogJobErrorForNegativeSubdirectoryStartNumber(IJobHistoryErrorService jobHistoryErrorService)
		{
			_jobHistoryErrorService = jobHistoryErrorService;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			_jobHistoryErrorService.ClearReceivedCalls();

			settings.SubdirectoryStartNumber = -1;
			settings.ExportNatives = true;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			const string expectedMessage = "Arithmetic operation resulted in an overflow.";
			_jobHistoryErrorService.Received().AddError(ErrorTypeChoices.JobHistoryErrorJob, Arg.Is<Exception>(x => x.Message == expectedMessage));
		}
	}
}