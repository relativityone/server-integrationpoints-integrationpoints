using System.IO;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using kCura.WinEDDS.Exporters;
using NSubstitute;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldLogItemErrorForTooSmallSubdirectoryDigitPadding : ExportTestCaseBase
	{
		private readonly IUserNotification _userNotification;

		public ItShouldLogItemErrorForTooSmallSubdirectoryDigitPadding(IUserNotification userNotification)
		{
			_userNotification = userNotification;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			_userNotification.ClearReceivedCalls();

			settings.SubdirectoryDigitPadding = 0;
			settings.VolumeDigitPadding = 2;
			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			const string expectedMessageBeggining = "The selected subdirectory padding of 0 is less than the recommended subdirectory padding 1 for this export";
			const string expectedMessageEnd = "Continue with this selection?";
			_userNotification.Received().AlertWarningSkippable(Arg.Is<string>(x => x.StartsWith(expectedMessageBeggining) && x.EndsWith(expectedMessageEnd)));
		}
	}
}