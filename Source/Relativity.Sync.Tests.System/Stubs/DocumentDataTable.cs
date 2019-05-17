using System;
using System.Data;
using System.Globalization;

namespace Relativity.Sync.Tests.System.Stubs
{
	internal sealed class DocumentData
	{
		public DataTable Data { get; }

		public IDataReader DataReader => Data.CreateDataReader();

		public DocumentData()
		{
			Data = new DataTable();
			Data.Columns.Add("Control Number", typeof(string));
			Data.Columns.Add("Extracted Text", typeof(string));
		}

		public void AddDocument(string controlNumber, string extractedText)
		{
			Data.Rows.Add(controlNumber, extractedText);
		}

		public static DocumentData GenerateDocumentsWithoutNatives(int numDocuments, string controlNumberPrefix = "RND")
		{
			var documentData = new DocumentData();
			for (int i = 0; i < numDocuments; i++)
			{
				string controlNumber = string.Format(CultureInfo.InvariantCulture, "{0}{1:D6}", controlNumberPrefix, i);
				string extractedText = Guid.NewGuid().ToString();
				documentData.AddDocument(controlNumber, extractedText);
			}
			return documentData;
		}
	}
}
