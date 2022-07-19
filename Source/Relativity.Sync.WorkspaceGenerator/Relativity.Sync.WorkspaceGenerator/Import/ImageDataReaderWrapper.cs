using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Sync.WorkspaceGenerator.Fields;
using Relativity.Sync.WorkspaceGenerator.FileGenerating;
using Relativity.Sync.WorkspaceGenerator.Settings;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
    internal class ImageDataReaderWrapper : DataReaderWrapperBase
    {
        private readonly IImageGenerator _imageGenerator;
        private readonly Stack<ImageFileDTO> _documentImagesStack = new Stack<ImageFileDTO>();

        private static DataColumn[] DefaultColumns => new[]
        {
            new DataColumn(ColumnNames.ControlNumber, typeof(string)),
            new DataColumn(ColumnNames.ImageFileName, typeof(string)),
            new DataColumn(ColumnNames.BegBates, typeof(string)),
            new DataColumn(ColumnNames.ImageFilePath, typeof(string)),
        };

        public ImageDataReaderWrapper(IImageGenerator imageGenerator, IDocumentFactory documentFactory, TestCase testCase, int batchSize, int alreadyProvidedRecordCount) : base(documentFactory, testCase, batchSize, alreadyProvidedRecordCount)
        {
            _imageGenerator = imageGenerator;

            DataTable.Columns.AddRange(DefaultColumns);
        }

        public override bool Read()
        {
            if (CurrentDocumentIndex >= BatchSize)
            {
                return false;
            }

            CurrentRow?.Delete();
            var currentRowData = GetNextDataRowData();

            CurrentRow = DataTable.NewRow();
            CurrentRow[ColumnNames.ControlNumber] = currentRowData.DocumentControlNumber;
            CurrentRow[ColumnNames.ImageFileName] = currentRowData.ImageFileName;
            CurrentRow[ColumnNames.ImageFilePath] = currentRowData.ImageFilePath;
            CurrentRow[ColumnNames.BegBates] = currentRowData.BegBates;

            return true;
        }

        private ImageFileDTO GetNextDataRowData()
        {
            if (!_documentImagesStack.Any())
            {
                var document = DocumentFactory.GetDocumentAsync(CurrentDocumentIndex + AlreadyProvidedRecordCount).GetAwaiter().GetResult();

                foreach (var image in _imageGenerator.GetImagesForDocument(document))
                {
                    _documentImagesStack.Push(image);
                }

                CurrentDocumentIndex++;
            }

            return _documentImagesStack.Pop();
        }
    }
}
