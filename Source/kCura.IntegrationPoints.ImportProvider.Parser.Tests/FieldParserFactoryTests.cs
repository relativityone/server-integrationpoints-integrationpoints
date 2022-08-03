using NUnit.Framework;
using NSubstitute;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoint.Tests.Core;
using kCura.WinEDDS;
using kCura.WinEDDS.Api;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
    [TestFixture, Category("Unit")]
    public class FieldParserFactoryTests : TestBase
    {
        private FieldParserFactory _instance;

        [SetUp]
        public override void SetUp()
        {
            LoadFile loadFile = new LoadFile();
            IArtifactReader loadFileReader = Substitute.For<IArtifactReader>();

            IWinEddsLoadFileFactory winEddsLoadFileFactory = Substitute.For<IWinEddsLoadFileFactory>();
            winEddsLoadFileFactory.GetLoadFile(Arg.Any<ImportProviderSettings>()).Returns(loadFile);

            IWinEddsFileReaderFactory winEddsFileReaderFactory = Substitute.For<IWinEddsFileReaderFactory>();
            winEddsFileReaderFactory.GetLoadFileReader(Arg.Any<LoadFile>()).Returns(loadFileReader);

            _instance = new FieldParserFactory(winEddsLoadFileFactory, winEddsFileReaderFactory);
        }

        [Test]
        public void ItShouldReturnFieldParser()
        {
            //Assert
            Assert.That(_instance.GetFieldParser(null) is IFieldParser);
        }
    }
}
