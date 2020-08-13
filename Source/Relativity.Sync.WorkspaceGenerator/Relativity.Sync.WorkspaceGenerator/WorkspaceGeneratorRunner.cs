using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.Vendor.Castle.Core.Internal;
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
			
			WorkspaceRef workspace = await PrepareWorkspaceAsync(workspaceService).ConfigureAwait(false);
			List<CustomField> workspaceFields = await PrepareWorkspaceFieldsAsync(workspaceService, workspace).ConfigureAwait(false);

			DirectoryInfo dataDir = new DirectoryInfo(_settings.TestDataDirectoryPath);
			DirectoryInfo nativesDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"NATIVES"));
			DirectoryInfo textDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"TEXT"));

			try
			{
				foreach (TestCase testCase in _settings.TestCases)
				{
					ExitCodes exitCode = await ImportTestCaseDocumentsAsync(
							relativityServicesFactory, workspaceService,
							workspace, workspaceFields, testCase,
							nativesDir, textDir)
						.ConfigureAwait(false);

					if (exitCode != ExitCodes.OK)
					{
						return (int)exitCode;
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

		private async Task<WorkspaceRef> PrepareWorkspaceAsync(IWorkspaceService workspaceService)
		{
			if (_settings.Append)
			{
				throw new NotImplementedException("AppendToWorkspace command line parameter is not supported yet");
			}

			WorkspaceRef workspace = await workspaceService.CreateWorkspaceAsync(_settings.DesiredWorkspaceName, _settings.TemplateWorkspaceName)
				.ConfigureAwait(false);

			if (_settings.EnabledDataGridForExtractedText)
			{
				await workspaceService.EnableExtractedTextFieldForDataGridAsync(workspace.ArtifactID)
					.ConfigureAwait(false);
			}

			return workspace;
		}

		private async Task<List<CustomField>> PrepareWorkspaceFieldsAsync(IWorkspaceService workspaceService, WorkspaceRef workspace)
		{
			if (_settings.Append)
			{
				throw new NotImplementedException("AppendToWorkspace command line parameter is not supported yet");
			}

			IRandomFieldsGenerator fieldsGenerator = new RandomFieldsGenerator();
			List<CustomField> workspaceFields = fieldsGenerator.GetRandomFields(_settings.TestCases).ToList();

			List<CustomField> fieldsToCreate = new List<CustomField>(workspaceFields)
			{
				new CustomField(ColumnNames.NativeFilePath, FieldType.FixedLengthText)
			};
			await workspaceService.CreateFieldsAsync(workspace.ArtifactID, fieldsToCreate)
				.ConfigureAwait(false);

			return workspaceFields;
		}

		private async Task<ExitCodes> ImportTestCaseDocumentsAsync(
			IRelativityServicesFactory relativityServicesFactory, IWorkspaceService workspaceService,
			WorkspaceRef workspace, List<CustomField> workspaceFields, TestCase testCase, 
			DirectoryInfo nativesDir, DirectoryInfo textDir)
		{
			ExitCodes exitCode = ExitCodes.OK;

			int savedSearchId = await PrepareSavedSearchAsync(relativityServicesFactory, workspace, testCase).ConfigureAwait(false))
			if (savedSearchId < 0)
			{
				return ExitCodes.OtherError;
			}

			Console.WriteLine($"Importing documents for test case: {testCase.Name}");

			testCase.Fields = workspaceFields.GetRange(0, testCase.NumberOfFields);

			IDataReaderProvider dataReaderProvider = PrepareTestCaseDataReaderProvider(testCase, savedSearchId, nativesDir, textDir);

			ImportHelper importHelper = new ImportHelper(workspaceService, dataReaderProvider, _settings, testCase);
			IList<ImportJobResult> results = await importHelper.ImportDataAsync(workspace.ArtifactID).ConfigureAwait(false);

			ImportJobResult[] errorResults = results.Where(x => !x.Success).ToArray();
			if (errorResults.Length == 0)
			{
				Console.WriteLine($"Successfully imported documents for test case: {testCase.Name}");
			}
			else
			{
				foreach (ImportJobResult errorResult in errorResults)
				{
					errorResult.Errors.ForEach(error => Console.WriteLine($"Import API error: {error}"));
				}

				exitCode = ExitCodes.OtherError;
			}

			return exitCode;
		}

		private async Task<int> PrepareSavedSearchAsync(IRelativityServicesFactory relativityServicesFactory, WorkspaceRef workspace, TestCase testCase)
		{
			int savedSearchId = -1;

			if (_settings.Append)
			{
				throw new NotImplementedException("AppendToWorkspace command line parameter is not supported yet");
			}

			Console.WriteLine($"Creating saved search: {testCase.Name}");

			try
			{
				ISavedSearchManager savedSearchManager = relativityServicesFactory.CreateSavedSearchManager();

				savedSearchId = await savedSearchManager.CreateSavedSearchForTestCaseAsync(workspace.ArtifactID, testCase.Name)
					.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to create saved search:\r\n{ex}");
			}

			return savedSearchId;
		}

		private IDataReaderProvider PrepareTestCaseDataReaderProvider(TestCase testCase, int savedSearchId, DirectoryInfo nativesDir, DirectoryInfo textDir)
		{
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

			if (_settings.Append)
			{
				throw new NotImplementedException("AppendToWorkspace command line parameter is not supported yet");
			}

			return new DataReaderProvider(documentFactory, testCase, _settings.BatchSize);
		}
	}
}