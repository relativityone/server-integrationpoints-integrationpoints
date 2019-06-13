using System;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed partial class SourceWorkspaceDataReaderTests : IDisposable
	{
		private ContainerBuilder _containerBuilder;
		private IContainer _container;
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

			_containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockReporting(_containerBuilder);
			_documentTransferServicesMocker.RegisterServiceMocks(_containerBuilder);
			_containerBuilder.RegisterInstance(_configuration).AsImplementedInterfaces();
			_container = _containerBuilder.Build();

			IFieldManager fieldManager = _container.Resolve<IFieldManager>();
			_documentTransferServicesMocker.SetFieldManager(fieldManager);
		}

		public void Dispose()
		{
			_instance?.Dispose();
		}

		[Test]
		public async Task ItShouldReadSingleBatch()
		{
			// Arrange
			const int batchSize = 100;
			Mock<IBatch> batch = new Mock<IBatch>();
			batch.SetupGet(x => x.TotalItemsCount).Returns(batchSize);
			IRelativityExportBatcher batcher = _container.Resolve<IRelativityExportBatcherFactory>().CreateRelativityExportBatcher(batch.Object);
			_instance = new SourceWorkspaceDataReader(_container.Resolve<IBatchDataReaderBuilder>(),
				_configuration,
				batcher,
				_container.Resolve<IFieldManager>(),
				_container.Resolve<IItemStatusMonitor>(),
				Mock.Of<ISyncLog>());

			DocumentImportJob importData = SingleBatchImportJob;
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
	}
}
