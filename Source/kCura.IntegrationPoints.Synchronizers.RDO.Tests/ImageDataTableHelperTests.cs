using System.Collections.Generic;
using System.Data;
using System.Linq;

using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.IntegrationPoint.Tests.Core;

using NUnit.Framework;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
	public class ImageDataTableHelperTests : TestBase
	{
		public override void SetUp()
		{
		}

		[Test]
		public void CanHandleEmptyDataReader()
		{
			DataTable source = new DataTable();
			DataTable dest = ImageDataTableHelper.GetDataTable(source.CreateDataReader());
			Assert.AreEqual(0, dest.Columns.Count);
			Assert.AreEqual(0, dest.Rows.Count);
		}

		[TestCase("1","1")]
		[TestCase("2","3")]
		[TestCase("3","2")]
		[TestCase("4","4")]
		public void CanLoadFromDataReader(string rowCount, string colCount)
		{
			int rows = int.Parse(rowCount);
			int cols = int.Parse(colCount);
			DataTable source = BuildTestTable(rows, cols);

			DataTable dest = ImageDataTableHelper.GetDataTable(source.CreateDataReader());

			Assert.AreEqual(dest.Columns.Count, cols);
			for (int col = 0; col < cols; col++)
			{
				Assert.AreEqual(source.Columns[col].ColumnName, dest.Columns[col].ColumnName);
			}

			Assert.AreEqual(dest.Rows.Count, rows);
			for (int row = 0; row < rows; row++)
			{
				for (int col = 0; col < cols; col++)
				{
					Assert.AreEqual(CellName(row, col), dest.Rows[row][col]);
				}
			}
		}

		private DataTable BuildTestTable(int rows, int cols)
		{
			DataTable rv = new DataTable();
			for (int i = 0; i < cols; i++)
			{
				rv.Columns.Add(ColumnName(i));
			}
			for (int row = 0; row < rows; row++)
			{
				DataRow newRow = rv.NewRow();
				for (int col = 0; col < cols; col++)
				{
					newRow[ColumnName(col)] = CellName(row, col);
				}
				rv.Rows.Add(newRow);
			}
			return rv;
		}

		private string ColumnName(int i)
		{
			return string.Format("Col-{0}", i);
		}
		private string CellName(int row, int col)
		{
			return string.Format("row-{0}|col-{1}", row, col);
		}
	}
}
