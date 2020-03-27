using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Relativity.Services;

namespace Relativity.Sync.WorkspaceGenerator.LoadFileGenerator
{
	public class LoadFileGenerator
	{
		private const string Quote = "^";
		private const string Delimiter = "|";

		private readonly DirectoryInfo _destinationDirectory;
		private readonly Random _random;

		public LoadFileGenerator(DirectoryInfo destinationDirectory)
		{
			_destinationDirectory = destinationDirectory;
			_random = new Random();
		}

		public void GenerateLoadFile(List<Document> documents, List<CustomField> fields)
		{
			FileInfo loadFile = new FileInfo(Path.Combine(_destinationDirectory.FullName, $"LoadFile-{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.dat"));
			string header = FormatHeader(fields);
			File.AppendAllText(loadFile.FullName, header + Environment.NewLine);

			List<string> documentLines = new List<string>(documents.Count);
			foreach (Document document in documents)
			{
				documentLines.Add(FormatDocumentLine(document.NativeFile, document.ExtractedTextFile, fields));
			}

			File.AppendAllLines(loadFile.FullName, documentLines);
		}

		private string FormatHeader(IEnumerable<CustomField> fields)
		{
			return FormatLine(fields.Select(x => x.Name));
		}

		private string FormatLine(IEnumerable<string> values)
		{
			string[] fieldNamesWithQuotes = values
				.Select(value => $"{Quote}{value}{Quote}")
				.ToArray();
			return string.Join(Delimiter, fieldNamesWithQuotes);
		}

		private string FormatDocumentLine(FileInfo nativeFile, FileInfo extractedTextFile, List<CustomField> fields)
		{
			List<string> fieldValues = new List<string>(fields.Count);
			foreach (CustomField field in fields)
			{
				if (field.Name == "Control Number")
				{
					fieldValues.Add(Guid.NewGuid().ToString());
					continue;
				}

				if (field.Name == "Extracted Text")
				{
					fieldValues.Add(GetRelativePath(extractedTextFile.FullName));
					continue;
				}

				if (field.Name == "Native File Path")
				{
					fieldValues.Add(GetRelativePath(nativeFile.FullName));
					continue;
				}

				fieldValues.Add(GetFieldValue(field));
			}

			return FormatLine(fieldValues);
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
			return random == 0 ? "No" : "yes";
		}

		public string GetFixedLengthText()
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

		private string GetRelativePath(string filePath)
		{
			string relativePath = filePath.Replace(_destinationDirectory.FullName, string.Empty);
			return $@".\{relativePath}";
		}
	}
}