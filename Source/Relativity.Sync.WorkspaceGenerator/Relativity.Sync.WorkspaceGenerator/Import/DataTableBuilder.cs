using System;
using System.Collections.Generic;
using System.Data;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
	public class DataTableBuilder
	{
		private readonly bool _withNatives;
		private readonly bool _withExtractedText;

		public DataTable DataTable { get; }

		private static IEnumerable<DataColumn> DefaultColumns => new[]
		{
				new DataColumn(ColumnNames.Identifier, typeof(string))
			};

		private static IEnumerable<DataColumn> NativesColumns => new[]
		{
				new DataColumn(ColumnNames.FileName, typeof(string)),
				new DataColumn(ColumnNames.NativeFilePath, typeof(string))
			};

		private static IEnumerable<DataColumn> ExtractedTextColumns => new[]
		{
				new DataColumn(ColumnNames.ExtractedText, typeof(string))
			};

		public DataTableBuilder(bool withNatives, bool withExtractedText, List<CustomField> customFields)
		{
			_withNatives = withNatives;
			_withExtractedText = withExtractedText;
			DataTable = new DataTable();
			List<DataColumn> dataColumns = new List<DataColumn>(DefaultColumns);

			if (withNatives)
			{
				dataColumns.AddRange(NativesColumns);
			}

			if (withExtractedText)
			{
				dataColumns.AddRange(ExtractedTextColumns);
			}

			foreach (CustomField customField in customFields)
			{
				dataColumns.Add(new DataColumn(customField.Name, typeof(string)));
			}

			DataTable.Columns.AddRange(dataColumns.ToArray());
		}

		public void AddDocument(Document document)
		{
			DataRow dataRow = DataTable.NewRow();

			dataRow[ColumnNames.Identifier] = document.Identifier;

			if (_withNatives)
			{
				dataRow[ColumnNames.FileName] = document.NativeFile.Name;
				dataRow[ColumnNames.NativeFilePath] = document.NativeFile.FullName;
			}

			if (_withExtractedText)
			{
				dataRow[ColumnNames.ExtractedText] = document.ExtractedTextFile.FullName;
			}

			foreach (Tuple<string, string> customFieldValuePair in document.CustomFields)
			{
				dataRow[customFieldValuePair.Item1] = customFieldValuePair.Item2;
			}

			DataTable.Rows.Add(dataRow);
		}

		public void AddDocuments(List<Document> documents)
		{
			Console.WriteLine($"Adding {documents.Count} documents to data table");
			foreach (Document document in documents)
			{
				AddDocument(document);
			}
		}
	}
}