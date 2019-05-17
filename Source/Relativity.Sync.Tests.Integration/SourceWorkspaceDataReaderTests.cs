using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed partial class SourceWorkspaceDataReaderTests : IDisposable
	{
		private SourceWorkspaceDataReader _instance;
		private DocumentTransferServicesMocker _documentTransferServicesMocker;
		private Mock<IDocumentFieldRepository> _documentFieldRepository;
		private ConfigurationStub _configuration;

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

			_configuration = new ConfigurationStub();
			
			_documentFieldRepository = new Mock<IDocumentFieldRepository>();

			IFieldManager fieldManager = new FieldManager(_configuration, _documentFieldRepository.Object, Enumerable.Empty<ISpecialFieldBuilder>());
			_documentTransferServicesMocker = new DocumentTransferServicesMocker(fieldManager);

			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockReporting(containerBuilder);
			_documentTransferServicesMocker.RegisterMocks(containerBuilder);
			IContainer container = containerBuilder.Build();

			_instance = new SourceWorkspaceDataReader(container.Resolve<ISourceWorkspaceDataTableBuilder>(), _configuration,
				container.Resolve<IRelativityExportBatcher>(),
				Mock.Of<ISyncLog>());
		}

		public void Dispose()
		{
			_instance?.Dispose();
		}

		[Test]
		public async Task ItShouldReadAcrossMultipleBatches()
		{
			// Arrange
			Document[] testData = MultipleBatchesTestData;
			const int batchSize = 100;
			await _documentTransferServicesMocker.SetupServicesWithTestData(testData, batchSize).ConfigureAwait(false);

			// Act/Assert
			foreach (Document document in testData)
			{
				bool hasMoreData = _instance.Read();
				hasMoreData.Should().Be(true);

				_instance["NativeFileFilename"].ConvertTo<string>().Should().Be(document.NativeFile.Filename);
				_instance["NativeFileLocation"].ConvertTo<string>().Should().Be(document.NativeFile.Location);
				_instance["NativeFileSize"].ConvertTo<long>().Should().Be(document.NativeFile.Size);
				_instance["FolderPath"].ConvertTo<string>().Should().Be("");
				_instance["Relativity Source Case"].ConvertTo<string>().Should().Be(_configuration.SourceWorkspaceTagName);
				_instance["Relativity Source Job"].ConvertTo<string>().Should().Be(_configuration.SourceJobTagName);

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
			IList<FieldMap> fieldMap = _configuration.FieldMappings;
			FieldEntry potentialDestinationField = fieldMap.FirstOrDefault(x => x.SourceField.DisplayName == sourceFieldName)?.DestinationField;
			if (potentialDestinationField == null)
			{
				return sourceFieldName;
			}
			return potentialDestinationField.DisplayName;
		}
	}
}
