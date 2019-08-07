using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;

namespace Rip.TestUtilities
{
	public class LoadFileHelper
	{
		public DocumentsTestData BuildDocumentTestDataFromLoadFile(
			string loadFilePath, 
			Type[] columnTypes, 
			char separator, 
			char quote, 
			string workspaceFolderName)
		{
			var documents = new List<FolderWithDocuments>();
			var images = new DataTable();

			using (StreamReader reader = new StreamReader(loadFilePath))
			{
				var dataTable = new DataTable();
				string columnsLine = reader.ReadLine();
				IEnumerable<string> columnNames = GetColumnValues(columnsLine, separator, quote);
				var columns = columnNames.Zip(columnTypes, (name, type) => new {Name = name, Type = type});

				foreach (var column in columns)
				{
					dataTable.Columns.Add(column.Name, column.Type);
				}

				var folder = new FolderWithDocuments(workspaceFolderName, dataTable);

				string line;
				while ((line = reader.ReadLine()) != null)
				{
					object[] columnValues = GetParsedColumnValues(line, separator, quote, columnTypes);
					folder.Documents.Rows.Add(columnValues);
				}

				documents.Add(folder);
			}

			return new DocumentsTestData(documents, images);
		}

		private static object[] GetParsedColumnValues(string line, char separator, char quote, Type[] columnTypes)
		{
			IEnumerable<string> columnValues = GetColumnValues(line, separator, quote);
			var parsedColumnValues = columnValues.Zip(columnTypes, ParseColumnValue).ToArray();
			return parsedColumnValues;
		}

		private static IEnumerable<string> GetColumnValues(string line, char separator, char quote)
		{
			return line.Split(separator).Select(x => x.Trim(quote));
		}

		private static object ParseColumnValue(string value, Type type)
		{
			if (type == typeof(bool))
			{
				return value == "Yes";
			}

			if (type == typeof(DateTime))
			{
				return DateTime.ParseExact(value, "yyyy-mm-dd", CultureInfo.InvariantCulture);
			}

			return value;
		}
	}
}
