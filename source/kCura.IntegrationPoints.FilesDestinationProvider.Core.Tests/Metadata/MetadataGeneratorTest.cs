using System.Collections.Generic;
using kCura.IntegrationPoint.FilesDestinationProvider.Core.Files;
using kCura.IntegrationPoint.FilesDestinationProvider.Core.Metadata;
using kCura.IntegrationPoint.FilesDestinationProvider.Core.Metadata.Formatters;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Metadata
{
	public class MetadataGeneratorTest
	{
		private MetadataGenerator _generator;

		private IFileRepository _fileRepository;
		private IMetadataFormatter _metadataFormatter;
		private MetadataSettings _settings;

		const string Col1Name = "Col1Name";
		const string Col2Name = "Col2Name";

		[TestFixtureSetUp]
		public void InitTests()
		{
			_settings = new MetadataSettings();

			_settings.HeaderMetadata = new List<HeaderMetadata>()
			{
				new HeaderMetadata(Col1Name),
				new HeaderMetadata(Col2Name)
			};

			_settings.FilePath = "c:\\data.dat";

			_settings.QuoteDelimiter = '|';
		}

		[SetUp]
		public void SetUp()
		{
			_fileRepository = Substitute.For<IFileRepository>();
			_metadataFormatter = Substitute.For<IMetadataFormatter>();

			_generator = new MetadataGenerator(_fileRepository, _metadataFormatter);
		}

		[Test]
		public void ItShouldCreateMetadata()
		{
			//Arrange

			//Act
			_generator.Create(_settings);

			//Assert
			_fileRepository.Received(1).Create(_settings.FilePath);
		}

		[Test]
		public void ItShouldCreateMetadataHeader()
		{
			//Arrange
			string expectedHeader = "SomeHeader";
			_metadataFormatter.GetHeaders(_settings).Returns(expectedHeader);

			//Act
			_generator.WriteHerader(_settings);

			//Assert
			_fileRepository.Received(1).Write(expectedHeader);
		}
	}
}
