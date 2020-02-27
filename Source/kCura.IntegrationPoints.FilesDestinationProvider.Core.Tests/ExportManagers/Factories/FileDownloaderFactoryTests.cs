using System;
using FluentAssertions;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers.Factories;
using kCura.WinEDDS;
using NUnit.Framework;
using Relativity.DataExchange.Service;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.ExportManagers.Factories
{
	[TestFixture, Category("Unit")]
	public class FileDownloaderFactoryTests
	{
		private const string _DOWNLOAD_HANDLER_URL = "http://relativity.com/download";

		[TestCase("abc", 4332, @"abc\EDDS4332")]
		[TestCase(@"abc\xyz", 0, @"abc\xyz\EDDS0")]
		public void ShouldCalculateProperDestinationFolderPath(
			string documentPath,
			int artifactID,
			string expectedDestinationFolderPath)
		{
			// arrange
			var caseInfo = new CaseInfo
			{
				DocumentPath = documentPath,
				ArtifactID = artifactID
			};

			// act
			string destinationFolderPath = FileDownloaderFactory.GetDestinationFolderPath(caseInfo);

			// assert
			destinationFolderPath.Should().Be(expectedDestinationFolderPath);
		}

		[Test]
		public void ShouldThrowArgumentExceptionWhenExportFileIsNull()
		{
			// arrange
			ExportFile exportFile = null;
			var sut = new FileDownloaderFactory();

			// act
			Action createFileDownloaderAction = () => sut.Create(exportFile);

			// act
			createFileDownloaderAction.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void ShouldThrowArgumentExceptionWhenCaseInfoIsNull()
		{
			// arrange
			var exportFile = new ExportFile(0)
			{
				CaseInfo = null
			};

			var sut = new FileDownloaderFactory();

			// act
			Action createFileDownloaderAction = () => sut.Create(exportFile);

			// act
			createFileDownloaderAction.Should().Throw<ArgumentException>();
		}

		[Test]
		public void ShouldThrowArgumentExceptionWhenDocumentPathIsNull()
		{
			// arrange
			var exportFile = new ExportFile(0)
			{
				CaseInfo = new CaseInfo()
				{
					DownloadHandlerURL = _DOWNLOAD_HANDLER_URL,
					ArtifactID = 0
				}
			};

			var sut = new FileDownloaderFactory();

			// act
			Action createFileDownloaderAction = () => sut.Create(exportFile);

			// act
			createFileDownloaderAction.Should().Throw<ArgumentException>();
		}
	}
}
