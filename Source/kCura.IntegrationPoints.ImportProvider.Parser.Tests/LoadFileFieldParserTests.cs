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

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
	public class LoadFileFieldParserTests : TestBase
	{
		private readonly string[] _HEADERS = new string[] { "Control Number", "Native File Path", "Extracted Text" };

		LoadFileFieldParser _instance;
		IArtifactReader _loadFileReader;
		LoadFile _loadFile;

		[SetUp]
		public override void SetUp()
		{
			_loadFile = new LoadFile();

			_loadFileReader = Substitute.For<IArtifactReader>();
			_loadFileReader.GetColumnNames(Arg.Any<object>()).Returns(_HEADERS);
		}

		[Test]
		public void ItShouldHandleEmptyFiles()
		{
			//Arrange
			_loadFileReader.GetColumnNames(Arg.Any<object>()).Returns(new string[0]);
			_instance = new LoadFileFieldParser(_loadFile, _loadFileReader);

			//Act

			//Assert
			Assert.AreEqual(0, _instance.GetFields().Count);
		}

		[Test]
		public void ItShouldReturnFieldsFromLoadFileReader()
		{
			//Arrange
			_instance = new LoadFileFieldParser(_loadFile, _loadFileReader);

			//Act

			//Assert
			Assert.AreEqual(_HEADERS.Length, _instance.GetFields().Count);

			for (int i = 0; i < _HEADERS.Length; i++)
			{
				Assert.AreEqual(_HEADERS[i], _instance.GetFields()[i]);
			}
		}
	}
}
