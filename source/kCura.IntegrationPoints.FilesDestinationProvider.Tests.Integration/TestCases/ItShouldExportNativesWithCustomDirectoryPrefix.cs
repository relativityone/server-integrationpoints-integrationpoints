﻿using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportNativesWithCustomDirectoryPrefix : ExportTestCaseBase
	{
		private const string _PREFIX = "custom_native_prefix";

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.CopyFileFromRepository = true;
			settings.IncludeNativeFilesPath = true;
			settings.SubdirectoryNativePrefix = _PREFIX;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			var nativesRootDirectory = directory.EnumerateDirectories("NATIVES", SearchOption.AllDirectories).ToList();

			var expectedDirectories = nativesRootDirectory.SelectMany(x => x.EnumerateDirectories().Where(y => y.Name.StartsWith(_PREFIX)));

			Assert.That(expectedDirectories.Any(), $"There should be at least one folder with specified prefix {_PREFIX}");
		}
	}
}