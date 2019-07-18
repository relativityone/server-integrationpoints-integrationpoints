using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Relativity.Sync.Tests.System.Helpers
{
	internal sealed class ImportDataTableWrapper
	{
		public DataTable Data { get; }

		public bool ExtractedText { get; }
		public bool Natives { get; }

		public static string IdentifierFieldName => "Control Number";
		public static string ExtractedTextFilePath => "Extracted Text";
		public static string FileName => "File Name";
		public static string NativeFilePath => "Native File";
		public static string FolderPath => "Document Folder Path";

		private static IEnumerable<DataColumn> DefaultColumns => new[]
		{
			new DataColumn(IdentifierFieldName, typeof(string))
		};

		private static IEnumerable<DataColumn> ExtractedTextColumns => new[]
		{
			new DataColumn(ExtractedTextFilePath, typeof(string))
		};

		private static IEnumerable<DataColumn> NativesColumns => new[]
		{
			new DataColumn(FileName, typeof(string)),
			new DataColumn(NativeFilePath, typeof(string)),
			new DataColumn(FolderPath, typeof(string))
		}; 

		public IDataReader DataReader => Data.CreateDataReader();

		public ImportDataTableWrapper(bool extractedText, bool natives)
		{
			ExtractedText = extractedText;
			Natives = natives;

			Data = new DataTable();

			IEnumerable<DataColumn> dataColumns = DefaultColumns;

			if (extractedText)
			{
				dataColumns = dataColumns.Concat(ExtractedTextColumns);
			}

			if (natives)
			{
				dataColumns = dataColumns.Concat(NativesColumns);
			}

			Data.Columns.AddRange(dataColumns.ToArray());
		}

		public void AddDocument(string controlNumber, IEnumerable<Tuple<string, string>> columnNameValuePairs)
		{
			DataRow dataRow = Data.NewRow();

			dataRow[IdentifierFieldName] = controlNumber;
			columnNameValuePairs
				.ForEach(columnValuePair => dataRow[columnValuePair.Item1] = columnValuePair.Item2);

			Data.Rows.Add(dataRow);
		}
	}
}
