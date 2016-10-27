using System;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportDataFileAsCustom : MetadataExportTestCaseBase
	{
		private int _columnCount;
		private char _columnSeparator;
		private char _multiValueSeparator;
		private char _nestedValueSeparator;
		private char _newLineSeparator;
		private char _quoteSeparator;

		public override string MetadataFormat => "txt";

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Custom;

			settings.ColumnSeparator = _columnSeparator = Convert.ToChar(200);
			settings.QuoteSeparator = _quoteSeparator = Convert.ToChar(201);
			settings.NewlineSeparator = _newLineSeparator = Convert.ToChar(202);
			settings.MultiValueSeparator = _multiValueSeparator = Convert.ToChar(203);
			settings.NestedValueSeparator = _nestedValueSeparator = Convert.ToChar(204);

			_columnCount = settings.SelViewFieldIds.Count;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var fileInfo = GetFileInfo(directory);
			var fileLines = File.ReadLines(fileInfo.FullName);

			var firstFileLine = fileLines.First();
			Assert.IsNotNull(firstFileLine);

			var columns = firstFileLine.Split(_columnSeparator);
			Assert.That(columns.Count() == _columnCount);

			foreach (var column in columns)
			{
				//we could also check other separators, but it would require additional work in set up and workspace creation
				Assert.That(column.StartsWith(_quoteSeparator.ToString()));
				Assert.That(column.EndsWith(_quoteSeparator.ToString()));
			}
		}
	}
}