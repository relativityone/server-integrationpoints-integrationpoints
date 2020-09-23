﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Tests.Integration.Helpers;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal class SourceWorkspaceDataReaderImagesTests : SourceWorkspaceDataReaderTestsBase
	{
		[Test]
		public void Read_ShouldReadMultipleBlocksAndConstructColumns()
		{
			// Arrange 
			const int batchSize = 500;
			const int blockSize = 300;
			SetUp(batchSize);

			DocumentImportJob importData = CreateDefaultDocumentImportJob(batchSize,
				(artifactId, values) => CreateDocumentForImagesTransfer(artifactId, values, 1),
				DefaultImageFieldConfiguration);

			_configuration.SetFieldMappings(importData.FieldMappings);
			_documentTransferServicesMocker.SetupServicesWithImagesTestDataAsync(importData, blockSize);

			// Act/Assert 
			foreach (Document document in importData.Documents)
			{
				bool hasMoreData = _instance.Read();
				hasMoreData.Should().Be(true);

				_instance["ImageFileName"].ConvertTo<string>().Should().Be(document.Images.First().Filename);
				_instance["ImageFileLocation"].ConvertTo<string>().Should().Be(document.Images.First().Location);

				foreach (FieldValue fieldValue in document.FieldValues)
				{
					Type expectedValueType = fieldValue.Value.GetType();
					_instance[fieldValue.Field].ConvertTo(expectedValueType).Should().Be(fieldValue.Value);
				}
			}

			bool hasExtraData = _instance.Read();

			_instance.ItemStatusMonitor.MarkReadSoFarAsSuccessful();
			foreach (Document document in importData.Documents)
			{
				_instance.ItemStatusMonitor.GetSuccessfulItemArtifactIds().Should().Contain(document.ArtifactId);
			}

			hasExtraData.Should().Be(false);
		}

		private static HashSet<FieldConfiguration> DefaultImageFieldConfiguration => new HashSet<FieldConfiguration>()
		{
			FieldConfiguration.Identifier(_DEFAULT_IDENTIFIER_COLUMN_NAME, _DEFAULT_IDENTIFIER_COLUMN_NAME)
		};

		private static Document CreateDocumentForImagesTransfer(int artifactId, HashSet<FieldConfiguration> values,
			int numberOfImages)
		{
			string workspaceFolderPath = string.Empty;
			string controlNumber = $"TST{artifactId.ToString("D4", CultureInfo.InvariantCulture)}";

			IEnumerable<ImageFile> images = Enumerable.Range(0, numberOfImages).Select(x =>
				new ImageFile(artifactId, $@"\\fake\path\img{x}.jpg", $"img{x}.jpg", 100 + artifactId));

			FieldValue[] fieldValues = values
				.Select(x => x.Type == FieldType.Identifier
					? new FieldValue(x.SourceColumnName, controlNumber)
					: new FieldValue(x.SourceColumnName, x.Value))
				.ToArray();

			return Document.Create(artifactId, images, workspaceFolderPath, fieldValues);
		}

		protected override IBatchDataReaderBuilder CreateBatchDataReaderBuilder()
		{
			return new ImageBatchDataReaderBuilder(_container.Resolve<IFieldManager>(),
				_container.Resolve<IExportDataSanitizer>());
		}
	}
}
