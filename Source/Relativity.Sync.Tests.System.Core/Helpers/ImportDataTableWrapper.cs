using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
	internal sealed class ImportDataTableWrapper
	{
		public DataTable Data { get; }

		public bool ExtractedText { get; }
		public bool Natives { get; }
		public bool Images { get; }

		public static string IdentifierFieldName => "Control Number";
		public static string ExtractedTextFilePath => "Extracted Text";
		public static string FileName => "File Name";
		public static string NativeFilePath => "Native File";
		public static string FolderPath => "Document Folder Path";
		public static string RelativitySyncTestUser => "Relativity Sync Test User";
		public static string ImageFile => "File";
		public static string BegBates => "Bates Beg";
        public static string SyncMultiChoice => "SyncMultiChoice";

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

		private static IEnumerable<DataColumn> ImagesColumns => new[]
		{
			new DataColumn(ImageFile, typeof(string)),
			new DataColumn(BegBates, typeof(string)),
		};

		private static IEnumerable<DataColumn> UserColumns => new[]
		{
			new DataColumn(RelativitySyncTestUser, typeof(string))
		};

        private static IEnumerable<DataColumn> SyncMultiChoiceColumns => new[]
        {
            new DataColumn(SyncMultiChoice, typeof(string))
        };

		public IDataReader DataReader => Data.CreateDataReader();

		public ImportDataTableWrapper(bool extractedText, bool natives, bool user, bool images, bool multiChoice)
		{
			ExtractedText = extractedText;
			Natives = natives;
			Images = images;

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

			if (images)
			{
				dataColumns = dataColumns.Concat(ImagesColumns);
			}

			if (user)
			{
				dataColumns = dataColumns.Concat(UserColumns);
			}

            if (multiChoice)
            {
                dataColumns = dataColumns.Concat(SyncMultiChoiceColumns);
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
