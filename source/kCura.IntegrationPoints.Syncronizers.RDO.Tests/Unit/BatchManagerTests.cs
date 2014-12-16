using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.Unit
{
	[TestFixture]
	public class BatchManagerTests
	{
		[Test]
		public void ConfigureTable_Pass()
		{
			//ARRANGE
			IEnumerable<string> columnNames = new List<string>() { "F1", "F2", "F3" };
			List<IDictionary<string, object>> dataSource = new List<IDictionary<string, object>>()
			{
				new Dictionary<string, object>(){{"F1",111},{"F2",DateTime.Parse("11/22/2014 11:22:33")},{"F3","Hello"}},
				new Dictionary<string, object>(){{"F1",222},{"F2",DateTime.Parse("11/23/2014 11:22:33")},{"F3","Goodbye"}},
				new Dictionary<string, object>(){{"F1",333},{"F2",DateTime.Parse("11/24/2014 11:22:33")},{"F3","Privet"}},
			};

			BatchManager batchManager = new BatchManager();


			//ACT
			DataTable dataTable = batchManager.ConfigureTable(columnNames, dataSource);


			//ASSERT
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

	}
}
