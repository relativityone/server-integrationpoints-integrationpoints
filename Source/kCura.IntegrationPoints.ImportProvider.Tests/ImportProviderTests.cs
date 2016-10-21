using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using NSubstitute;

using kCura.IntegrationPoints.ImportProvider;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Tests
{
    [TestFixture]
    public class ImportProviderTests
    {
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
        public void ImportProviderConstructorRuns()
        {
            ImportProvider p = new ImportProvider(_fieldParserFactory, _dataReaderFactory, _enumerableParserFactory);
        }
    }
}
