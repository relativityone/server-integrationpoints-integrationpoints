using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using NSubstitute;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldLogJobErrorForInvalidFilesharePathFormat : IInvalidFileshareExportTestCase
	{
		private readonly IJobHistoryErrorService _jobHistoryErrorService;

		public ItShouldLogJobErrorForInvalidFilesharePathFormat(IJobHistoryErrorService jobHistoryErrorService)
		{
			_jobHistoryErrorService = jobHistoryErrorService;
		}

		public ExportSettings Prepare(ExportSettings settings)
		{
			_jobHistoryErrorService.ClearReceivedCalls();

			settings.ExportFilesLocation = @"AB:\InvalidPath";
			return settings;
		}

		public void Verify()
		{
			const string expectedMessage = "The given path's format is not supported.";
			_jobHistoryErrorService.Received().AddError(ErrorTypeChoices.JobHistoryErrorJob, Arg.Is<Exception>(x => x.Message == expectedMessage));
		}
	}
}