﻿using System.IO;
using System.Linq;
using System.Data;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportSavedSearch : MetadataExportTestCaseBase
	{
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.CopyFileFromRepository = true;
			settings.IncludeNativeFilesPath = true;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			var nativeDirectories = directory.EnumerateDirectories("NATIVES", SearchOption.AllDirectories);
			var nativeFileInfos = nativeDirectories
				.SelectMany(item => item.EnumerateFiles("*", SearchOption.AllDirectories))
				.ToList();

			var expectedFileNames = documents
				.AsEnumerable()
				.Select(row => row.Field<string>("File Name"))
				.ToList();

			Assert.AreEqual(expectedFileNames.Count, nativeFileInfos.Count, "Exported Native File count is not like expected!");
			Assert.That(nativeFileInfos.Any(item => expectedFileNames.Exists(name => name == item.Name)));

			var datFileInfo = GetFileInfo(directory);
			Assert.That(datFileInfo?.Name, Is.EqualTo($"{ExportSettings.ExportedObjName}_export.{MetadataFormat}"));
			Assert.That(datFileInfo?.Length, Is.GreaterThan(0));
		}

		public override string MetadataFormat => "dat";
	}
}
