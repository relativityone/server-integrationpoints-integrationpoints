using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services;
using Relativity.Services.Workspace;
using Relativity.Sync.WorkspaceGenerator.Fields;
using Relativity.Sync.WorkspaceGenerator.FileGenerating;
using Relativity.Sync.WorkspaceGenerator.FileGenerating.FileContentProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerating.FileExtensionProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerating.SizeCalculator;
using Relativity.Sync.WorkspaceGenerator.Import;
using Relativity.Sync.WorkspaceGenerator.RelativityServices;
using Relativity.Sync.WorkspaceGenerator.Settings;
using IWorkspaceService = Relativity.Sync.WorkspaceGenerator.RelativityServices.IWorkspaceService;

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
			IRelativityServicesFactory relativityServicesFactory = new RelativityServicesFactory(_settings);
			IWorkspaceService workspaceService = relativityServicesFactory.CreateWorkspaceService();
			ISavedSearchManager savedSearchManager = relativityServicesFactory.CreateSavedSearchManager();
			IRandomFieldsGenerator fieldsGenerator = new RandomFieldsGenerator();

			List<CustomField> randomFields = fieldsGenerator.GetRandomFields(_settings.TestCases).ToList();
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
			if (_settings.enabledDataGridForExtractedText)
			{
				await workspaceService
					.EnableExtractedTextFieldForDataGridAsync(workspace.ArtifactID)
					.ConfigureAwait(false);
			}
			

			DirectoryInfo dataDir = new DirectoryInfo(_settings.TestDataDirectoryPath);
			DirectoryInfo nativesDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"NATIVES"));
			DirectoryInfo textDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"TEXT"));

			try
			{
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
					IDataReaderProvider dataReaderProvider = new DataReaderProvider(documentFactory, testCase, _settings.BatchSize);

					ImportHelper importHelper = new ImportHelper(workspaceService, dataReaderProvider, _settings, testCase);
					IList<ImportJobResult> results = await importHelper.ImportDataAsync(workspace.ArtifactID).ConfigureAwait(false);

					var errorResults = results.Where(x => !x.Success);
					if (!errorResults.Any())
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
						foreach (var result in errorResults)
						{
							foreach(string error in result.Errors)
							{
								Console.WriteLine($"Import API error: {error}");
							}
						}

						return (int)ExitCodes.OtherError;
					}
				}
			}
			finally
			{
				dataDir.Delete(recursive: true);
			}

			Console.WriteLine("\n\nFinished processing all test cases.");
			return (int)ExitCodes.OK;
		}
	}
}