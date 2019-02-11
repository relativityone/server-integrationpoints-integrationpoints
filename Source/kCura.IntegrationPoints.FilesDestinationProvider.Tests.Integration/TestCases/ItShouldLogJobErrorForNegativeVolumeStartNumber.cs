using System;
using System.IO;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	//"This test fails due to missing validator in WinEDDS for StartNumber greater than zero REL-198503"
	internal class ItShouldLogJobErrorForNegativeVolumeStartNumber : ExportTestCaseBase
	{
		private readonly IJobHistoryErrorService _jobHistoryErrorService;

		public ItShouldLogJobErrorForNegativeVolumeStartNumber(IJobHistoryErrorService jobHistoryErrorService)
		{
			_jobHistoryErrorService = jobHistoryErrorService;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			_jobHistoryErrorService.ClearReceivedCalls();

			settings.VolumeStartNumber = -1;
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