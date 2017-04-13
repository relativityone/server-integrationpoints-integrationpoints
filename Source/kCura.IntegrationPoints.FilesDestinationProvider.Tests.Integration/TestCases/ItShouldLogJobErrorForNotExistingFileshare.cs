using System;
using System.IO;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using NSubstitute;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldLogJobErrorForNotExistingFileshare : IInvalidFileshareExportTestCase
	{
		private readonly IJobHistoryErrorService _jobHistoryErrorService;

		private string _notExistingPath = @"X:\NotExistingPath";

		public ItShouldLogJobErrorForNotExistingFileshare(IJobHistoryErrorService jobHistoryErrorService)
		{
			_jobHistoryErrorService = jobHistoryErrorService;
		}

		public ExportSettings Prepare(ExportSettings settings)
		{
			_jobHistoryErrorService.ClearReceivedCalls();

			while (Directory.Exists(_notExistingPath))
			{
				_notExistingPath += "a";
			}

			settings.ExportFilesLocation = _notExistingPath;
			return settings;
		}

		public void Verify()
		{
			_jobHistoryErrorService.Received().AddError(ErrorTypeChoices.JobHistoryErrorJob, Arg.Any<Exception>());
		}
	}
}