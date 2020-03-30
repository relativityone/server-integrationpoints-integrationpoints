﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services;
using Relativity.Sync.WorkspaceGenerator.FileGenerator;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.SizeCalculator;
using Relativity.Sync.WorkspaceGenerator.LoadFileGenerator;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
	class DocumentFactory
	{
		private readonly GeneratorSettings _settings;
		private readonly IFileGenerator _nativeFileGenerator;
		private readonly IFileGenerator _extractedTextFileGenerator;

		private readonly List<long> _nativesSizes;
		private readonly List<long> _extractedTextSizes;

		private readonly Random _random;

		public DocumentFactory(GeneratorSettings settings, IFileSizeCalculatorStrategy nativesSizeCalculatorStrategy, IFileSizeCalculatorStrategy extractedTextSizeCalculatorStrategy, IFileGenerator nativeFileGenerator, IFileGenerator extractedTextFileGenerator)
		{
			_settings = settings;
			_nativeFileGenerator = nativeFileGenerator;
			_extractedTextFileGenerator = extractedTextFileGenerator;

			_nativesSizes = nativesSizeCalculatorStrategy.GetSizesInBytes(settings.NumberOfDocuments, settings.TotalNativesSizeInMB).ToList();
			_extractedTextSizes = extractedTextSizeCalculatorStrategy.GetSizesInBytes(settings.NumberOfDocuments, settings.TotalExtractedTextSizeInMB).ToList();

			_random = new Random();
		}

		public async Task<IEnumerable<Document>> GenerateDocumentsAsync(List<CustomField> fields)
		{
			List<Document> documents = new List<Document>();

			for (int i = 0; i < _settings.NumberOfDocuments; i++)
			{
				Document document = new Document();
				Console.WriteLine($"Generating document: {document.Identifier}");

				if (_settings.GenerateNatives)
				{
					document.NativeFile = await _nativeFileGenerator
						.GenerateAsync(document.Identifier, _nativesSizes[i])
						.ConfigureAwait(false);
				}

				if (_settings.GenerateExtractedText)
				{
					document.ExtractedTextFile = await _extractedTextFileGenerator
						.GenerateAsync(document.Identifier, _extractedTextSizes[i])
						.ConfigureAwait(false);
				}

				foreach (CustomField field in fields)
				{
					document.CustomFields.Add(new Tuple<string, string>(field.Name, GetFieldValue(field)));
				}

				documents.Add(document);
			}

			return documents;
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
			return _random.NextDouble().ToString();
		}

		private string GetDecimal()
		{
			return _random.NextDouble().ToString();
		}
	}
}