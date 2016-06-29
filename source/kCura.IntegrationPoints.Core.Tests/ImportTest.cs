using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace kCura.IntegrationPoints.Core.Tests.Integration
{
	[TestFixture]
	[Category("Integration Tests")]
	public class ImportTest : SingleWorkspaceTestTemplate
	{
		private IObjectTypeRepository _objectTypeRepository;
		private IWebDriver _webDriver;

		private int _userCreated;
		private string _email;
		private int _groupId;

		public ImportTest(): base("Import Test")
		{
		}

		[Test]
		public void TestingImportSadFace()
		{
			Import.ImportNewDocuments(WorkspaceArtifactId, GetImportTable("ImportDoc", 30));
		}

		private DataTable GetImportTable(string documentPrefix, int numberOfDocuments)
		{
			DataTable table = new DataTable();
			table.Columns.Add("Control Number", typeof(string));
			table.Columns.Add("Date Sent", typeof(DateTime));

			for (int index = 1; index <= numberOfDocuments; index++)
			{
				string controlNumber = $"{documentPrefix}{index}";
				table.Rows.Add(controlNumber, DateTime.Now);
			}
			return table;
		}
	}
}