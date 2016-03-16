using System.Collections.Generic;
using kCura.IntegrationPoint.FilesDestinationProvider.Core.Metadata;
using kCura.IntegrationPoint.FilesDestinationProvider.Core.Metadata.Formatters;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Metadata.Formatters
{
	public class ConcordanceFormatterTest
	{
		private readonly ConcordanceFormatter _subjectUnderTest = new ConcordanceFormatter();
		private MetadataSettings _settings;
		

		[SetUp]
		public void Init()
		{
			_settings = new MetadataSettings();
		}

		[Test]
		public void ItShouldFormatHeader()
		{
			//Arange

			const string col1Name = "Column1";
			const string col2Name = "Column2";

			
			_settings.HeaderMetadata = new List<HeaderMetadata> {new HeaderMetadata(col1Name), new HeaderMetadata(col2Name)};

			_settings.QuoteDelimiter = '|';

			//Act

			var headerLine = _subjectUnderTest.GetHeaders(_settings);

			//Assert
			Assert.That(headerLine, Is.EqualTo("|Column1||Column2|"));
		}
	}
}
