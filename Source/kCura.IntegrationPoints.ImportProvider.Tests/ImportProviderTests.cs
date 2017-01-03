using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;
using NSubstitute;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.ImportProvider.Parser;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Tests
{
    [TestFixture]
    public class ImportProviderTests : TestBase
    {
        private int MAX_COLS = 100;
        private int MAX_ROWS = 20;

        private IFieldParser _fieldParser;
        private IFieldParserFactory _fieldParserFactory;
        private IDataReaderFactory _dataReaderFactory;
        private IEnumerableParserFactory _enumerableParserFactory;

        [SetUp]
        public override void SetUp()
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

            _fieldParserFactory.GetFieldParser(null).ReturnsForAnyArgs(_fieldParser);
            _fieldParser.GetFields().Returns(testData);

            ImportProvider ip = new ImportProvider(_fieldParserFactory, _dataReaderFactory, _enumerableParserFactory);
            IEnumerable<FieldEntry> ipFields = ip.GetFields(string.Empty);

            Assert.AreEqual(testData.Count, ipFields.Count());

            int tdIndex = 0;
            foreach (FieldEntry ipEntry in ipFields)
            {
                Assert.AreEqual(tdEnum.Current, ipEntry.DisplayName);
                Assert.AreEqual(tdIndex, Int32.Parse(ipEntry.FieldIdentifier));
                tdIndex++;
                tdEnum.MoveNext();
            }
        }

        [Test]
        public void ImportProviderCanGetData()
        {
            Random r = new Random();
            int colCount = r.Next(MAX_COLS);
            int rowCount = r.Next(MAX_ROWS);
            List<string> testHeaders = TestHeaders(colCount);
            List<List<string>> testData = TestData(colCount, rowCount);
            char recordDelimiter = ',';
            char quoteDelimiter = '"';

            //Subsitute config so test can use GetFields
            _fieldParserFactory.GetFieldParser(null).ReturnsForAnyArgs(_fieldParser);
            _fieldParser.GetFields().Returns(testHeaders);

            //Subsitute config so test can use GetData
            IEnumerable<string> tdJoinedRows = testData.Select(x => string.Join(recordDelimiter.ToString(), x));
            EnumerableParser tdEnumerableParser = new EnumerableParser(tdJoinedRows, recordDelimiter, quoteDelimiter);
            _enumerableParserFactory.GetEnumerableParser(null, null).ReturnsForAnyArgs(tdEnumerableParser);

            ImportProvider ip = new ImportProvider(_fieldParserFactory, _dataReaderFactory, _enumerableParserFactory);
            IEnumerable<FieldEntry> ipFields = ip.GetFields(string.Empty);

            IDataReader ipGetDataResult = ip.GetData(ipFields, tdJoinedRows, string.Empty);

            Assert.AreEqual(colCount, ipGetDataResult.FieldCount);

            int tdRow = 0;
            if (ipGetDataResult.Read())
            {
                do
                {
                    int tdCol = 0;
                    foreach (FieldEntry currentField in ipFields)
                    {
                        int ordinal = ipGetDataResult.GetOrdinal(currentField.FieldIdentifier);
                        Assert.AreEqual(testData[tdRow][tdCol], ipGetDataResult.GetString(ordinal));
                        tdCol++;
                    }
                    tdRow++;
                } while (ipGetDataResult.Read());
                Assert.AreEqual(tdRow, rowCount);
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

