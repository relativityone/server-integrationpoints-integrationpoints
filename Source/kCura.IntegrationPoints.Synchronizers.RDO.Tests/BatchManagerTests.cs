using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
    [TestFixture, Category("Unit")]
    public class BatchManagerTests : TestBase
    {
        [SetUp]
        public override void SetUp()
        {

        }

        [Test]
        public void Add_RaisesOnBatchCreateEvent()
        {
            // ARRANGE
            BatchManager batchManager = new BatchManager();
            bool eventFired = false;

            batchManager.OnBatchCreate += delegate ( int batchSize )
            {
                eventFired = true;
            };

            var fileDataToAdd = new Dictionary<string, object>();
            fileDataToAdd.Add("F1", 111);

            // ACT
            batchManager.Add(fileDataToAdd);

            // ASSERT
            Assert.IsTrue( eventFired );
            Assert.AreEqual(1, batchManager.CurrentSize );
        }

        [Test]
        public void IsBatchFull_ReturnsTrueWhenCountIsEqualOrGreaterThanMinBatchSize()
        {
            BatchManager batchManager = new BatchManager(2);

            // less than 2
            batchManager.Add(new Dictionary<string, object>());
            Assert.IsFalse(batchManager.IsBatchFull());

            // equal to 2
            batchManager.Add(new Dictionary<string, object>());
            Assert.IsTrue(batchManager.IsBatchFull());

            // greater than 2
            batchManager.Add(new Dictionary<string, object>());
            Assert.IsTrue(batchManager.IsBatchFull());
        }

        [Test]
        public void GetBatchData_ReturnsDataReaderEquivalentToConfigureTable()
        {
            // ARRANGE
            HashSet<string> columnNamesSet = new HashSet<string>() { "F1","F2","F3" };
            IEnumerable<string> columnNames = columnNamesSet;
            var fileData = new Dictionary<string, object>()
            {
                {"F1", 111},
                {"F2", DateTime.Parse("11/22/2014 11:22:33")},
                {"F3", "Hello"}
            };

            BatchManager batchManager = new BatchManager() { ColumnNames = columnNamesSet };
            batchManager.Add(fileData);

            // ACT
            var result = batchManager.GetBatchData();
            var expected = batchManager.ConfigureTable(columnNames, new List<IDictionary<string, object>> {fileData})
                .CreateDataReader();

            // ASSERT
            Assert.AreEqual(expected.FieldCount, result.FieldCount);
            Assert.AreEqual(expected.Read(), result.Read());
            Assert.AreEqual(expected.GetValue(0), result.GetValue(0));
            Assert.AreEqual(expected.GetValue(1), result.GetValue(1));
            Assert.AreEqual(expected.GetValue(2), result.GetValue(2));
            Assert.IsFalse(result.Read());
        }

        [Test]
        public void ClearDataSource_Pass()
        {
            // ARRANGE
            var fileData = new Dictionary<string, object>()
            {
                {"F1", 111},
                {"F2", DateTime.Parse("11/22/2014 11:22:33")},
                {"F3", "Hello"}
            };

            BatchManager batchManager = new BatchManager();
            batchManager.Add(fileData);

            // ACT
            batchManager.ClearDataSource();

            // ASSERT
            Assert.AreEqual(0, batchManager.CurrentSize);
        }

        [Test]
        public void ConfigureTable_Pass()
        {
            // ARRANGE
            HashSet<string> columnNamesSet = new HashSet<string>() { "F1", "F2", "F3" };
            IEnumerable<string> columnNames = columnNamesSet;
            List<IDictionary<string, object>> dataSource = new List<IDictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    {"F1", 111},
                    {"F2", DateTime.Parse("11/22/2014 11:22:33")},
                    {"F3", "Hello"}
                },
                new Dictionary<string, object>()
                {
                    {"F1", 222},
                    {"F2", DateTime.Parse("11/23/2014 11:22:33")},
                    {"F3", "Goodbye"}
                },
                new Dictionary<string, object>()
                {
                    {"F1", 333},
                    {"F2", DateTime.Parse("11/24/2014 11:22:33")},
                    {"F3", "Privet"}
                },
            };

            BatchManager batchManager = new BatchManager() { ColumnNames = columnNamesSet };

            // ACT
            DataTable dataTable = batchManager.ConfigureTable(columnNames, dataSource);

            // ASSERT
            Assert.AreEqual(3, dataTable.Rows.Count);
            Assert.AreEqual("111", dataTable.Rows[0]["F1"]);
            Assert.AreEqual(DateTime.Parse("11/22/2014 11:22:33").ToString(), dataTable.Rows[0]["F2"]);
            Assert.AreEqual("Hello", dataTable.Rows[0]["F3"]);
            Assert.AreEqual("222", dataTable.Rows[1]["F1"]);
            Assert.AreEqual(DateTime.Parse("11/23/2014 11:22:33").ToString(), dataTable.Rows[1]["F2"]);
            Assert.AreEqual("Goodbye", dataTable.Rows[1]["F3"]);
            Assert.AreEqual("333", dataTable.Rows[2]["F1"]);
            Assert.AreEqual(DateTime.Parse("11/24/2014 11:22:33").ToString(), dataTable.Rows[2]["F2"]);
            Assert.AreEqual("Privet", dataTable.Rows[2]["F3"]);
        }

        [Test]
        public void ConfigureTable_NoRecords_ReturnsNull()
        {
            // ARRANGE
            HashSet<string> columnNamesSet = new HashSet<string>() { "F1", "F2", "F3" };
            IEnumerable<string> columnNames = columnNamesSet;
            List<IDictionary<string, object>> dataSource = new List<IDictionary<string, object>>();

            BatchManager batchManager = new BatchManager() { ColumnNames = columnNamesSet };

            // ACT
            DataTable dataTable = batchManager.ConfigureTable(columnNames, dataSource);

            // ASSERT
            Assert.IsNull(dataTable);
        }

        [Test]
        public void ConfigureTable_NoNativeFilePathInColumnNames_ColumnIsAdded()
        {
            // ARRANGE
            HashSet<string> columnNamesSet = new HashSet<string>() { "F1", "F2", "F3" };
            IEnumerable<string> columnNames = columnNamesSet;
            List<IDictionary<string, object>> dataSource = new List<IDictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    {"F1", 111},
                    {"F2", DateTime.Parse("11/22/2014 11:22:33")},
                    {"F3", "Hello"},
                    {kCura.IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME, "\\\\P-NOPE-NOPE\\Nope\\nope.txt" }
                },
                new Dictionary<string, object>()
                {
                    {"F1", 222},
                    {"F2", DateTime.Parse("11/23/2014 11:22:33")},
                    {"F3", "Goodbye"},
                    {kCura.IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME, "\\\\P-NOPE-NOPE\\Nope\\nope.txt" }
                },
                new Dictionary<string, object>()
                {
                    {"F1", 333},
                    {"F2", DateTime.Parse("11/24/2014 11:22:33")},
                    {"F3", "Privet"},
                    {kCura.IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME, "\\\\P-NOPE-NOPE\\Nope\\nope.txt" }
                },
            };

            BatchManager batchManager = new BatchManager() { ColumnNames = columnNamesSet };

            // ACT
            DataTable dataTable = batchManager.ConfigureTable(columnNames, dataSource);

            // ASSERT
            Assert.AreEqual(3, dataTable.Rows.Count);
            Assert.AreEqual("111", dataTable.Rows[0]["F1"]);
            Assert.AreEqual(DateTime.Parse("11/22/2014 11:22:33").ToString(), dataTable.Rows[0]["F2"]);
            Assert.AreEqual("Hello", dataTable.Rows[0]["F3"]);
            Assert.AreEqual("\\\\P-NOPE-NOPE\\Nope\\nope.txt", dataTable.Rows[0][kCura.IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME]);
            Assert.AreEqual("222", dataTable.Rows[1]["F1"]);
            Assert.AreEqual(DateTime.Parse("11/23/2014 11:22:33").ToString(), dataTable.Rows[1]["F2"]);
            Assert.AreEqual("Goodbye", dataTable.Rows[1]["F3"]);
            Assert.AreEqual("\\\\P-NOPE-NOPE\\Nope\\nope.txt", dataTable.Rows[1][kCura.IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME]);
            Assert.AreEqual("333", dataTable.Rows[2]["F1"]);
            Assert.AreEqual(DateTime.Parse("11/24/2014 11:22:33").ToString(), dataTable.Rows[2]["F2"]);
            Assert.AreEqual("Privet", dataTable.Rows[2]["F3"]);
            Assert.AreEqual("\\\\P-NOPE-NOPE\\Nope\\nope.txt", dataTable.Rows[2][kCura.IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME]);
        }

        [Test]
        public void ConfigureTable_NativeFilePathInColumnNames_ColumnIsNotAddedTwice()
        {
            // ARRANGE
            HashSet<string> columnNamesSet = new HashSet<string>() { "F1", "F2", "F3", kCura.IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME };
            IEnumerable<string> columnNames = columnNamesSet;
            List<IDictionary<string, object>> dataSource = new List<IDictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    {"F1", 111},
                    {"F2", DateTime.Parse("11/22/2014 11:22:33")},
                    {"F3", "Hello"},
                    {
                        kCura.IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME,
                        "\\\\P-NOPE-NOPE\\Nope\\nope.txt"
                    }
                },
                new Dictionary<string, object>()
                {
                    {"F1", 222},
                    {"F2", DateTime.Parse("11/23/2014 11:22:33")},
                    {"F3", "Goodbye"},
                    {
                        kCura.IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME,
                        "\\\\P-NOPE-NOPE\\Nope\\nope.txt"
                    }
                },
                new Dictionary<string, object>()
                {
                    {"F1", 333},
                    {"F2", DateTime.Parse("11/24/2014 11:22:33")},
                    {"F3", "Privet"},
                    {
                        kCura.IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME,
                        "\\\\P-NOPE-NOPE\\Nope\\nope.txt"
                    }
                },
            };

            BatchManager batchManager = new BatchManager() { ColumnNames = columnNamesSet };

            // ACT
            DataTable dataTable = batchManager.ConfigureTable(columnNames, dataSource);

            // ASSERT
            Assert.AreEqual(3, dataTable.Rows.Count);
            Assert.AreEqual("111", dataTable.Rows[0]["F1"]);
            Assert.AreEqual(DateTime.Parse("11/22/2014 11:22:33").ToString(), dataTable.Rows[0]["F2"]);
            Assert.AreEqual("Hello", dataTable.Rows[0]["F3"]);
            Assert.AreEqual("\\\\P-NOPE-NOPE\\Nope\\nope.txt",
                dataTable.Rows[0][kCura.IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME]);
            Assert.AreEqual("222", dataTable.Rows[1]["F1"]);
            Assert.AreEqual(DateTime.Parse("11/23/2014 11:22:33").ToString(), dataTable.Rows[1]["F2"]);
            Assert.AreEqual("Goodbye", dataTable.Rows[1]["F3"]);
            Assert.AreEqual("\\\\P-NOPE-NOPE\\Nope\\nope.txt",
                dataTable.Rows[1][kCura.IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME]);
            Assert.AreEqual("333", dataTable.Rows[2]["F1"]);
            Assert.AreEqual(DateTime.Parse("11/24/2014 11:22:33").ToString(), dataTable.Rows[2]["F2"]);
            Assert.AreEqual("Privet", dataTable.Rows[2]["F3"]);
            Assert.AreEqual("\\\\P-NOPE-NOPE\\Nope\\nope.txt",
                dataTable.Rows[2][kCura.IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME]);
        }
    }
}
