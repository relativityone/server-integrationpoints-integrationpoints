using System;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed partial class SourceWorkspaceDataReaderTests : IDisposable
	{
		private SourceWorkspaceDataReader _instance;
		private DocumentTransferServicesMocker _documentTransferServicesMocker;
		private ConfigurationStub _configuration;

		[SetUp]
		public void SetUp()
		{
			_configuration = new ConfigurationStub
			{
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None,
				FieldMappings = StandardFieldMappings
			};
			
			_documentTransferServicesMocker = new DocumentTransferServicesMocker();

			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockReporting(containerBuilder);
			_documentTransferServicesMocker.RegisterServiceMocks(containerBuilder);
			containerBuilder.RegisterInstance(_configuration).AsImplementedInterfaces();
			IContainer container = containerBuilder.Build();

			IFieldManager fieldManager = container.Resolve<IFieldManager>();
			_documentTransferServicesMocker.SetFieldManager(fieldManager);

			_instance = new SourceWorkspaceDataReader(container.Resolve<ISourceWorkspaceDataTableBuilder>(),
				_configuration,
				container.Resolve<IRelativityExportBatcher>(),
				container.Resolve<IFieldManager>(),
				container.Resolve<IItemStatusMonitor>(),
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
			DocumentImportJob importData = MultipleBatchesImportJob;
			const int batchSize = 100;
			await _documentTransferServicesMocker.SetupServicesWithTestData(importData, batchSize).ConfigureAwait(false);

			// Act/Assert
			foreach (Document document in importData.Documents)
			{
				bool hasMoreData = _instance.Read();
				hasMoreData.Should().Be(true);

				_instance["NativeFileFilename"].ConvertTo<string>().Should().Be(document.NativeFile.Filename);
				_instance["NativeFileLocation"].ConvertTo<string>().Should().Be(document.NativeFile.Location);
				_instance["NativeFileSize"].ConvertTo<long>().Should().Be(document.NativeFile.Size);
				_instance["Relativity Source Case"].ConvertTo<string>().Should().Be(_configuration.SourceWorkspaceTagName);
				_instance["Relativity Source Job"].ConvertTo<string>().Should().Be(_configuration.SourceJobTagName);

				foreach (FieldValue fieldValue in document.FieldValues)
				{
					Type valueType = fieldValue.Value.GetType();
					_instance[fieldValue.Field].ConvertTo(valueType).Should().Be(fieldValue.Value);
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
	}
}
