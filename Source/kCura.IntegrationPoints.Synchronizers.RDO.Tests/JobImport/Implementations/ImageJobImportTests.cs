using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.JobImport.Implementations
{
	[TestFixture]
	public class ImageJobImportTests : TestBase
	{
		private IDataTransferContext _context;
		private IExtendedImportAPI _importApi;
		private IImportSettingsBaseBuilder<ImageSettings> _builder;

		private ImageJobImport _instance;
		private ImportSettings _importSettings;

		[SetUp]
		public override void SetUp()
		{
			_importSettings = Substitute.For<ImportSettings>();
			_importApi = Substitute.For<IExtendedImportAPI>();
			_builder = Substitute.For<IImportSettingsBaseBuilder<ImageSettings>>();
			_context = Substitute.For<IDataTransferContext>();

			_instance = new ImageJobImport(_importSettings, _importApi, _builder, _context);
		}

		[Test]
		public void ItShouldCreateJob()
		{
			//Arrange
			var expected = new ImageImportBulkArtifactJob();
			_importApi.NewImageImportJob().Returns(expected);

			//Act
			ImageImportBulkArtifactJob actual = _instance.CreateJob();

			//Assert
			Assert.AreEqual(expected, actual);
			_importApi.Received().NewImageImportJob();
		}
	}
}
