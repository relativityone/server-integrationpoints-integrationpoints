﻿using System.IO;
using System.Linq;
using System.Data;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportSavedSearch : IExportTestCase
	{
		private const string _METADATA_FORMAT = "dat";

		private string _expectedMetadataFilename;

		public ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportFilesLocation += $"_{nameof(ItShouldExportSavedSearch)}";

			settings.CopyFileFromRepository = true;
			settings.IncludeNativeFilesPath = true;

			_expectedMetadataFilename = $"{settings.ExportedObjName}_export.{_METADATA_FORMAT}";

			return settings;
		}

		public void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
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

			var datFileInfo = directory.EnumerateFiles($"*.{_METADATA_FORMAT}", SearchOption.TopDirectoryOnly)
				.FirstOrDefault();

			Assert.That(datFileInfo, Is.Not.Null);
			Assert.That(datFileInfo?.Name, Is.EqualTo(_expectedMetadataFilename));
			Assert.That(datFileInfo?.Length, Is.GreaterThan(0));
		}
	}
}
