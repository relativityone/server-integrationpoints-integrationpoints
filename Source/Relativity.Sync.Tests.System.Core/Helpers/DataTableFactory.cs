﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
	internal sealed class DataTableFactory
	{
		public static ImportDataTableWrapper GenerateDocumentsWithExtractedText(int numDocuments, string controlNumberPrefix = "RND")
		{
			var documentData = new ImportDataTableWrapper(true, false, false, false);

			Func<string> generateExtractedText = () => Guid.NewGuid().ToString();
			Func<int, string> getControlNumber = number => string.Format(CultureInfo.InvariantCulture, "{0}{1:D6}", controlNumberPrefix, number);
			Func<IEnumerable<Tuple<string, string>>> buildColumnValuePair = () => new[]
			{
				Tuple.Create(ImportDataTableWrapper.ExtractedTextFilePath, generateExtractedText())
			};

			Enumerable.Range(0, numDocuments)
				.Select(number => new { ControlNumber = getControlNumber(number), ColumnToValueMap = buildColumnValuePair() })
				.ForEach(document => documentData.AddDocument(document.ControlNumber, document.ColumnToValueMap));

			return documentData;
		}

		public static ImportDataTableWrapper GenerateDocumentWithNoFields(string controlNumberPrefix = "RND", int documentsCount = 1)
		{
			var documentData = new ImportDataTableWrapper(false, false, true, false);

			Enumerable.Range(0, documentsCount).ForEach(documentNumber => documentData.AddDocument(
				string.Format(CultureInfo.InvariantCulture, "{0}{1:D6}", controlNumberPrefix, documentNumber),
				Enumerable.Empty<Tuple<string, string>>()
			));
			
			return documentData;
		}

		public static ImportDataTableWrapper GenerateDocumentWithUserField(string controlNumberPrefix = "RND")
		{
			var documentData = new ImportDataTableWrapper(false, false, true, false);

			documentData.AddDocument(
				string.Format(CultureInfo.InvariantCulture, "{0}{1:D6}", controlNumberPrefix, 0),
				new[] { Tuple.Create(ImportDataTableWrapper.RelativitySyncTestUser, AppSettings.RelativityUserName) }
			);

			return documentData;
		}

		public static ImportDataTableWrapper CreateImportDataTable(Dataset dataset, bool extractedText = false, bool natives = false)
		{
			if (!(extractedText || natives))
			{
				throw new ArgumentException("One of the input flags must be true.");
			}

			DirectoryInfo rootFolder = new DirectoryInfo(dataset.FolderPath);
			DirectoryInfo[] subFolders = rootFolder.GetDirectories();

			DirectoryInfo extractedTextFolder = null;
			DirectoryInfo nativesFolder = null;
			List<DirectoryInfo> directories = new List<DirectoryInfo>();

			if (extractedText)
			{
				extractedTextFolder = subFolders.First(x => x.Name == "TEXT");
				directories.Add(extractedTextFolder);
			}

			if (natives)
			{
				nativesFolder = subFolders.First(x => x.Name == "NATIVES");
				directories.Add(nativesFolder);
			}

			IEnumerable<IEnumerable<string>> groupsOfControlNumbers = directories
				.Select(GetControlNumbers);

			IEnumerable<string> validControlNumbers = GetIntersectionOfEnumerables(groupsOfControlNumbers);

			ImportDataTableWrapper dataTableWrapper = new ImportDataTableWrapper(true, true, false, false);
			foreach (string controlNumber in validControlNumbers)
			{
				var columnValuePairs = new List<Tuple<string, string>>();

				// Fill extracted text columns
				if (extractedText)
				{
					FileInfo extractedTextFile = GetFileInfoFromFolder(extractedTextFolder, controlNumber);
					columnValuePairs.Add(
						Tuple.Create(ImportDataTableWrapper.ExtractedTextFilePath, extractedTextFile.FullName)
					);
				}

				// Fill native file columns
				if (natives)
				{
					FileInfo nativeFile = GetFileInfoFromFolder(nativesFolder, controlNumber);
					IEnumerable<Tuple<string, string>> nativeColumnValuePairs = new[]
					{
						Tuple.Create(ImportDataTableWrapper.FileName, nativeFile.Name),
						Tuple.Create(ImportDataTableWrapper.NativeFilePath, nativeFile.FullName),
						Tuple.Create(ImportDataTableWrapper.FolderPath, "")
					};
					columnValuePairs.AddRange(nativeColumnValuePairs);
				}

				dataTableWrapper.AddDocument(controlNumber, columnValuePairs);
			}

			return dataTableWrapper;
		}

		public static ImportDataTableWrapper CreateImageImportDataTable(Dataset dataset)
		{
			var images = dataset.GetFiles();


			ImportDataTableWrapper dataTableWrapper = new ImportDataTableWrapper(true, true, false, true);

			foreach (FileInfo imageFile in images)
			{
				var controlNumber = GetControlNumber(imageFile);

				var columnValuePairs = new List<Tuple<string, string>>
				{
					Tuple.Create(ImportDataTableWrapper.BegBates, controlNumber),
					Tuple.Create(ImportDataTableWrapper.IdentifierFieldName, controlNumber),
					Tuple.Create(ImportDataTableWrapper.ImageFile, imageFile.FullName)
				};
				
				dataTableWrapper.AddDocument(controlNumber, columnValuePairs);
			}

			return dataTableWrapper;
		}

		private static IEnumerable<string> GetIntersectionOfEnumerables(IEnumerable<IEnumerable<string>> enumerables)
		{
			IEnumerable<IEnumerable<string>> enumerablesList = enumerables.ToList();

			if (enumerablesList.Count() < 1)
			{
				return Enumerable.Empty<string>();
			}

			// Here we skip the first enumerable to use it as the initial value of Aggregate's accumulator.
			IEnumerable<string> intersectionOfLists = enumerablesList
				.Skip(1)
				.Aggregate(
					enumerablesList.First(),
					(acc, enumerable) => acc.Intersect(enumerable)
				);
			return intersectionOfLists;
		}

		private static FileInfo GetFileInfoFromFolder(DirectoryInfo subDirectory, string controlNumber)
		{
			return subDirectory
				.GetFiles()
				.First(x => Path.GetFileNameWithoutExtension(x.Name) == controlNumber);
		}

		private static IEnumerable<string> GetControlNumbers(DirectoryInfo subDirectory)
		{
			return subDirectory == null
				? Enumerable.Empty<string>()
				: subDirectory
					.GetFiles()
					.Select(GetControlNumber);
		}

		private static string GetControlNumber(FileInfo file)
		{
			return Path.GetFileNameWithoutExtension(file.Name);
		}

	}
}