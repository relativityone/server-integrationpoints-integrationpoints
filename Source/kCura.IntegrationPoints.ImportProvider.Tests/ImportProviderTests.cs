using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using NSubstitute;

using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.ImportProvider;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Tests
{
    [TestFixture]
    public class ImportProviderTests
    {
        private int MAX_COLS = 100;

        private IFieldParser _fieldParser;
        private IFieldParserFactory _fieldParserFactory;
        private IDataReaderFactory _dataReaderFactory;
        private IEnumerableParserFactory _enumerableParserFactory;

        [SetUp]
        public void Setup()
        {
            _fieldParser = Substitute.For<IFieldParser>();
            _fieldParserFactory = Substitute.For<IFieldParserFactory>();;
            _dataReaderFactory = Substitute.For<IDataReaderFactory>();;
            _enumerableParserFactory = Substitute.For<IEnumerableParserFactory>();
        }

        [Test]
        public void ImportProviderCanGetFields()
        {
            List<string> testData = TestHeaders((new Random()).Next(MAX_COLS));
            IEnumerator<string> tdEnum = testData.GetEnumerator();
            tdEnum.MoveNext();

            _fieldParserFactory.GetFieldParser("").ReturnsForAnyArgs(_fieldParser);
            _fieldParser.GetFields().Returns(testData);

            ImportProvider ip = new ImportProvider(_fieldParserFactory, _dataReaderFactory, _enumerableParserFactory);
            IEnumerable<FieldEntry> ipFields = ip.GetFields(string.Empty);

            Assert.AreEqual(testData.Count, ipFields.Count());

            int tdIndex = 0;
            foreach (FieldEntry ipEntry in  ipFields)
            {
                Assert.AreEqual(tdEnum.Current, ipEntry.DisplayName);
                Assert.AreEqual(tdIndex, Int32.Parse(ipEntry.FieldIdentifier));
                tdIndex++;
                tdEnum.MoveNext();
            }
        }

        private List<string> TestHeaders(int fieldCount)
        {
            return
                Enumerable
                .Range(0, fieldCount)
                .Select(x => string.Format("col-{0}", x))
                .ToList();
        }

        private List<List<string>> TestData(int fieldCount, int rowCount)
        {
            return
                Enumerable
                .Range(0, rowCount)
                .Select(row =>
                        Enumerable
                        .Range(0, fieldCount)
                        .Select(col => string.Format("r{0}c{1}", row, col))
                        .ToList())
                .ToList();
        }
    }
}

