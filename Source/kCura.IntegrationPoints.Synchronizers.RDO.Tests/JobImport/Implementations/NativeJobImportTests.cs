using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.Relativity.Client;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.JobImport.Implementations
{
	[TestFixture]
	public class NativeJobImportTests : TestBase
	{
		private NativeJobImport _instance;
		private ImportSettings _importSettings;
		private IExtendedImportAPI _importApi;
		private IImportSettingsBaseBuilder<Settings> _builder;
		private IAPILog _loggerMock;
		private IHelper _helperMock;

		[SetUp]
		public override void SetUp()
		{
			_importSettings = Substitute.For<ImportSettings>();
			_importApi = Substitute.For<IExtendedImportAPI>();
			_builder = Substitute.For<IImportSettingsBaseBuilder<Settings>>();
			var context = Substitute.For<IDataTransferContext>();
			_loggerMock = Substitute.For<IAPILog>();
			_helperMock = Substitute.For<IHelper>();
			_helperMock.GetLoggerFactory().GetLogger().ForContext<NativeJobImport>().Returns(_loggerMock);

			_instance = new NativeJobImport(_importSettings, _importApi, _builder, context, _helperMock);
		}

		[Test]
		public void ItShouldCreateJob_byNewNativeDocumentImportJob_withParams()
		{
			//Arrange
			_importSettings.ArtifactTypeId = (int) ArtifactType.Document;
			_importSettings.FederatedInstanceArtifactId = null;
			var expected = new ImportBulkArtifactJob();
			_importApi.NewNativeDocumentImportJob(Arg.Any<string>()).Returns(expected);

			//Act
			ImportBulkArtifactJob actual = _instance.CreateJob();

			//Assert
			Assert.AreEqual(expected, actual);
			_importApi.Received().NewNativeDocumentImportJob(Arg.Any<string>());
		}

		[Test]
		public void ItShouldCreateJob_byNewNativeDocumentImportJob()
		{
			//Arrange
			_importSettings.ArtifactTypeId = (int)ArtifactType.Document;
			_importSettings.FederatedInstanceArtifactId = 0;
			var expected = new ImportBulkArtifactJob();
			_importApi.NewNativeDocumentImportJob().Returns(expected);

			//Act
			ImportBulkArtifactJob actual = _instance.CreateJob();

			//Assert
			Assert.AreEqual(expected, actual);
			_importApi.Received().NewNativeDocumentImportJob();
		}

		[Test]
		public void ItShouldCreateJob_byNewObjectImportJob()
		{
			//Arrange
			_importSettings.ArtifactTypeId = (int)ArtifactType.Document + 10;
			var expected = new ImportBulkArtifactJob();
			_importApi.NewObjectImportJob(Arg.Any<int>()).Returns(expected);

			//Act
			ImportBulkArtifactJob actual = _instance.CreateJob();

			//Assert
			Assert.AreEqual(expected, actual);
			_importApi.Received().NewObjectImportJob(Arg.Any<int>());
		}
	}
}
