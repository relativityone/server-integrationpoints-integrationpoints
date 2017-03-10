using System.Data;
using System.IO;
using System.Reflection;

using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;
using NSubstitute;
using kCura.IntegrationPoints.ImportProvider.Parser.Services;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
	[TestFixture, Category("ImportProvider")]
	public class ImportDataReaderTests : TestBase
	{
		private const string _RESOURCE_STREAM_ROOT = "kCura.IntegrationPoints.ImportProvider.Parser.Tests.";
		private const string _CSV_RESOURCE = "CsvResources.";
		private const string _JSON_RESOURCE = "JsonResources.";
		private const string _FIELDMAP_WITH_FOLDER = "FieldMap-With-Folder.json";
		private const string _FIELDMAP_WITHOUT_FOLDER = "FieldMap-Without-Folder.json";
		private const string _LOADFILE_1 = "small.csv";

		[SetUp]
		public override void SetUp()
		{
		}

		[Test]
		public void ItShouldHaveAllMappedColumns_WithNoFolderMapping()
		{
			//Arrange
			DataTable sourceDataTable = SourceDataTable(_LOADFILE_1);
			List<FieldMap> fieldMaps = FieldMapObject(_FIELDMAP_WITHOUT_FOLDER);

			//Act
			ImportDataReader idr = new ImportDataReader(sourceDataTable.CreateDataReader());
			idr.Setup(fieldMaps.ToArray());

			//Assert

			//ImportDataReader schema should have all columns in field map
			List<string> columnNames = idr.GetSchemaTable().Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();
			foreach (FieldMap map in fieldMaps)
			{
				Assert.IsTrue(columnNames.Contains(map.SourceField.FieldIdentifier));
			}

			//ImportDataReader schema should have no extra columns
			Assert.AreEqual(fieldMaps.Count, columnNames.Count);
		}

		[Test]
		public void ItShouldHaveAllMappedColumnsAndFolder_WithFolderMapping()
		{
			//Arrange
			DataTable sourceDataTable = SourceDataTable(_LOADFILE_1);
			List<FieldMap> fieldMaps = FieldMapObject(_FIELDMAP_WITH_FOLDER);

			//Act
			ImportDataReader idr = new ImportDataReader(sourceDataTable.CreateDataReader());
			idr.Setup(fieldMaps.ToArray());

			//Assert

			//ImportDataReader schema should have all columns in field map plus extra special column
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

			//ImportDataReader schema should have no extra columns
			Assert.AreEqual(fieldMaps.Count, columnNames.Count);
		}

		[Test]
		public void ItShouldProvideCorrectData_WithNoFolderMapping()
		{
			//Arrange
			DataTable sourceDataTable = SourceDataTable(_LOADFILE_1);
			List<FieldMap> fieldMaps = FieldMapObject(_FIELDMAP_WITHOUT_FOLDER);

			//Act
			ImportDataReader idr = new ImportDataReader(sourceDataTable.CreateDataReader());
			idr.Setup(fieldMaps.ToArray());

			//Assert

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
			//Arrange
			DataTable sourceDataTable = SourceDataTable(_LOADFILE_1);
			List<FieldMap> fieldMaps = FieldMapObject(_FIELDMAP_WITH_FOLDER);

			//Act
			ImportDataReader idr = new ImportDataReader(sourceDataTable.CreateDataReader());
			idr.Setup(fieldMaps.ToArray());

			//Assert

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
