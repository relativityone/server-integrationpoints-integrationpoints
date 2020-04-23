﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services;
using Relativity.Services.Workspace;
using Relativity.Sync.WorkspaceGenerator.FileGenerating;
using Relativity.Sync.WorkspaceGenerator.FileGenerating.FileContentProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerating.FileExtensionProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerating.SizeCalculator;
using Relativity.Sync.WorkspaceGenerator.Import;
using Relativity.Sync.WorkspaceGenerator.RelativityServices;
using Relativity.Sync.WorkspaceGenerator.SavedSearch;
using Relativity.Sync.WorkspaceGenerator.Settings;

namespace Relativity.Sync.WorkspaceGenerator
{
	public class WorkspaceGeneratorRunner
	{
		private readonly GeneratorSettings _settings;

		public WorkspaceGeneratorRunner(GeneratorSettings settings)
		{
			_settings = settings;
		}

		public async Task<int> RunAsync()
		{
			RelativityServicesFactory relativityServicesFactory = new RelativityServicesFactory(_settings);
			WorkspaceService workspaceService = relativityServicesFactory.CreateWorkspaceService();
			SavedSearchManager savedSearchManager = relativityServicesFactory.CreateSavedSearchManager();

			List<CustomField> randomFields = new RandomFieldsGenerator().GetRandomFields(_settings.TestCases).ToList();
			List<CustomField> fieldsToCreate = new List<CustomField>(randomFields)
			{
				new CustomField(ColumnNames.NativeFilePath, FieldType.FixedLengthText)
			};

			WorkspaceRef workspace = await workspaceService
				.CreateWorkspaceAsync(_settings.DesiredWorkspaceName, _settings.TemplateWorkspaceName)
				.ConfigureAwait(false);
			await workspaceService
				.CreateFieldsAsync(workspace.ArtifactID, fieldsToCreate)
				.ConfigureAwait(false);

			DirectoryInfo dataDir = new DirectoryInfo(_settings.TestDataDirectoryPath);
			DirectoryInfo nativesDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"NATIVES"));
			DirectoryInfo textDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"TEXT"));

			foreach (TestCase testCase in _settings.TestCases)
			{
				Console.WriteLine($"Importing documents for test case: {testCase.Name}");

				testCase.Fields = randomFields.GetRange(0, testCase.NumberOfFields);

				IFileGenerator nativesGenerator = new SingleFileGenerator(
					new RandomNativeFileExtensionProvider(),
					new NativeFileContentProvider(),
					new EqualFileSizeCalculatorStrategy(testCase.NumberOfDocuments, testCase.TotalNativesSizeInMB).GetNext(),
					nativesDir);
				IFileGenerator textGenerator = new SingleFileGenerator(
					new TextFileExtensionProvider(),
					new AsciiExtractedTextFileContentProvider(),
					new EqualFileSizeCalculatorStrategy(testCase.NumberOfDocuments, testCase.TotalExtractedTextSizeInMB).GetNext(),
					textDir);

				IDocumentFactory documentFactory = new DocumentFactory(testCase, nativesGenerator, textGenerator);
				IDataReader dataReader = new DataReaderWrapper(documentFactory, testCase);

				ImportHelper importHelper = new ImportHelper(workspaceService, _settings, testCase);
				ImportJobResult result = await importHelper.ImportDataAsync(workspace.ArtifactID, dataReader).ConfigureAwait(false);

				if (result.Success)
				{
					Console.WriteLine($"Successfully imported documents for test case: {testCase.Name}");

					try
					{
						Console.WriteLine($"Creating saved search: {testCase.Name}");
						await savedSearchManager.CreateSavedSearchForTestCaseAsync(workspace.ArtifactID, testCase.Name)
							.ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Failed to create saved search:\r\n{ex}");
					}
				}
				else
				{
					foreach (string error in result.Errors)
					{
						Console.WriteLine($"Import API error: {error}");
					}

					return (int)ExitCodes.OtherError;
				}
			}

			Console.WriteLine("\n\nFinished processing all test cases.\r\nPress [Enter] to exit");
			Console.ReadLine();
			return (int)ExitCodes.OK;
		}
	}
}