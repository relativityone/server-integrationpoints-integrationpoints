﻿using System;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Integration.Helpers;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal class SourceWorkspaceDataReaderNativesTests : SourceWorkspaceDataReaderTestsBase
	{
		[TestCase(ImportNativeFileCopyMode.CopyFiles)]
		[TestCase(ImportNativeFileCopyMode.DoNotImportNativeFiles)]
		public async Task Read_ShouldReadMultipleBlocksAndConstructColumns(ImportNativeFileCopyMode importNativeCopyMode)
		{
			// Arrange 
			const int batchSize = 500;
			const int blockSize = 300;
			SetUp(batchSize, importNativeCopyMode);

			DocumentImportJob importData;

			if (importNativeCopyMode == ImportNativeFileCopyMode.DoNotImportNativeFiles)
            {
				importData = CreateDefaultDocumentImportJob(batchSize, CreateDocumentForNativesTransfer, DefaultIdentifierWithoutSpecialFields);
			}
            else
            {
				importData = CreateDefaultDocumentImportJob(batchSize, CreateDocumentForNativesTransfer, DefaultIdentifierWithSpecialFields);
			}

			
			_configuration.SetFieldMappings(importData.FieldMappings);
			await _documentTransferServicesMocker.SetupServicesWithNativesTestDataAsync(importData, blockSize).ConfigureAwait(false);

			// Act/Assert 
			foreach (Document document in importData.Documents)
			{
				bool hasMoreData = _instance.Read();
				hasMoreData.Should().Be(true);
				if(_configuration.ImportNativeFileCopyMode != ImportNativeFileCopyMode.DoNotImportNativeFiles)
                {
					_instance["NativeFileFilename"].ConvertTo<string>().Should().Be(document.NativeFile.Filename);
					_instance["NativeFileLocation"].ConvertTo<string>().Should().Be(document.NativeFile.Location);
					_instance["NativeFileSize"].ConvertTo<long>().Should().Be(document.NativeFile.Size);
                }

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

		protected override IBatchDataReaderBuilder CreateBatchDataReaderBuilder()
		{
			return new NativeBatchDataReaderBuilder(_container.Resolve<IFieldManager>(), _container.Resolve<IExportDataSanitizer>(), new EmptyLogger());
		}
	}
}