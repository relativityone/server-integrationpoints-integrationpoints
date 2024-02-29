using System;
using System.Collections.Generic;
using System.Data;
using Relativity.Sync.WorkspaceGenerator.Fields;
using Relativity.Sync.WorkspaceGenerator.Settings;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
    public class DocumentDataReaderWrapper : DataReaderWrapperBase
    {
        private static IEnumerable<DataColumn> DefaultColumns => new[]
        {
            new DataColumn(ColumnNames.ControlNumber, typeof(string))
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

        public DocumentDataReaderWrapper(IDocumentFactory documentFactory, TestCase testCase, int batchSize, int alreadyProvidedRecordCount) 
            : base(documentFactory, testCase, batchSize, alreadyProvidedRecordCount)
        {
            List<DataColumn> dataColumns = new List<DataColumn>(DefaultColumns);

            if (TestCase.GenerateNatives)
            {
                dataColumns.AddRange(NativesColumns);
            }

            if (TestCase.GenerateExtractedText)
            {
                dataColumns.AddRange(ExtractedTextColumns);
            }

            foreach (CustomField customField in TestCase.Fields)
            {
                dataColumns.Add(new DataColumn(customField.Name, typeof(string)));
            }

            DataTable.Columns.AddRange(dataColumns.ToArray());
        }

        public override bool Read()
        {
            if (CurrentDocumentIndex >= BatchSize)
            {
                return false;
            }

            CurrentRow?.Delete();
            CurrentDocument = DocumentFactory.GetDocumentAsync(CurrentDocumentIndex + AlreadyProvidedRecordCount).GetAwaiter().GetResult();

            CurrentRow = DataTable.NewRow();
            CurrentRow[ColumnNames.ControlNumber] = CurrentDocument.Identifier;

            if (TestCase.GenerateNatives)
            {
                CurrentRow[ColumnNames.FileName] = CurrentDocument.NativeFile.Name;
                CurrentRow[ColumnNames.NativeFilePath] = CurrentDocument.NativeFile.FullName;
            }

            if (TestCase.GenerateExtractedText)
            {
                CurrentRow[ColumnNames.ExtractedText] = CurrentDocument.ExtractedTextFile.FullName;
            }

            foreach (Tuple<string, string> customFieldValuePair in CurrentDocument.CustomFields)
            {
                CurrentRow[customFieldValuePair.Item1] = customFieldValuePair.Item2;
            }

            CurrentDocumentIndex++;

            return true;
        }


    }
}