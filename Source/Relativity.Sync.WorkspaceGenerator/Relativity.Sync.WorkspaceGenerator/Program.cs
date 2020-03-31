using System;
using System.Collections;
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

namespace Relativity.Sync.WorkspaceGenerator
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			Console.Title = "Workspace Generator";

			GeneratorSettings settings = new GeneratorSettingsReader().ReadFromConsole();
			RelativityServicesFactory relativityServicesFactory = new RelativityServicesFactory(settings);
			WorkspaceService workspaceService = relativityServicesFactory.CreateWorkspaceService();

			List<CustomField> randomFields = new RandomFieldsGenerator().GetRandomFields(settings.NumberOfFields).ToList();
			List<CustomField> fieldsToCreate = new List<CustomField>(randomFields)
			{
				new CustomField(ColumnNames.NativeFilePath, FieldType.FixedLengthText)
			};

			WorkspaceRef workspace = await workspaceService
				.CreateWorkspaceAsync(settings.DesiredWorkspaceName, settings.TemplateWorkspaceName)
				.ConfigureAwait(false);
			await workspaceService
				.CreateFieldsAsync(workspace.ArtifactID, fieldsToCreate).ConfigureAwait(false);

			DirectoryInfo dataDir = new DirectoryInfo(settings.TestDataDirectoryPath);
			DirectoryInfo nativesDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"NATIVES"));
			DirectoryInfo textDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"TEXT"));

			IFileSizeCalculatorStrategy equalFileSizeCalculatorStrategy = new EqualFileSizeCalculatorStrategy();
			FileGenerator nativesGenerator = new FileGenerator(new RandomNativeFileExtensionProvider(), new NativeFileContentProvider(), nativesDir);
			FileGenerator textGenerator = new FileGenerator(new TextFileExtensionProvider(), new AsciiExtractedTextFileContentProvider(), textDir);

			IDocumentFactory documentFactory = new DocumentFactory(settings, equalFileSizeCalculatorStrategy, equalFileSizeCalculatorStrategy, nativesGenerator, textGenerator, randomFields);
			DataReaderWrapper dataReader = new DataReaderWrapper(documentFactory, settings.NumberOfDocuments, settings.GenerateNatives, settings.GenerateExtractedText, randomFields);

			ImportHelper importHelper = new ImportHelper(workspaceService, settings);
			ImportJobResult result = await importHelper.ImportDataAsync(workspace.ArtifactID, dataReader).ConfigureAwait(false);

			if (result.Success)
			{
				Console.WriteLine("Completed!");
			}
			else
			{
				foreach (string error in result.Errors)
				{
					Console.WriteLine($"Import API error: {error}");
				}
			}

			Console.WriteLine("\n\nPress [Enter] to exit");
			Console.ReadLine();
		}
	}
}
