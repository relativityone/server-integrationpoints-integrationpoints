using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using Moq.Language;
using NUnit.Framework;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed partial class SourceWorkspaceDataReaderTests : IDisposable
	{
		private SourceDataReaderConfiguration _configuration;
		private SourceWorkspaceDataReader _instance;
		private DocumentTransferServicesMocker _documentTransferServicesMocker;

		[SetUp]
		public void SetUp()
		{
			List<FieldMap> fieldMap = new List<FieldMap>
			{
				new FieldMap
				{
					FieldMapType = FieldMapType.None,
					DestinationField = new FieldEntry { DisplayName = "Control Number 2" },
					SourceField = new FieldEntry { DisplayName = "Control Number" }
				}
			};
			_configuration = new SourceDataReaderConfiguration
			{
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None,
				MetadataMapping = new MetadataMapping(DestinationFolderStructureBehavior.None, 0, fieldMap),
				RunId = Guid.NewGuid(),
				SourceJobId = 0,
				SourceWorkspaceId = 0,
				SyncConfigurationId = 0
			};

			_documentTransferServicesMocker = new DocumentTransferServicesMocker(_configuration.MetadataMapping);

			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockReporting(containerBuilder);
			_documentTransferServicesMocker.RegisterMocks(containerBuilder);
			IContainer container = containerBuilder.Build();

			_instance = new SourceWorkspaceDataReader(_configuration,
				container.Resolve<ISourceWorkspaceDataTableBuilderFactory>(),
				container.Resolve<IRelativityExportBatcher>(),
				Mock.Of<ISyncLog>());
		}

		public void Dispose()
		{
			_instance?.Dispose();
		}

		[Test]
		public void ItShouldReadAcrossMultipleBatches()
		{
			// Arrange
			Document[] testData = MultipleBatchesTestData;
			const int batchSize = 100;
			_documentTransferServicesMocker.SetupServicesWithTestData(testData, batchSize);

			// Act/Assert
			foreach (Document document in testData)
			{
				bool hasMoreData = _instance.Read();
				hasMoreData.Should().Be(true);

				_instance["NativeFileFilename"].ConvertTo<string>().Should().Be(document.NativeFile.Filename);
				_instance["NativeFileLocation"].ConvertTo<string>().Should().Be(document.NativeFile.Location);
				_instance["NativeFileSize"].ConvertTo<long>().Should().Be(document.NativeFile.Size);
				_instance["FolderPath"].ConvertTo<string>().Should().Be("");
				_instance["Relativity Source Case"].ConvertTo<int>().Should().Be(_configuration.SourceWorkspaceId);
				_instance["Relativity Source Job"].ConvertTo<int>().Should().Be(_configuration.SourceJobId);

				foreach (FieldValue fieldValue in document.Values)
				{
					string destinationFieldName = GetDestinationFieldName(fieldValue.Field);
					Type valueType = fieldValue.Value.GetType();
					_instance[destinationFieldName].ConvertTo(valueType).Should().Be(fieldValue.Value);
				}
			}

			bool hasExtraData = _instance.Read();
			hasExtraData.Should().Be(false);
		}

		public void ItShouldThrowProperExceptionWhenExportFails()
		{

		}

		public void ItShouldThrowProperExceptionWhenNativeFileQueryFails()
		{

		}

		public void ItShouldThrowProperExceptionWhenFolderPathQueryFails()
		{

		}

		public void ItShouldReadDestinationTags()
		{

		}

		public void ItShouldHandleDestinationFolderStructureBehavior()
		{

		}

		private string GetDestinationFieldName(string sourceFieldName)
		{
			IReadOnlyList<FieldMap> fieldMap = _configuration.MetadataMapping.FieldMappings;
			FieldEntry potentialDestinationField = fieldMap.FirstOrDefault(x => x.SourceField.DisplayName == sourceFieldName)?.DestinationField;
			if (potentialDestinationField == null)
			{
				return sourceFieldName;
			}
			return potentialDestinationField.DisplayName;
		}
	}
}
