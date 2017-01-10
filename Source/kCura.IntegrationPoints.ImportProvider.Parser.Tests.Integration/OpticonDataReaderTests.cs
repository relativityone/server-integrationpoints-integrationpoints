using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using NUnit.Framework;
using kCura.IntegrationPoint.Tests.Core;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests.Integration
{
	public class OpticonDataReaderTests : TestBase
	{
		private const string _OPTICON_EXTENSION = ".opt";
		private const string _RESULTS_EXTENSION = ".xml";
		private const string _OPTICON_RESOURCE_STRING_ROOT = "kCura.IntegrationPoints.ImportProvider.Parser.Tests.Integration.OpticonResources.";
		private const string _RESULTS_RESOURCE_STRING_ROOT = "kCura.IntegrationPoints.ImportProvider.Parser.Tests.Integration.ResultsResources.";

		private string _tempDirPath;
		public override void SetUp()
		{
			string tempDirName = Guid.NewGuid().ToString().Trim(new char[] { '{', '}' });
			_tempDirPath = Path.Combine(Path.GetTempPath(), tempDirName);
			Directory.CreateDirectory(_tempDirPath);
		}

		[TearDown]
		public void TestTearDown()
		{
			Directory.Delete(_tempDirPath, true);
		}

		[Test]
		public void CanHandleEmptyFile()
		{
			string opticonTempLocation = Path.GetTempFileName();
			ImageLoadFile config = new ImageLoadFile { FileName = opticonTempLocation };
			using (OpticonDataReader reader = new OpticonDataReader(config))
			{
				reader.Init();
				Assert.IsFalse(reader.Read());
			}
		}

		[TestCase("SINGLE_LINE")]
		[TestCase("SINGLE_IMAGE_DOCUMENTS")]
		[TestCase("MULTI_IMAGE_DOCUMENTS")]
		public void CanReadOpticonFile(string resourceName)
		{
			ResultsResource expectedResults = ExpectedResults(resourceName);
			ImageLoadFile config = new ImageLoadFile { FileName = CopyEmbeddedOpticonToTemp(resourceName) };
			using (OpticonDataReader reader = new OpticonDataReader(config))
			{
				reader.Init();
				int resultsIdx = 0;
				while (reader.Read())
				{
					Assert.AreEqual(expectedResults.DataReaderLines[resultsIdx++], reader.GetString(0));
				}
				Assert.AreEqual(expectedResults.DataReaderLines.Count, resultsIdx);
			}
		}

		private string CopyEmbeddedOpticonToTemp(string resourceName)
		{
			string rv = Path.Combine(_tempDirPath, resourceName +  _OPTICON_EXTENSION);
			using (StreamReader reader = OpticonResourceStreamReader(resourceName))
			using (StreamWriter writer = new StreamWriter(rv))
			{
				writer.Write(reader.ReadToEnd());
			}
			return rv;
		}

		private ResultsResource ExpectedResults(string resourceName)
		{
			return (ResultsResource)(new XmlSerializer(typeof(ResultsResource))).Deserialize(ResultsResourceStreamReader(resourceName));
		}

		private StreamReader OpticonResourceStreamReader(string resourceName)
		{
			return new StreamReader(EmbeddedResourceStream(OpticonResourcePath(resourceName)), Encoding.UTF8);
		}
		private StreamReader ResultsResourceStreamReader(string resourceName)
		{
			return new StreamReader(EmbeddedResourceStream(ResultsResourcePath(resourceName)), Encoding.UTF8);
		}

		private Stream EmbeddedResourceStream(string resourcePath)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
		}

		private string OpticonResourcePath(string resourceName)
		{
			return _OPTICON_RESOURCE_STRING_ROOT + resourceName + _OPTICON_EXTENSION;
		}

		private string ResultsResourcePath(string resourceName)
		{
			return _RESULTS_RESOURCE_STRING_ROOT + resourceName + _RESULTS_EXTENSION;
		}
	}
}
