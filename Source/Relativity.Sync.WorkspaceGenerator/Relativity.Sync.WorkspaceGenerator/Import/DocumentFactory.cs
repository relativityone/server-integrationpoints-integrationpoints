using System;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services;
using Relativity.Sync.WorkspaceGenerator.Fields;
using Relativity.Sync.WorkspaceGenerator.FileGenerating;
using Relativity.Sync.WorkspaceGenerator.Settings;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
    public class DocumentFactory : IDocumentFactory
    {
        private readonly TestCase _testCase;
        private readonly IFileGenerator _nativeSingleFileGenerator;
        private readonly IFileGenerator _extractedTextSingleFileGenerator;
        private readonly Random _random;
        private readonly Guid[] _documentsInBatchIds;

        public DocumentFactory(TestCase testCase, IFileGenerator nativeSingleFileGenerator, IFileGenerator extractedTextSingleFileGenerator)
        {
            _testCase = testCase;
            _nativeSingleFileGenerator = nativeSingleFileGenerator;
            _extractedTextSingleFileGenerator = extractedTextSingleFileGenerator;
            _random = new Random();
            _documentsInBatchIds =
                Enumerable.Range(0, testCase.NumberOfDocuments).Select(x => Guid.NewGuid()).ToArray();
        }

        public async Task<Document> GetDocumentAsync(int index)
        {
            if (index >= _testCase.NumberOfDocuments)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index must be less than number of documents in the test case: {_testCase.Name} - {_testCase.NumberOfDocuments}");
            }

            Document document = new Document(_documentsInBatchIds[index], _testCase.Name);

            if (_testCase.GenerateNatives)
            {
                document.NativeFile = await _nativeSingleFileGenerator
                    .GenerateAsync()
                    .ConfigureAwait(false);
            }

            if (_testCase.GenerateExtractedText)
            {
                document.ExtractedTextFile = await _extractedTextSingleFileGenerator
                    .GenerateAsync()
                    .ConfigureAwait(false);
            }

            foreach (CustomField field in _testCase.Fields)
            {
                document.CustomFields.Add(new Tuple<string, string>(field.Name, GetFieldValue(field)));
            }

            return document;
        }

        private string GetFieldValue(CustomField field)
        {
            switch (field.Type)
            {
                case FieldType.Date:
                    return GetDate();
                case FieldType.WholeNumber:
                    return GetWholeNumber();
                case FieldType.Decimal:
                    return GetDecimal();
                case FieldType.Currency:
                    return GetCurrency();
                case FieldType.FixedLengthText:
                    return GetFixedLengthText();
                case FieldType.YesNo:
                    return GetYesNo();
                default:
                    throw new Exception($"Fied type not supported: {field.Type}");
            }
        }

        private string GetDate()
        {
            DateTime randomDate = DateTime.Now.Subtract(TimeSpan.FromSeconds(_random.Next(0, int.MaxValue)));
            return randomDate.ToString();
        }

        private string GetWholeNumber()
        {
            return _random.Next(int.MinValue, int.MaxValue).ToString();
        }

        private string GetYesNo()
        {
            int random = _random.Next(0, 2);
            return random == 0 ? "No" : "Yes";
        }

        private string GetFixedLengthText()
        {
            return string.Concat(Enumerable.Repeat(".", 255));
        }

        private string GetCurrency()
        {
            return Math.Round(_random.NextDouble(), 2).ToString();
        }

        private string GetDecimal()
        {
            return Math.Round(_random.NextDouble(), 4).ToString();
        }
    }
}