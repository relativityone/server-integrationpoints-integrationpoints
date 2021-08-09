using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Castle.Core.Internal;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Transfer;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal sealed class SourceWorkspaceDataReaderTests : SystemTest
	{
		[IdentifiedTest("789d730f-1d5a-403e-83c8-b0f7bfae8a1a")]
		public async Task Read_ShouldPassGoldFlow_WhenPushingNatives()
		{
			Dataset dataset = Dataset.NativesAndExtractedText;
			const string folderInfoFieldName = "Document Folder Path";
			const int controlNumberFieldId = 1003667;
			const int extractedTextFieldId = 1003668;
			const int totalItemsCount = 10;

			long extractedTextSizeThreshold = await QueryForExtractedTextSizeThresholdAsync().ConfigureAwait(false);

			string sourceWorkspaceName = $"{Guid.NewGuid()}";
			string jobHistoryName = $"JobHistory.{Guid.NewGuid()}";

			var fieldMap = new List<FieldMap>
			{
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = "Control Number",
						FieldIdentifier = controlNumberFieldId,
						IsIdentifier = true
					},
					DestinationField = new FieldEntry()
					{
						DisplayName = "Control Number",
						FieldIdentifier = controlNumberFieldId,
						IsIdentifier = true
					}
				},
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = "Extracted Text",
						FieldIdentifier = extractedTextFieldId
					},
					DestinationField = new FieldEntry()
					{
						DisplayName = "Extracted Text",
						FieldIdentifier = extractedTextFieldId
					}
				},
			};

			// Prepare environment
			int sourceWorkspaceArtifactId = await CreateWorkspaceAsync(sourceWorkspaceName).ConfigureAwait(false);
			int allDocumentsSavedSearchArtifactId =
				await Rdos.GetSavedSearchInstanceAsync(ServiceFactory, sourceWorkspaceArtifactId).ConfigureAwait(false);
			int jobHistoryArtifactId = await Rdos
				.CreateJobHistoryInstanceAsync(ServiceFactory, sourceWorkspaceArtifactId, jobHistoryName)
				.ConfigureAwait(false);

			// Create configuration
			ConfigurationStub configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				JobHistoryArtifactId = jobHistoryArtifactId,
				DataSourceArtifactId = allDocumentsSavedSearchArtifactId,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.ReadFromField,
				ImportNativeFileCopyMode = ImportNativeFileCopyMode.CopyFiles,
				FolderPathSourceFieldName = folderInfoFieldName
			};
			configuration.SetFieldMappings(fieldMap);

			configuration.SyncConfigurationArtifactId = await Rdos
				.CreateSyncConfigurationRdoAsync(sourceWorkspaceArtifactId, configuration)
				.ConfigureAwait(false);

			// Import documents
			var importHelper = new ImportHelper(ServiceFactory);
			ImportDataTableWrapper dataTableWrapper =
				DataTableFactory.CreateImportDataTable(dataset, extractedText: true, natives: true);
			ImportJobErrors importJobErrors = await importHelper
				.ImportDataAsync(sourceWorkspaceArtifactId, dataTableWrapper).ConfigureAwait(false);
			Assert.IsTrue(importJobErrors.Success,
				$"IAPI errors: {string.Join(global::System.Environment.NewLine, importJobErrors.Errors)}");

			// Initialize container
			IContainer container = ContainerHelper.Create(configuration);

			// Create snapshot
			IExecutor<IDataSourceSnapshotConfiguration> executor =
				container.Resolve<IExecutor<IDataSourceSnapshotConfiguration>>();
			ExecutionResult result =
				await executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);
			result.Status.Should().Be(ExecutionStatus.Completed);

			// Create batch and SourceWorkspaceDataReader
			IBatchRepository batchRepository = container.Resolve<IBatchRepository>();
			IBatch batch = await batchRepository
				.CreateAsync(sourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId, totalItemsCount, 0)
				.ConfigureAwait(false);
			ISourceWorkspaceDataReader dataReader = container.Resolve<ISourceWorkspaceDataReaderFactory>()
				.CreateNativeSourceWorkspaceDataReader(batch, CancellationToken.None);

			// Test SourceWorkspaceDataReader
			const int resultsBlockSize = 100;
			object[] tmpTable = new object[resultsBlockSize];
			ISyncLog logger = new ConsoleLogger();

			IDataReaderRowSetValidator validator = DataReaderRowSetValidator.Create(dataTableWrapper.Data);

			while (dataReader.Read())
			{
				for (int i = 0; i < dataReader.GetValues(tmpTable); i++)
				{
					logger.LogInformation(
						$"{dataReader.GetName(i)} [{(tmpTable[i] == null ? "null" : tmpTable[i].GetType().Name)}]: {tmpTable[i]}");
				}

				object controlNumberObject = dataReader["Control Number"];
				controlNumberObject.Should().BeOfType<string>();
				string controlNumber = (string) controlNumberObject;

				Action<string, object, object> extractedTextValidator =
					(cn, extractedTextFilePathObject, actualExtractedTextObject) =>
						ValidateExtractedText(cn, extractedTextFilePathObject, actualExtractedTextObject,
							extractedTextSizeThreshold);

				validator.ValidateAndRegisterRead(
					controlNumber,
					new FieldVerifyData
					{
						ColumnName = ImportDataTableWrapper.FileName, ActualValue = dataReader["NativeFileFilename"],
						Validator = ValidateNativeFileName
					},
					new FieldVerifyData
					{
						ColumnName = ImportDataTableWrapper.NativeFilePath, ActualValue = dataReader["NativeFileSize"],
						Validator = ValidateNativeFileSize
					},
					new FieldVerifyData
					{
						ColumnName = ImportDataTableWrapper.ExtractedTextFilePath,
						ActualValue = dataReader["Extracted Text"],
						Validator = extractedTextValidator
					}
				);
			}

			validator.ValidateAllRead();
		}

		[IdentifiedTest("8cc37da4-96e8-4817-902a-c42283a3de31")]
		public async Task Read_ShouldPassGoldFlow_WhenPushingImages()
		{
			Dataset dataset = Dataset.MultipleImagesPerDocument;
			const string folderInfoFieldName = "Document Folder Path";
			const int controlNumberFieldId = 1003667;
			const int totalItemsCount = 2;

			string sourceWorkspaceName = $"{Guid.NewGuid()}";
			string jobHistoryName = $"JobHistory.{Guid.NewGuid()}";

			var fieldMap = new List<FieldMap>
			{
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = "Control Number",
						FieldIdentifier = controlNumberFieldId,
						IsIdentifier = true
					},
					DestinationField = new FieldEntry()
					{
						DisplayName = "Control Number",
						FieldIdentifier = controlNumberFieldId,
						IsIdentifier = true
					}
				}
			};

			// Prepare environment
			int sourceWorkspaceArtifactId = await CreateWorkspaceAsync(sourceWorkspaceName).ConfigureAwait(false);
			int allDocumentsSavedSearchArtifactId =
				await Rdos.GetSavedSearchInstanceAsync(ServiceFactory, sourceWorkspaceArtifactId).ConfigureAwait(false);
			int jobHistoryArtifactId = await Rdos
				.CreateJobHistoryInstanceAsync(ServiceFactory, sourceWorkspaceArtifactId, jobHistoryName)
				.ConfigureAwait(false);
			
			// Create configuration
			ConfigurationStub configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				JobHistoryArtifactId = jobHistoryArtifactId,
				DataSourceArtifactId = allDocumentsSavedSearchArtifactId,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.ReadFromField,
				ImportNativeFileCopyMode = ImportNativeFileCopyMode.CopyFiles,
				FolderPathSourceFieldName = folderInfoFieldName,
				IsImageJob = true
			};
			configuration.SetFieldMappings(fieldMap);

			configuration.SyncConfigurationArtifactId = await Rdos
				.CreateSyncConfigurationRdoAsync(sourceWorkspaceArtifactId, configuration).ConfigureAwait(false);

			// Import documents
			var importHelper = new ImportHelper(ServiceFactory);
			ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImageImportDataTable(dataset);
			ImportJobErrors importJobErrors = await importHelper.ImportDataAsync(sourceWorkspaceArtifactId, dataTableWrapper).ConfigureAwait(false);
			Assert.IsTrue(importJobErrors.Success, $"IAPI errors: {string.Join(global::System.Environment.NewLine, importJobErrors.Errors)}");

			// Initialize container
			IContainer container = ContainerHelper.Create(configuration);

			// Create snapshot
			IExecutor<IDataSourceSnapshotConfiguration> executor = container.Resolve<IExecutor<IDataSourceSnapshotConfiguration>>();
			ExecutionResult result = await executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);
			result.Status.Should().Be(ExecutionStatus.Completed);

			// Create batch and SourceWorkspaceDataReader
			IBatchRepository batchRepository = container.Resolve<IBatchRepository>();
			IBatch batch = await batchRepository
				.CreateAsync(sourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId, totalItemsCount, 0)
				.ConfigureAwait(false);
			ISourceWorkspaceDataReader dataReader = container.Resolve<ISourceWorkspaceDataReaderFactory>()
				.CreateImageSourceWorkspaceDataReader(batch, CancellationToken.None);

			// Test SourceWorkspaceDataReader
			const int resultsBlockSize = 100;
			object[] tmpTable = new object[resultsBlockSize];
			ISyncLog logger = new ConsoleLogger();

			int imageFilesCount = dataset.GetFiles().Count();
			bool read;

			for (int imageFileIndex = 0; imageFileIndex < imageFilesCount; imageFileIndex++)
			{
				read = dataReader.Read();
				read.Should().BeTrue();

				for (int i = 0; i < dataReader.GetValues(tmpTable); i++)
				{
					logger.LogInformation(
						$"{dataReader.GetName(i)} [{(tmpTable[i] == null ? "null" : tmpTable[i].GetType().Name)}]: {tmpTable[i]}");
				}

				DataRow dataRow = dataTableWrapper.Data.Rows[imageFileIndex];
				object actualFileName = dataReader["ImageFileName"];
				object expectedFileName = dataRow["File Name"];
				actualFileName.Should().Be(expectedFileName);
			}

			read = dataReader.Read();
			read.Should().BeFalse();
		}

		private async Task<long> QueryForExtractedTextSizeThresholdAsync()
		{
			const string instanceSettingName = "MaximumLongTextSizeForExportInCell";
			const string instanceSettingSection = "kCura.EDDS.WebAPI";

			QueryRequest query = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.InstanceSetting },
				Condition = $"('Section' == '{instanceSettingSection}') AND ('Name' == '{instanceSettingName}')",
				Fields = new[]
				{
					new FieldRef {Name = "Value"}
				}
			};

			using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				QueryResult queryAsync = await objectManager.QueryAsync(-1, query, 0, 1).ConfigureAwait(false);
				queryAsync.ResultCount.Should().Be(1, $"{instanceSettingSection}.{instanceSettingName} should be set.");
				long threshold = long.Parse((string)queryAsync.Objects[0]["Value"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
				return threshold;
			}
		}

		private static void ValidateNativeFileName(string controlNumber, object expectedNativeFileNameObject, object actualNativeFileNameObject)
		{
			string expectedNativeFile = (string)expectedNativeFileNameObject;

			actualNativeFileNameObject.Should().BeOfType<string>("every column in the data reader apart from Long Text should be of type string");

			string actualNativeFileName = (string)actualNativeFileNameObject;
			actualNativeFileName.Should().Be(expectedNativeFile);
		}

		private static void ValidateNativeFileSize(string controlNumber, object nativeFilePathObject, object actualNativeFileSizeObject)
		{
			string nativeFilePath = (string)nativeFilePathObject;
			var nativeFile = new FileInfo(nativeFilePath);
			long expectedNativeFileSize = nativeFile.Length;

			actualNativeFileSizeObject.Should().BeOfType<string>("every column in the data reader apart from Long Text should be of type string");

			long.TryParse((string)actualNativeFileSizeObject, NumberStyles.Any, CultureInfo.InvariantCulture, out var actualNativeFileSize).Should().BeTrue("native file size should a parsable long");
			actualNativeFileSize.Should().Be(expectedNativeFileSize);
		}

		private static void ValidateExtractedText(string controlNumber, object extractedTextFilePathObject, object actualExtractedTextObject, long extractedTextSizeThreshold)
		{
			string extractedTextFilePath = (string)extractedTextFilePathObject;
			long extractedTextFileSize = File.ReadAllText(extractedTextFilePath).Length;

			if (extractedTextFileSize > extractedTextSizeThreshold)
			{
				actualExtractedTextObject.Should().BeAssignableTo<Stream>("document {0} has size {1}, which is above extracted text size threshold ({2}).",
					controlNumber, extractedTextFileSize, extractedTextSizeThreshold);

				Stream actualExtractedTextStream = (Stream)actualExtractedTextObject;
				actualExtractedTextStream.CanRead.Should().BeTrue("received stream needs to be readable.");
			}
			else
			{
				actualExtractedTextObject.Should().BeOfType<string>("document {0} has size {1}, which is below extracted text size threshold ({2}).",
					controlNumber, extractedTextFileSize, extractedTextSizeThreshold);

				string actualExtractedTextAsString = (string)actualExtractedTextObject;
				actualExtractedTextAsString.IsNullOrEmpty().Should().BeFalse("extracted text should contain some content.");
			}
		}

		private async Task<int> CreateWorkspaceAsync(string workspaceName)
		{
			WorkspaceRef workspace = await Environment
				.CreateWorkspaceWithFieldsAsync(name: workspaceName)
				.ConfigureAwait(false);

			return workspace.ArtifactID;
		}
	}
}
