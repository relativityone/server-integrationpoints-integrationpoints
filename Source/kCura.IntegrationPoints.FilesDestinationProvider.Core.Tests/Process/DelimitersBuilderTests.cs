using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.WinEDDS;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
    [TestFixture, Category("Unit")]
    public class DelimitersBuilderTests : TestBase
    {
        private DelimitersBuilder _delimitersBuilder;
        private ExportFile _exportFile;
        private ExportSettings _exportSettings;

        [SetUp]
        public override void SetUp()
        {
            _exportSettings = DefaultExportSettingsFactory.Create();
            _exportFile = new ExportFile(1);
            _delimitersBuilder = new DelimitersBuilder();
        }

        [Test]
        public void ItShouldSetDelimitersForCustomFileFormat()
        {
            const char columnDelimiter = 'a';
            const char quoteDelimiter = 'b';
            const char newlineDelimiter = 'c';
            const char multiValueDelimiter = 'd';
            const char nestedValueDelimiter = 'e';

            _exportSettings.OutputDataFileFormat = ExportSettings.DataFileFormat.Custom;

            _exportSettings.ColumnSeparator = columnDelimiter;
            _exportSettings.QuoteSeparator = quoteDelimiter;
            _exportSettings.NewlineSeparator = newlineDelimiter;
            _exportSettings.MultiValueSeparator = multiValueDelimiter;
            _exportSettings.NestedValueSeparator = nestedValueDelimiter;

            _delimitersBuilder.SetDelimiters(_exportFile, _exportSettings);

            Assert.AreEqual(columnDelimiter, _exportFile.RecordDelimiter);
            Assert.AreEqual(quoteDelimiter, _exportFile.QuoteDelimiter);
            Assert.AreEqual(newlineDelimiter, _exportFile.NewlineDelimiter);
            Assert.AreEqual(multiValueDelimiter, _exportFile.MultiRecordDelimiter);
            Assert.AreEqual(nestedValueDelimiter, _exportFile.NestedValueDelimiter);
        }

        [Test]
        public void ItShouldSetDefaultDelimitersForConcordanceFileFormat()
        {
            const char defaultColumnDelimiter = '\x0014';
            const char defaultQuoteDelimiter = 'þ';
            const char defaultNewlineDelimiter = '®';
            const char defaultMultiRecordDelimiter = ';';
            const char defaultNestedValueDelimiter = '\\';

            _exportSettings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;

            _exportSettings.ColumnSeparator = 'q';
            _exportSettings.QuoteSeparator = 'w';
            _exportSettings.NewlineSeparator = 'e';
            _exportSettings.MultiValueSeparator = 'r';
            _exportSettings.NestedValueSeparator = 't';

            _delimitersBuilder.SetDelimiters(_exportFile, _exportSettings);

            Assert.AreEqual(defaultColumnDelimiter, _exportFile.RecordDelimiter);
            Assert.AreEqual(defaultQuoteDelimiter, _exportFile.QuoteDelimiter);
            Assert.AreEqual(defaultNewlineDelimiter, _exportFile.NewlineDelimiter);
            Assert.AreEqual(defaultMultiRecordDelimiter, _exportFile.MultiRecordDelimiter);
            Assert.AreEqual(defaultNestedValueDelimiter, _exportFile.NestedValueDelimiter);
        }

        [Test]
        public void ItShouldSetDefaultDelimitersForCsvFileFormat()
        {
            const char defaultColumnDelimiter = ',';
            const char defaultQuoteDelimiter = '"';
            const char defaultNewlineDelimiter = '\x000A';
            const char defaultMultiRecordDelimiter = ';';
            const char defaultNestedValueDelimiter = '\\';

            _exportSettings.OutputDataFileFormat = ExportSettings.DataFileFormat.CSV;

            _exportSettings.ColumnSeparator = 'q';
            _exportSettings.QuoteSeparator = 'w';
            _exportSettings.NewlineSeparator = 'e';
            _exportSettings.MultiValueSeparator = 'r';
            _exportSettings.NestedValueSeparator = 't';

            _delimitersBuilder.SetDelimiters(_exportFile, _exportSettings);

            Assert.AreEqual(defaultColumnDelimiter, _exportFile.RecordDelimiter);
            Assert.AreEqual(defaultQuoteDelimiter, _exportFile.QuoteDelimiter);
            Assert.AreEqual(defaultNewlineDelimiter, _exportFile.NewlineDelimiter);
            Assert.AreEqual(defaultMultiRecordDelimiter, _exportFile.MultiRecordDelimiter);
            Assert.AreEqual(defaultNestedValueDelimiter, _exportFile.NestedValueDelimiter);
        }
    }
}