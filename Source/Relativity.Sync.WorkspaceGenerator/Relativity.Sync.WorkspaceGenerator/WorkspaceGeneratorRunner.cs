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
            if (workspace is null)
            {
                return (int)ExitCodes.OtherError;
            }

            List<CustomField> workspaceFields = await PrepareWorkspaceFieldsAsync(workspaceService, workspace).ConfigureAwait(false);
            if (workspaceFields is null)
            {
                return (int)ExitCodes.OtherError;
            }

            DirectoryInfo dataDir = Directory.CreateDirectory(_settings.TestDataDirectoryPath);
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
            WorkspaceRef workspace = null;

            if (_settings.Append)
            {
                workspace = await workspaceService.GetWorkspaceAsync(_settings.DesiredWorkspaceName)
                    .ConfigureAwait(false);

                if (workspace is null)
                {
                    Console.WriteLine($"Cannot find workspace name: '{_settings.DesiredWorkspaceName}'");
                }
                else if (_settings.EnabledDataGridForExtractedText != await workspaceService.GetExtractedTextFieldEnableForDataGridAsync(workspace.ArtifactID).ConfigureAwait(false))
                {
                    Console.WriteLine($"Workspace '{_settings.DesiredWorkspaceName}' Data Grid configuration for Extracted Text not consistent with settings.");

                    workspace = null;
                }
            }
            else
            {
                workspace = await workspaceService.CreateWorkspaceAsync(_settings.DesiredWorkspaceName, _settings.TemplateWorkspaceName)
                    .ConfigureAwait(false);

                if (_settings.EnabledDataGridForExtractedText)
                {
                    await workspaceService.EnableExtractedTextFieldForDataGridAsync(workspace.ArtifactID)
                        .ConfigureAwait(false);
                }
            }

            return workspace;
        }

        private async Task<List<CustomField>> PrepareWorkspaceFieldsAsync(IWorkspaceService workspaceService, WorkspaceRef workspace)
        {
            List<CustomField> workspaceFields = null;

            if (_settings.Append)
            {
                workspaceFields = await workspaceService.GetAllNonSystemDocumentFieldsAsync(workspace.ArtifactID)
                    .ConfigureAwait(false);

                CustomField nativeFilePath = workspaceFields.Find(cf => cf.Name == ColumnNames.NativeFilePath);

                if (nativeFilePath is null)
                {
                    Console.WriteLine($"The '{ColumnNames.NativeFilePath}' field doesn't exist in '{_settings.DesiredWorkspaceName}'");
                    return null;
                }

                workspaceFields.Remove(nativeFilePath);

                if (workspaceFields.Count != _settings.TestCases.Max(x => x.NumberOfFields))
                {
                    Console.WriteLine($"The number of existing fields in '{_settings.DesiredWorkspaceName}' workspace is different than number of fields in settings");
                    workspaceFields = null;
                }
            }
            else
            {
                IRandomFieldsGenerator fieldsGenerator = new RandomFieldsGenerator();
                workspaceFields = fieldsGenerator.GetRandomFields(_settings.TestCases).ToList();

                List<CustomField> fieldsToCreate = new List<CustomField>(workspaceFields)
                {
                    new CustomField(ColumnNames.NativeFilePath, FieldType.FixedLengthText)
                };
                await workspaceService.CreateFieldsAsync(workspace.ArtifactID, fieldsToCreate)
                    .ConfigureAwait(false);
            }

            return workspaceFields;
        }

        private async Task<ExitCodes> ImportTestCaseDocumentsAsync(
            IRelativityServicesFactory relativityServicesFactory, IWorkspaceService workspaceService,
            WorkspaceRef workspace, List<CustomField> workspaceFields, TestCase testCase,
            DirectoryInfo nativesDir, DirectoryInfo textDir)
        {
            ExitCodes exitCode = ExitCodes.OK;

            int savedSearchId = await PrepareSavedSearchAsync(relativityServicesFactory, workspace, testCase).ConfigureAwait(false);
            if (savedSearchId < 0)
            {
                return ExitCodes.OtherError;
            }
            
            int documentsToImportCount =
                await GetDocumentsToImportCountAsync(relativityServicesFactory, workspace, savedSearchId, testCase)
                    .ConfigureAwait(false);

            if (documentsToImportCount <= 0)
            {
                Console.WriteLine($"All documents for test case '{testCase.Name}' already imported");
                return exitCode;
            }

            IDataReaderProvider dataReaderProvider =
                PrepareTestCaseDataReaderProvider(testCase, documentsToImportCount, nativesDir, textDir);

            ImportHelper importHelper =
                new ImportHelper(workspaceService, dataReaderProvider, _settings, testCase);
            
            IList<ImportJobResult> results = null;
            if (testCase.GenerateImages)
            {
                results = await GenerateImagesAsync(importHelper, relativityServicesFactory, workspace.ArtifactID, testCase).ConfigureAwait(false);
            }
            else
            {
                results = await GenerateDocumentsAsync(importHelper, workspace.ArtifactID, workspaceFields, testCase).ConfigureAwait(false);
            }

            return ParseResults(testCase, results, exitCode);
        }

        private static ExitCodes ParseResults(TestCase testCase, IList<ImportJobResult> results, ExitCodes exitCode)
        {
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

        private async Task<IList<ImportJobResult>> GenerateImagesAsync(ImportHelper importHelper, IRelativityServicesFactory relativityServicesFactory, int workspaceId, TestCase testCase)
        {
            int? productionId = null;

            if (!string.IsNullOrEmpty(testCase.ProductionName))
            {
                var productionService = relativityServicesFactory.CreateProductionService();
                productionId = await productionService
                    .GetProductionIdAsync(workspaceId, testCase.ProductionName)
                    .ConfigureAwait(false);

                if (productionId == null)
                {
                    if (_settings.Append)
                    {
                        Console.WriteLine($"Production {testCase.ProductionName} does not exist - cannot append");
                        return new List<ImportJobResult>();
                    }

                    productionId = await productionService.CreateProductionAsync(workspaceId, testCase.ProductionName).ConfigureAwait(false);
                }
            }

            IEnumerable<ImportJobResult> imageResults =
                await importHelper.ImportImagesAsync(workspaceId, productionId).ConfigureAwait(false);

            return imageResults.ToList();
        }

        private async Task<IList<ImportJobResult>> GenerateDocumentsAsync(ImportHelper importHelper, int workspaceId, List<CustomField> workspaceFields, TestCase testCase)
        {
            IList<ImportJobResult> results;
            Console.WriteLine($"Importing documents for test case: {testCase.Name}");

            testCase.Fields = workspaceFields.GetRange(0, testCase.NumberOfFields);

            results = await importHelper.ImportDataAsync(workspaceId).ConfigureAwait(false);
            return results;
        }

        private async Task<int> PrepareSavedSearchAsync(IRelativityServicesFactory relativityServicesFactory, WorkspaceRef workspace, TestCase testCase)
        {
            int savedSearchId = -1;

            ISavedSearchManager savedSearchManager = relativityServicesFactory.CreateSavedSearchManager();

            if (_settings.Append)
            {
                Console.WriteLine($"Retrieving saved search: {testCase.Name}");

                try
                {
                    savedSearchId = await savedSearchManager.GetSavedSearchIdForTestCaseAsync(workspace.ArtifactID, testCase.Name)
                        .ConfigureAwait(false) ?? -1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to retrieve saved search:\r\n{ex}");
                }
            }

            if (savedSearchId == -1)
            {
                Console.WriteLine($"Creating saved search: {testCase.Name}");

                try
                {
                    savedSearchId = await savedSearchManager.CreateSavedSearchForTestCaseAsync(workspace.ArtifactID, testCase.Name)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create saved search:\r\n{ex}");
                }
            }

            return savedSearchId;
        }

        private async Task<int> GetDocumentsToImportCountAsync(IRelativityServicesFactory relativityServicesFactory, WorkspaceRef workspace, int savedSearchId, TestCase testCase)
        {
            ISavedSearchManager savedSearchManager = relativityServicesFactory.CreateSavedSearchManager();

            return testCase.NumberOfDocuments - (await savedSearchManager.CountSavedSearchDocumentsAsync(workspace.ArtifactID, savedSearchId).ConfigureAwait(false));
        }

        private IDataReaderProvider PrepareTestCaseDataReaderProvider(TestCase testCase, int documentsToImportCount, DirectoryInfo nativesDir, DirectoryInfo textDir)
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

            IImageGenerator imageGenerator = new ImageGenerator(testCase.TotalImagesSizeInMB, testCase.NumberOfDocuments);

            IDocumentFactory documentFactory = new DocumentFactory(testCase, nativesGenerator, textGenerator);

            return new DataReaderProvider(documentFactory, imageGenerator, testCase, documentsToImportCount, _settings.BatchSize);
        }
    }
}