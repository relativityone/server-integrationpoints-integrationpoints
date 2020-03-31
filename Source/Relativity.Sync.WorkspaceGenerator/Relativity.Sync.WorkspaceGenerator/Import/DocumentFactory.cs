using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services;
using Relativity.Sync.WorkspaceGenerator.FileGenerating;
using Relativity.Sync.WorkspaceGenerator.FileGenerating.SizeCalculator;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
	public class DocumentFactory : IDocumentFactory
	{
		private readonly GeneratorSettings _settings;
		private readonly FileGenerator _nativeFileGenerator;
		private readonly FileGenerator _extractedTextFileGenerator;
		private readonly List<CustomField> _customFields;

		private readonly List<long> _nativesSizes;
		private readonly List<long> _extractedTextSizes;

		private readonly Random _random;

		private int _currentDocumentIndex = 0;

		public DocumentFactory(GeneratorSettings settings, IFileSizeCalculatorStrategy nativesSizeCalculatorStrategy, IFileSizeCalculatorStrategy extractedTextSizeCalculatorStrategy, FileGenerator nativeFileGenerator, FileGenerator extractedTextFileGenerator, List<CustomField> customFields)
		{
			_settings = settings;
			_nativeFileGenerator = nativeFileGenerator;
			_extractedTextFileGenerator = extractedTextFileGenerator;
			_customFields = customFields;

			_nativesSizes = nativesSizeCalculatorStrategy.GetSizesInBytes(settings.NumberOfDocuments, settings.TotalNativesSizeInMB).ToList();
			_extractedTextSizes = extractedTextSizeCalculatorStrategy.GetSizesInBytes(settings.NumberOfDocuments, settings.TotalExtractedTextSizeInMB).ToList();

			_random = new Random();
		}

		public async Task<Document> GetNextDocumentAsync()
		{
			if (_currentDocumentIndex >= _settings.NumberOfDocuments)
			{
				return null;
			}

			Document document = new Document(Guid.NewGuid().ToString());
			Console.WriteLine($"Generating document: {document.Identifier}");

			if (_settings.GenerateNatives)
			{
				document.NativeFile = await _nativeFileGenerator
					.GenerateAsync(document.Identifier, _nativesSizes[_currentDocumentIndex])
					.ConfigureAwait(false);
			}

			if (_settings.GenerateExtractedText)
			{
				document.ExtractedTextFile = await _extractedTextFileGenerator
					.GenerateAsync(document.Identifier, _extractedTextSizes[_currentDocumentIndex])
					.ConfigureAwait(false);
			}

			foreach (CustomField field in _customFields)
			{
				document.CustomFields.Add(new Tuple<string, string>(field.Name, GetFieldValue(field)));
			}

			_currentDocumentIndex++;

			return document;
		}

		private string GetFieldValue(CustomField field)
		{
			switch (field.Type)
			{
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

		private string GetWholeNumber()
		{
			return _random.Next(-99999, 99999).ToString();
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