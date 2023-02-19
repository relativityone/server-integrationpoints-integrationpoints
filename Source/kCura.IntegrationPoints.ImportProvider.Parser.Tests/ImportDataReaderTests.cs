using System.Data;
using System.IO;
using System.Reflection;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;
using NSubstitute;
using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS.Api;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
    [TestFixture, Category("Unit"), Category("ImportProvider")]
    public class ImportDataReaderTests : TestBase
    {
        private const string _RESOURCE_STREAM_ROOT = "kCura.IntegrationPoints.ImportProvider.Parser.Tests.";
        private const string _CSV_RESOURCE = "CsvResources.";
        private const string _JSON_RESOURCE = "JsonResources.";
        private const string _FIELDMAP_WITH_FOLDER = "FieldMap-With-Folder.json";
        private const string _FIELDMAP_WITHOUT_FOLDER = "FieldMap-Without-Folder.json";
        private const string _FIELDMAP_WITH_NATIVE_FILE_PATH = "FieldMap-With-Native-File-Path.json";
        private const string _LOADFILE_1 = "small.csv";

        [SetUp]
        public override void SetUp()
        {
        }

        [Test]
        public void ItShouldHaveAllMappedColumns_WithNoFolderMapping()
        {
            // Arrange
            DataTable sourceDataTable = SourceDataTable(_LOADFILE_1);
            List<FieldMap> fieldMaps = FieldMapObject(_FIELDMAP_WITHOUT_FOLDER);

            // Act
            ImportDataReader idr = new ImportDataReader(sourceDataTable.CreateDataReader());
            idr.Setup(fieldMaps.ToArray());

            // Assert

            // ImportDataReader schema should have all columns in field map
            List<string> columnNames = idr.GetSchemaTable().Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();
            foreach (FieldMap map in fieldMaps)
            {
                Assert.IsTrue(columnNames.Contains(map.SourceField.FieldIdentifier));
            }

            // ImportDataReader schema should have no extra columns
            Assert.AreEqual(fieldMaps.Count, columnNames.Count);
            // ImportDataReader schema should have number of mapped columns
            Assert.AreEqual(fieldMaps.Count, idr.FieldCount);
        }

        [Test]
        public void ItShouldHaveAdditionalColumn_WithNativeFileMapping()
        {
            // Arrange
            DataTable sourceDataTable = SourceDataTable(_LOADFILE_1);
            List<FieldMap> fieldMaps = FieldMapObject(_FIELDMAP_WITH_NATIVE_FILE_PATH);

            // Act
            ImportDataReader idr = new ImportDataReader(sourceDataTable.CreateDataReader());
            idr.Setup(fieldMaps.ToArray());

            // Assert

            // ImportDataReader schema should have all columns in field map
            List<string> columnNames = idr.GetSchemaTable().Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();
            foreach (FieldMap map in fieldMaps)
            {
                Assert.IsTrue(columnNames.Contains(map.SourceField.FieldIdentifier));
            }

            // ImportDataReader schema should have no extra columns
            Assert.AreEqual(fieldMaps.Count + 1, columnNames.Count);
            // ImportDataReader schema should have number of mapped columns
            Assert.AreEqual(fieldMaps.Count + 1, idr.FieldCount);
        }

        [Test]
        public void ItShouldHaveAllMappedColumnsAndFolder_WithFolderMapping()
        {
            // Arrange
            DataTable sourceDataTable = SourceDataTable(_LOADFILE_1);
            List<FieldMap> fieldMaps = FieldMapObject(_FIELDMAP_WITH_FOLDER);

            // Act
            ImportDataReader idr = new ImportDataReader(sourceDataTable.CreateDataReader());
            idr.Setup(fieldMaps.ToArray());

            // Assert

            // ImportDataReader schema should have all columns in field map plus extra special column
            List<string> columnNames = idr.GetSchemaTable().Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();
            foreach (FieldMap map in fieldMaps)
            {
                if (map.FieldMapType == FieldMapTypeEnum.FolderPathInformation)
                {
                    Assert.IsTrue(columnNames.Contains(Domain.Constants.SPECIAL_FOLDERPATH_FIELD));
                }
                else
                {
                    Assert.IsTrue(columnNames.Contains(map.SourceField.FieldIdentifier));
                }
            }

            // ImportDataReader schema should have one extra column (folder mapped to group id field)
            Assert.AreEqual(fieldMaps.Count + 1, columnNames.Count);
            // ImportDataReader schema should have same number of columns as source data
            Assert.AreEqual(fieldMaps.Count + 1, idr.FieldCount);
        }

        [Test]
        public void ItShouldProvideCorrectData_WithNoFolderMapping()
        {
            // Arrange
            DataTable sourceDataTable = SourceDataTable(_LOADFILE_1);
            List<FieldMap> fieldMaps = FieldMapObject(_FIELDMAP_WITHOUT_FOLDER);

            // Act
            ImportDataReader idr = new ImportDataReader(sourceDataTable.CreateDataReader());
            idr.Setup(fieldMaps.ToArray());

            // Assert

            List<string> columnNames = idr.GetSchemaTable().Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();
            int dtRowIndex = 0;
            while (idr.Read())
            {
                for (int i = 0; i < columnNames.Count; i++)
                {
                    Assert.AreEqual(sourceDataTable.Rows[dtRowIndex][columnNames[i]], idr[i]);
                }
                dtRowIndex++;
            }

            Assert.AreEqual(sourceDataTable.Rows.Count, dtRowIndex);
        }

        [Test]
        public void ItShouldProvideCorrectData_WithFolderMapping()
        {
            // Arrange
            DataTable sourceDataTable = SourceDataTable(_LOADFILE_1);
            List<FieldMap> fieldMaps = FieldMapObject(_FIELDMAP_WITH_FOLDER);

            // Act
            ImportDataReader idr = new ImportDataReader(sourceDataTable.CreateDataReader());
            idr.Setup(fieldMaps.ToArray());

            // Assert

            List<string> columnNames = idr.GetSchemaTable().Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();
            Assert.IsTrue(columnNames.Contains(Domain.Constants.SPECIAL_FOLDERPATH_FIELD));
            string folderSourceColumnName =
                fieldMaps.FirstOrDefault(x =>x.FieldMapType == FieldMapTypeEnum.FolderPathInformation).SourceField.FieldIdentifier;

            int dtRowIndex = 0;
            while (idr.Read())
            {
                for (int i = 0; i < columnNames.Count; i++)
                {
                    if (columnNames[i] == Domain.Constants.SPECIAL_FOLDERPATH_FIELD)
                    {
                        Assert.AreEqual(sourceDataTable.Rows[dtRowIndex][folderSourceColumnName], idr[i]);
                    }
                    else
                    {
                        Assert.AreEqual(sourceDataTable.Rows[dtRowIndex][columnNames[i]], idr[i]);
                    }
                }
                dtRowIndex++;
            }

            Assert.AreEqual(sourceDataTable.Rows.Count, dtRowIndex);
        }

        [Test]
        public void ItShouldProvideCorrectData_WithNativeFileMapping()
        {
            // Arrange
            DataTable sourceDataTable = SourceDataTable(_LOADFILE_1);
            List<FieldMap> fieldMaps = FieldMapObject(_FIELDMAP_WITH_NATIVE_FILE_PATH);

            // Act
            ImportDataReader idr = new ImportDataReader(sourceDataTable.CreateDataReader());
            idr.Setup(fieldMaps.ToArray());

            // Assert

            List<string> columnNames = idr.GetSchemaTable().Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();
            Assert.IsTrue(columnNames.Contains(Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD));

            string nativePathColumnName =
                fieldMaps.FirstOrDefault(x => x.FieldMapType == FieldMapTypeEnum.NativeFilePath)?.SourceField.FieldIdentifier;

            int dtRowIndex = 0;
            while (idr.Read())
            {
                for (int i = 0; i < columnNames.Count; i++)
                {
                    if (columnNames[i] == Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD)
                    {
                        Assert.AreEqual(sourceDataTable.Rows[dtRowIndex][nativePathColumnName], idr[i]);
                    }
                    else
                    {
                        Assert.AreEqual(sourceDataTable.Rows[dtRowIndex][columnNames[i]], idr[i]);
                    }
                }
                dtRowIndex++;
            }

            Assert.AreEqual(sourceDataTable.Rows.Count, dtRowIndex);
        }

        [Test]
        public void ItShouldPassThroughCallsToManageErrorRecords()
        {
            // Arrange
            IDataReader dataSource = Substitute.For<IDataReader, IArtifactReader>();
            ((IArtifactReader) dataSource).ManageErrorRecords(Arg.Any<string>(), Arg.Any<string>()).Returns("Error_file");

            // Act
            ImportDataReader idr = new ImportDataReader(dataSource);

            // Assert
            Assert.AreEqual("Error_file", idr.ManageErrorRecords(string.Empty, string.Empty));
        }

        [Test]
        public void ItShouldPassThroughCallsToCountRecords()
        {
            // Arrange
            IDataReader dataSource = Substitute.For<IDataReader, IArtifactReader>();
            ((IArtifactReader) dataSource).CountRecords().Returns(1);

            // Act
            ImportDataReader idr = new ImportDataReader(dataSource);

            // Assert
            Assert.AreEqual(1, idr.CountRecords());
        }

        [Test]
        public void ItShouldNotBeClosed_WhenFirstCreated()
        {
            // Arrange
            DataTable sourceDataTable = SourceDataTable(_LOADFILE_1);
            List<FieldMap> fieldMaps = FieldMapObject(_FIELDMAP_WITH_FOLDER);

            // Act
            ImportDataReader idr = new ImportDataReader(sourceDataTable.CreateDataReader());
            idr.Setup(fieldMaps.ToArray());

            // Assert
            Assert.IsFalse(idr.IsClosed);
        }

        [Test]
        public void ItShouldReturnTheCorrectName_WhenFolderMappingIsPresent()
        {
            // Arrange
            DataTable sourceDataTable = SourceDataTable(_LOADFILE_1);
            List<FieldMap> fieldMaps = FieldMapObject(_FIELDMAP_WITH_FOLDER);

            Dictionary<int, string> nameMap = new Dictionary<int, string>();
            nameMap[0] = "0";
            nameMap[1] = "1";
            nameMap[2] = kCura.IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD;
            nameMap[3] = "2";

            // Act
            ImportDataReader idr = new ImportDataReader(sourceDataTable.CreateDataReader());
            idr.Setup(fieldMaps.ToArray());

            foreach (int key in nameMap.Keys)
            {
                Assert.AreEqual(nameMap[key], idr.GetName(key));
            }
        }

        [Test]
        public void ItShouldReturnTheCorrectName_WhenFolderMappingIsNotPresent()
        {
            // Arrange
            DataTable sourceDataTable = SourceDataTable(_LOADFILE_1);
            List<FieldMap> fieldMaps = FieldMapObject(_FIELDMAP_WITHOUT_FOLDER);

            Dictionary<int, string> nameMap = new Dictionary<int, string>();
            nameMap[0] = "0";
            nameMap[1] = "2";

            // Act
            ImportDataReader idr = new ImportDataReader(sourceDataTable.CreateDataReader());
            idr.Setup(fieldMaps.ToArray());

            foreach (int key in nameMap.Keys)
            {
                Assert.AreEqual(nameMap[key], idr.GetName(key));
            }
        }

        [Test]
        public void ItShouldReturnTheCorrectOrdinal_WhenFolderMappingIsPresent()
        {
            // Arrange
            DataTable sourceDataTable = SourceDataTable(_LOADFILE_1);
            List<FieldMap> fieldMaps = FieldMapObject(_FIELDMAP_WITH_FOLDER);

            Dictionary<string, int> ordinalMap = new Dictionary<string, int>();
            ordinalMap["0"] = 0;
            ordinalMap["1"] = 1;
            ordinalMap[kCura.IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD] = 2;
            ordinalMap["2"] = 3;

            // Act
            ImportDataReader idr = new ImportDataReader(sourceDataTable.CreateDataReader());
            idr.Setup(fieldMaps.ToArray());

            foreach (string key in ordinalMap.Keys)
            {
                Assert.AreEqual(ordinalMap[key], idr.GetOrdinal(key));
            }
        }

        [Test]
        public void ItShouldReturnTheCorrectOrdinal_WhenFolderMappingIsNotPresent()
        {
            // Arrange
            DataTable sourceDataTable = SourceDataTable(_LOADFILE_1);
            List<FieldMap> fieldMaps = FieldMapObject(_FIELDMAP_WITHOUT_FOLDER);

            Dictionary<string, int> ordinalMap = new Dictionary<string, int>();
            ordinalMap["0"] = 0;
            ordinalMap["2"] = 1;

            // Act
            ImportDataReader idr = new ImportDataReader(sourceDataTable.CreateDataReader());
            idr.Setup(fieldMaps.ToArray());

            foreach (string key in ordinalMap.Keys)
            {
                Assert.AreEqual(ordinalMap[key], idr.GetOrdinal(key));
            }
        }

        [Test]
        public void ItShouldBeClosedAfterCloseCalled()
        {
            ImportDataReader idr = new ImportDataReader((new DataTable()).CreateDataReader());
            idr.Close();
            Assert.IsTrue(idr.IsClosed);
        }

        [Test]
        public void ItShouldReturnFalseFromReadWhenClosed()
        {
            ImportDataReader idr = new ImportDataReader((new DataTable()).CreateDataReader());
            idr.Close();
            Assert.IsFalse(idr.Read());
        }

        private DataTable SourceDataTable(string resourceName)
        {
            DataTable rv = new DataTable();
            bool columnsAdded = false;
            using (StreamReader reader = LoadFileResourceStreamReader(resourceName))
            {
                while (!reader.EndOfStream)
                {
                    string currentLine = reader.ReadLine();
                    string[] splitted = currentLine.Split(',');

                    if (!columnsAdded)
                    {
                        for (int i = 0; i < splitted.Length; i++)
                        {
                            rv.Columns.Add(i.ToString());
                        }
                        columnsAdded = true;
                    }

                    DataRow row = rv.NewRow();
                    for (int i = 0; i < splitted.Length; i++)
                    {
                        row[i.ToString()] = splitted[i];
                    }
                    rv.Rows.Add(row);
                }
            }

            return rv;
        }

        private List<FieldMap> FieldMapObject(string fieldMapResourceName)
        {
            JSONSerializer serializer = new JSONSerializer();
            return serializer.Deserialize<List<FieldMap>>(FieldMapResourceStreamReader(fieldMapResourceName).ReadToEnd());
        }

        private StreamReader FieldMapResourceStreamReader(string fieldMapResourceName)
        {
            return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(_RESOURCE_STREAM_ROOT + _JSON_RESOURCE + fieldMapResourceName));
        }

        private StreamReader LoadFileResourceStreamReader(string loadFileResourceName)
        {
            return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(_RESOURCE_STREAM_ROOT + _CSV_RESOURCE + loadFileResourceName));
        }
    }
}
