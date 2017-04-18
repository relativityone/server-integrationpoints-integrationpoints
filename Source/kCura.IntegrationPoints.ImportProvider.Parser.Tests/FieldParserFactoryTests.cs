using System.Data;
using System.IO;

using NUnit.Framework;
using NSubstitute;

using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoint.Tests.Core;
using kCura.WinEDDS;
using kCura.WinEDDS.Api;
using NSubstitute.Core;
using NSubstitute.ExceptionExtensions;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
	public class FieldParserFactoryTests : TestBase
	{
		private IWinEddsLoadFileFactory _winEddsLoadFileFactory;
		private IWinEddsFileReaderFactory _winEddsFileReaderFactory;
		private IArtifactReader _loadFileReader;
		private LoadFile _loadFile;

		private FieldParserFactory _instance;

		[SetUp]
		public override void SetUp()
		{
			_loadFile = new LoadFile();
			_loadFileReader = Substitute.For<IArtifactReader>();

			_winEddsLoadFileFactory = Substitute.For<IWinEddsLoadFileFactory>();
			_winEddsLoadFileFactory.GetLoadFile(Arg.Any<ImportProviderSettings>()).Returns(_loadFile);

			_winEddsFileReaderFactory = Substitute.For<IWinEddsFileReaderFactory>();
			_winEddsFileReaderFactory.GetLoadFileReader(Arg.Any<LoadFile>()).Returns(_loadFileReader);

			_instance = new FieldParserFactory(_winEddsLoadFileFactory, _winEddsFileReaderFactory);
		}

		[Test]
		public void ItShouldReturnFieldParser()
		{
			//Arrange

			//Act

			//Assert
			Assert.That(_instance.GetFieldParser(null) is IFieldParser);
		}
	}
}
