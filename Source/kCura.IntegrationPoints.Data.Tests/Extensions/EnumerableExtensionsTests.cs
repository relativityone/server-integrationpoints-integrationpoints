using System.Data;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Extensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Extensions
{
    [TestFixture, Category("Unit")]
    public class EnumerableExtensionsTests
    {
        private string[] _expectedColumnNames;
        private readonly TestCase[] _testCases =
        {
            new TestCase
            {
                ID = 1,
                Name = "TestCase1",
                Duration = 123.2,
                Description = "TestDescription1"
            },
            new TestCase
            {
                ID = 2,
                Name = "TestCase2",
                Duration = 123.21,
                Description = "TestDescription2"
            }
        };

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            PropertyInfo[] columns = typeof(TestCase).GetProperties().ToArray();
            _expectedColumnNames = columns.Select(x => x.Name).ToArray();
        }

        [Test]
        public void ToDataTable_ShouldCreateDataTableWithCorrectNumberOfColumnsAndRows()
        {
            // arrange
            DataTable result = _testCases.ToDataTable();

            // assert
            AssertDataTableColumns(result, _expectedColumnNames);
            AssertDataTableRows(result, _testCases);
        }

        [Test]
        public void ToDataView_ShouldCreateDataViewWithCorrectNumberOfColumnsAndRows()
        {
            // arrange
            kCura.Data.DataView result = _testCases.ToDataView();

            // assert
            AssertDataTableColumns(result.Table, _expectedColumnNames);
            AssertDataTableRows(result.Table, _testCases);
        }

        [Test]
        public void ToDataSet_ShouldCreateDataSetWithCorrectNumberOfColumnsAndRows()
        {
            // arrange
            DataSet result = _testCases.ToDataSet();

            // assert
            result.Tables.Count.Should().Be(1);
            DataTable dataTable = result.Tables[0];
            AssertDataTableColumns(dataTable, _expectedColumnNames);
            AssertDataTableRows(dataTable, _testCases);
        }

        private static void AssertDataTableColumns(DataTable result, string[] expectedColumnsNames)
        {
            result.Columns.Count.Should().Be(expectedColumnsNames.Length);
            result.Columns
                .OfType<DataColumn>()
                .Select(x => x.ColumnName)
                .Should()
                .Contain(expectedColumnsNames);
        }

        private void AssertDataTableRows(DataTable result, TestCase[] testCases)
        {
            result.Rows.Count.Should().Be(result.Rows.Count);

            DataRow[] rows = result.Rows.OfType<DataRow>().ToArray();
            var asserts = rows.Zip(testCases, (a, e) => new
            {
                Expected = e,
                Actual = a
            });

            foreach (var assert in asserts)
            {
                TestCase expected = assert.Expected;
                DataRow actual = assert.Actual;

                actual[nameof(TestCase.ID)].Should().Be(expected.ID);
                actual[nameof(TestCase.Name)].Should().Be(expected.Name);
                actual[nameof(TestCase.Duration)].Should().Be(expected.Duration);
                actual[nameof(TestCase.Description)].Should().Be(expected.Description);
            }
        }

        private class TestCase
        {
            public int ID { get; set; }

            public string Name { get; set; }

            public double Duration { get; set; }

            public string Description { get; set; }
        }
    }
}
