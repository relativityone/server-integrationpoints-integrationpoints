using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services;
using Relativity.Services.Workspace;
using Relativity.Sync.WorkspaceGenerator.FileGenerator;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.FileContentProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.FileExtensionProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.SizeCalculator;
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

			List<CustomField> fields = new List<CustomField>
			{
				new CustomField("Control Number", FieldType.FixedLengthText),
				new CustomField("Extracted Text", FieldType.LongText)
			};

			List<CustomField> fieldsToCreate = new List<CustomField>();
			fieldsToCreate.Add(new CustomField("Native File Path", FieldType.FixedLengthText));
			fieldsToCreate.AddRange(new FieldsGenerator().GetRandomFields(settings.NumberOfFields).ToList());
			fields.AddRange(fieldsToCreate);


			//WorkspaceRef workspace = await workspaceService
			//	.CreateWorkspaceAsync(settings.DesiredWorkspaceName, settings.TemplateWorkspaceName)
			//	.ConfigureAwait(false);
			//await workspaceService.CreateFieldsAsync(workspace.ArtifactID, fieldsToCreate).ConfigureAwait(false);



			DirectoryInfo dataDir = new DirectoryInfo(settings.TestDataDirectoryPath);
			DirectoryInfo nativesDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"NATIVES"));
			DirectoryInfo textDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"TEXT"));

			IFileGenerator nativesGenerator = new FileGenerator.FileGenerator(new EqualFileSizeCalculatorStrategy(), new RandomNativeFileExtensionProvider(), new NativeFileContentProvider(), nativesDir);
			IFileGenerator textGenerator = new FileGenerator.FileGenerator(new EqualFileSizeCalculatorStrategy(), new TextFileExtensionProvider(), new AsciiExtractedTextFileContentProvider(), textDir);

			IEnumerable<FileInfo> natives = await nativesGenerator.GenerateAsync(filesCount: settings.NumberOfDocuments, totalSizeInMB: settings.TotalNativesSizeInMB).ConfigureAwait(false);
			IEnumerable<FileInfo> texts = await textGenerator.GenerateAsync(filesCount: settings.NumberOfDocuments, totalSizeInMB: settings.TotalExtractedTextSizeInMB).ConfigureAwait(false);



			LoadFileGenerator.LoadFileGenerator loadFileGenerator = new LoadFileGenerator.LoadFileGenerator(dataDir);
			IEnumerable<Document> documents = Enumerable.Zip(natives, texts, (nativeFile, extractedTextFile) => new Document(nativeFile, extractedTextFile));
			loadFileGenerator.GenerateLoadFile(documents.ToList(), fields);



			await Task.CompletedTask.ConfigureAwait(false);
		}
	}
}
