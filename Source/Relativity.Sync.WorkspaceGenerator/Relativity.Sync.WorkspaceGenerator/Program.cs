using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services;
using Relativity.Services.Workspace;
using Relativity.Sync.WorkspaceGenerator.FileGenerator;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.FileContentProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.FileExtensionProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.SizeCalculator;
using Relativity.Sync.WorkspaceGenerator.Import;
using Relativity.Sync.WorkspaceGenerator.LoadFileGenerator;
using Relativity.Sync.WorkspaceGenerator.RelativityServices;

namespace Relativity.Sync.WorkspaceGenerator
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			Console.Title = "Workspace Generator";

			GeneratorSettings settings = new GeneratorSettingsReader().DefaultSettings();
			RelativityServicesFactory relativityServicesFactory = new RelativityServicesFactory(settings);
			WorkspaceService workspaceService = relativityServicesFactory.CreateWorkspaceService();

			List<CustomField> randomFields = new FieldsGenerator().GetRandomFields(settings.NumberOfFields).ToList();

			List<CustomField> fieldsToCreate = new List<CustomField>()
			{
				new CustomField(ColumnNames.NativeFilePath, FieldType.FixedLengthText)
			};

			fieldsToCreate.AddRange(randomFields);

			WorkspaceRef workspace = await workspaceService
				.CreateWorkspaceAsync(settings.DesiredWorkspaceName, settings.TemplateWorkspaceName)
				.ConfigureAwait(false);
			await workspaceService
				.CreateFieldsAsync(workspace.ArtifactID, fieldsToCreate).ConfigureAwait(false);

			DirectoryInfo dataDir = new DirectoryInfo(settings.TestDataDirectoryPath);
			DirectoryInfo nativesDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"NATIVES"));
			DirectoryInfo textDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"TEXT"));

			IFileSizeCalculatorStrategy equalFileSizeCalculatorStrategy = new EqualFileSizeCalculatorStrategy();
			IFileGenerator nativesGenerator = new FileGenerator.FileGenerator(new RandomNativeFileExtensionProvider(), new NativeFileContentProvider(), nativesDir);
			IFileGenerator textGenerator = new FileGenerator.FileGenerator(new TextFileExtensionProvider(), new AsciiExtractedTextFileContentProvider(), textDir);

			DocumentFactory documentFactory = new DocumentFactory(settings, equalFileSizeCalculatorStrategy, equalFileSizeCalculatorStrategy, nativesGenerator, textGenerator);
			IEnumerable<Document> documents = await documentFactory.GenerateDocumentsAsync(randomFields).ConfigureAwait(false);
			DataTableBuilder dataTableBuilder = new DataTableBuilder(settings.GenerateNatives, settings.GenerateExtractedText, randomFields);
			dataTableBuilder.AddDocumentRange(documents);

			ImportHelper importHelper = new ImportHelper(workspaceService, settings);
			ImportJobErrors errors = await importHelper.ImportDataAsync(workspace.ArtifactID, dataTableBuilder.DataTable.CreateDataReader()).ConfigureAwait(false);

			if (errors.Success)
			{
				Console.WriteLine("Completed!");
			}
			else
			{
				foreach (string error in errors.Errors)
				{
					Console.WriteLine($"Import API error: {error}");
				}
			}

			Console.WriteLine("\n\nPress [Enter] to exit");
			Console.ReadLine();
		}
	}
}
