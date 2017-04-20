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
	public class LoadFileDataReaderTests : TestBase
	{
		private const string _LOAD_FILE_FULL_PATH = @"C:\LoadFileDirectory\ExampleLoadFile.csv";
		private const string _ROOTED_PATH = @"C:\Images\Example.txt";
		private const string _UN_ROOTED_PATH = @"Example.txt";
		private const string _ERROR_FILE_PATH = @"ExampleErrorFile.csv";

		private const int _IDENTIFIER_FIELD_TYPE = 2;
		private const int _NONE_FIELD_TYPE = -1;

		private const int _CONTROL_NUMBER_LOAD_FILE_INDEX = 0;
		private const int _NATIVE_PATH_LOAD_FILE_INDEX = 1;
		private const int _EXTRACTED_TEXT_PATH_LOAD_FILE_INDEX = 1;

		private const int _ROOTED_PATH_ARTIFACT_INDEX = 0;
		private const int _UN_ROOTED_PATH_ARTIFACT_INDEX = 1;

		private readonly string[] _HEADERS = new string[] { "Control Number", "Native File Path", "Extracted Text" };
		private readonly string[][] _RECORDS = new string[][]
		{
			new string [] { "REL1", _ROOTED_PATH, _ROOTED_PATH },
			new string [] { "REL2", _UN_ROOTED_PATH, _UN_ROOTED_PATH }
		};

		LoadFileDataReader _instance;
		IArtifactReader _loadFileReader;
		LoadFile _loadFile;
		ImportProviderSettings _providerSettings;
		ArtifactFieldCollection _artifact;
		private int _currentArtifactIndex;

		private void LoadArtifact(int recordIndex)
		{
			_currentArtifactIndex = recordIndex;
			_artifact = new ArtifactFieldCollection();
			for (int k = 0; k < _RECORDS[recordIndex].Length; k++)
			{
				ArtifactField cur = new ArtifactField(new DocumentField(_HEADERS[k], k,
					k == 0 ? _IDENTIFIER_FIELD_TYPE : _NONE_FIELD_TYPE, -1, -1, -1, -1, false,
					kCura.EDDS.WebAPI.DocumentManagerBase.ImportBehaviorChoice.LeaveBlankValuesUnchanged, false));
				cur.Value = _RECORDS[recordIndex][k];
				_artifact.Add(cur);
			}
			_loadFileReader.ReadArtifact().Returns(_artifact);
		}

		[SetUp]
		public override void SetUp()
		{
			_providerSettings = new ImportProviderSettings();
			_providerSettings.LoadFile = _LOAD_FILE_FULL_PATH;

			_loadFile = new LoadFile();

			_loadFileReader = Substitute.For<IArtifactReader>();
			_loadFileReader.GetColumnNames(Arg.Any<object>()).Returns(_HEADERS);
			_loadFileReader.HasMoreRecords.Returns(true);
			_loadFileReader.ManageErrorRecords(Arg.Any<string>(), Arg.Any<string>()).Returns(_ERROR_FILE_PATH);
			_loadFileReader.CountRecords().Returns(_RECORDS.Length);

			LoadArtifact(_ROOTED_PATH_ARTIFACT_INDEX);
		}

		[Test]
		public void ItShouldHandleEmptyFiles()
		{
			//Arrange
			_loadFileReader.HasMoreRecords.Returns(false);
			_instance = new LoadFileDataReader(_providerSettings, _loadFile, _loadFileReader);

			//Act
			_instance.Init();

			//Assert
			Assert.IsFalse(_instance.Read());
			Assert.IsTrue(_instance.IsClosed);
		}

		[Test]
		public void ItShouldCallReadArtifact_IfReaderHasMoreRecords()
		{
			//Arrange
			_instance = new LoadFileDataReader(_providerSettings, _loadFile, _loadFileReader);

			//Act
			_instance.Init();

			//Assert
			Assert.IsTrue(_instance.Read());
			_loadFileReader.Received(1).ReadArtifact();
		}

		[Test]
		public void ItShouldRewriteFileLocations_WithoutRootedPath()
		{
			//Arrange
			LoadArtifact(_UN_ROOTED_PATH_ARTIFACT_INDEX);
			_providerSettings.NativeFilePathFieldIdentifier = _NATIVE_PATH_LOAD_FILE_INDEX.ToString();
			_providerSettings.ExtractedTextPathFieldIdentifier = _EXTRACTED_TEXT_PATH_LOAD_FILE_INDEX.ToString();
			_instance = new LoadFileDataReader(_providerSettings, _loadFile, _loadFileReader);

			//Act
			_instance.Init();

			//Assert
			Assert.IsTrue(_instance.Read());
			Assert.AreEqual(Path.Combine(Path.GetDirectoryName(_LOAD_FILE_FULL_PATH), _RECORDS[_currentArtifactIndex][_NATIVE_PATH_LOAD_FILE_INDEX]),
				_instance.GetValue(_NATIVE_PATH_LOAD_FILE_INDEX));
			Assert.AreEqual(Path.Combine(Path.GetDirectoryName(_LOAD_FILE_FULL_PATH), _RECORDS[_currentArtifactIndex][_EXTRACTED_TEXT_PATH_LOAD_FILE_INDEX]),
				_instance.GetValue(_EXTRACTED_TEXT_PATH_LOAD_FILE_INDEX));
		}

		[Test]
		public void ItShouldNotRewriteFileLocation_WithRootedPath()
		{
			//Arrange
			LoadArtifact(_ROOTED_PATH_ARTIFACT_INDEX);
			_providerSettings.NativeFilePathFieldIdentifier = _NATIVE_PATH_LOAD_FILE_INDEX.ToString();
			_providerSettings.ExtractedTextPathFieldIdentifier = _EXTRACTED_TEXT_PATH_LOAD_FILE_INDEX.ToString();
			_instance = new LoadFileDataReader(_providerSettings, _loadFile, _loadFileReader);

			//Act
			_instance.Init();

			//Assert
			Assert.IsTrue(_instance.Read());
			Assert.AreEqual(_RECORDS[_currentArtifactIndex][_NATIVE_PATH_LOAD_FILE_INDEX], _instance.GetValue(_NATIVE_PATH_LOAD_FILE_INDEX));
			Assert.AreEqual(_RECORDS[_currentArtifactIndex][_EXTRACTED_TEXT_PATH_LOAD_FILE_INDEX], _instance.GetValue(_EXTRACTED_TEXT_PATH_LOAD_FILE_INDEX));
		}

		[Test]
		public void ItShouldSequentiallyNameColumnsInSchemaTable()
		{
			//Arrange
			_instance = new LoadFileDataReader(_providerSettings, _loadFile, _loadFileReader);

			//Act
			_instance.Init();

			//Assert
			Assert.IsTrue(_instance.Read());
			DataTable schemaTable = _instance.GetSchemaTable(); 
			for (int i = 0; i < schemaTable.Columns.Count; i++)
			{
				Assert.AreEqual(i.ToString(), schemaTable.Columns[i].ColumnName);
			}
		}

		[Test]
		public void ItShouldPassThroughCallsToManageErrorRecords()
		{
			//Arrange
			_instance = new LoadFileDataReader(_providerSettings, _loadFile, _loadFileReader);

			//Act
			_instance.Init();

			//Assert
			Assert.AreEqual(_ERROR_FILE_PATH, _instance.ManageErrorRecords(string.Empty, string.Empty));
		}

		[Test]
		public void ItShouldPassThroughCallsToCountRecords()
		{
			//Arrange
			_instance = new LoadFileDataReader(_providerSettings, _loadFile, _loadFileReader);

			//Act
			_instance.Init();

			//Assert
			Assert.AreEqual(_RECORDS.Length, _instance.CountRecords());
		}

		[Test]
		public void ItShouldNotBeClosed_WhenFirstCreated()
		{
			//Arrange
			_instance = new LoadFileDataReader(_providerSettings, _loadFile, _loadFileReader);

			//Act
			_instance.Init();

			//Assert
			Assert.IsFalse(_instance.IsClosed);
		}

		[Test]
		public void ItShouldReturnFieldCountOfZero_WhenOperatingOnEmptyFile()
		{
			//Arrange
			_loadFileReader.GetColumnNames(Arg.Any<object>()).Returns(new string[0]);
			_instance = new LoadFileDataReader(_providerSettings, _loadFile, _loadFileReader);

			//Act
			_instance.Init();

			//Assert
			Assert.AreEqual(0, _instance.FieldCount);
		}

		[Test]
		public void ItShouldReturnFieldCount_WhenOperatingOnNonEmptyFile()
		{
			//Arrange
			_instance = new LoadFileDataReader(_providerSettings, _loadFile, _loadFileReader);

			//Act
			_instance.Init();

			//Assert
			Assert.AreEqual(_HEADERS.Length, _instance.FieldCount);
		}

		[Test]
		public void ItShouldReturnCorrectOrdinal()
		{
			//Arrange
			_instance = new LoadFileDataReader(_providerSettings, _loadFile, _loadFileReader);

			//Act
			_instance.Init();

			//Assert
			for (int i = 0; i < _HEADERS.Length; i++)
			{
				Assert.AreEqual(i, _instance.GetOrdinal(i.ToString()));
			}
		}

		[Test]
		public void ItShouldReturnCorrectName()
		{
			//Arrange
			_instance = new LoadFileDataReader(_providerSettings, _loadFile, _loadFileReader);

			//Act
			_instance.Init();

			//Assert
			for (int i = 0; i < _HEADERS.Length; i++)
			{
				Assert.AreEqual(i.ToString(), _instance.GetName(i));
			}
		}

		//Note: This unit test is related to a work-around; see comment above catch block in LoadFileDataReader.ReadCurrentRecord
		[Test]
		public void ItShouldBlankOutput_WhenLoadFileReaderThrowsColumnCountMismatchException()
		{
			_loadFileReader.ReadArtifact().Throws(new kCura.WinEDDS.LoadFileBase.ColumnCountMismatchException(0,0,0));
			_instance = new LoadFileDataReader(_providerSettings, _loadFile, _loadFileReader);

			//Act
			_instance.Init();
			_instance.Read();

			//Assert
			for (int i = 0; i < _RECORDS[_currentArtifactIndex].Length; i++)
			{
				Assert.IsEmpty(_instance.GetString(i));
			}
		}

		[Test]
		public void ItShouldThrowNotImplementedForUnusedMethods()
		{
			//Arrange
			_instance = new LoadFileDataReader(_providerSettings, _loadFile, _loadFileReader);

			//Act
			_instance.Init();

			//Assert

			//Methods
			Assert.Throws(typeof(System.NotImplementedException), () => _instance.GetDataTypeName(0));
			Assert.Throws(typeof(System.NotImplementedException), () => _instance.GetFieldType(0));
			Assert.Throws(typeof(System.NotImplementedException), () => _instance.ReadArtifact());
			Assert.Throws(typeof(System.NotImplementedException), () => _instance.GetColumnNames(null));
			Assert.Throws(typeof(System.NotImplementedException), () => _instance.SourceIdentifierValue());
			Assert.Throws(typeof(System.NotImplementedException), () => _instance.AdvanceRecord());
			Assert.Throws(typeof(System.NotImplementedException), () => _instance.OnFatalErrorState());
			Assert.Throws(typeof(System.NotImplementedException), () => _instance.Halt());

			//Properties
			Assert.Throws(typeof(System.NotImplementedException), () => { bool testProperty = _instance.HasMoreRecords; });
			Assert.Throws(typeof(System.NotImplementedException), () => { int testProperty = _instance.CurrentLineNumber; });
			Assert.Throws(typeof(System.NotImplementedException), () => { long testProperty = _instance.SizeInBytes; });
			Assert.Throws(typeof(System.NotImplementedException), () => { long testProperty = _instance.BytesProcessed; });

			//Events
			IArtifactReader instanceAsArtifactReader = (IArtifactReader)_instance;

			Assert.Throws(typeof(System.NotImplementedException), () => { instanceAsArtifactReader.OnIoWarning += (e) => { }; });
			Assert.Throws(typeof(System.NotImplementedException), () => { instanceAsArtifactReader.DataSourcePrep += (e) => { }; });
			Assert.Throws(typeof(System.NotImplementedException), () => { instanceAsArtifactReader.StatusMessage += (e) => { }; });
			Assert.Throws(typeof(System.NotImplementedException), () => { instanceAsArtifactReader.FieldMapped += (srcField, wsField) => { }; });
			Assert.Throws(typeof(System.NotImplementedException), () => { instanceAsArtifactReader.OnIoWarning += (e) => { }; });

			Assert.Throws(typeof(System.NotImplementedException), () => { instanceAsArtifactReader.OnIoWarning -= (e) => { }; });
			Assert.Throws(typeof(System.NotImplementedException), () => { instanceAsArtifactReader.DataSourcePrep -= (e) => { }; });
			Assert.Throws(typeof(System.NotImplementedException), () => { instanceAsArtifactReader.StatusMessage -= (e) => { }; });
			Assert.Throws(typeof(System.NotImplementedException), () => { instanceAsArtifactReader.FieldMapped -= (srcField, wsField) => { }; });
			Assert.Throws(typeof(System.NotImplementedException), () => { instanceAsArtifactReader.OnIoWarning -= (e) => { }; });
		}
	}
}