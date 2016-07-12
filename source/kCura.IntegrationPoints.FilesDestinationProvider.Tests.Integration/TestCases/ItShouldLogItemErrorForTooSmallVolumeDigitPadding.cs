﻿using System.Data;
using System.IO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using kCura.WinEDDS.Exporters;
using NSubstitute;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldLogItemErrorForTooSmallVolumeDigitPadding : ExportTestCaseBase
	{
		private readonly IUserNotification _userNotification;

		public ItShouldLogItemErrorForTooSmallVolumeDigitPadding(IUserNotification userNotification)
		{
			_userNotification = userNotification;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			_userNotification.ClearReceivedCalls();

			settings.SubdirectoryDigitPadding = 2;
			settings.VolumeDigitPadding = 0;
			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			const string expectedMessageBeggining = "The selected volume padding of 0 is less than the recommended volume padding 1 for this export";
			const string expectedMessageEnd = "Continue with this selection?";
			_userNotification.Received().AlertWarningSkippable(Arg.Is<string>(x => x.StartsWith(expectedMessageBeggining) && x.EndsWith(expectedMessageEnd)));
		}
	}
}