using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Relativity.Sync.WorkspaceGenerator.FileGenerator;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.FileContentProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.FileExtensionProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.SizeCalculator;
using Relativity.Sync.WorkspaceGenerator.RelativityServices;

namespace Relativity.Sync.WorkspaceGenerator
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			GeneratorSettings settings = new GeneratorSettingsReader().DefaultSettings();
			var relativityServicesFactory = new RelativityServicesFactory(settings);
			var workspaceService = relativityServicesFactory.CreateWorkspaceService();

			DirectoryInfo dataDir = new DirectoryInfo(settings.TestDataDirectoryPath);
			DirectoryInfo nativesDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"NATIVES"));
			DirectoryInfo textDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"TEXT"));

			IFileGenerator nativesGenerator = new FileGenerator.FileGenerator(new EqualFileSizeCalculatorStrategy(), new RandomNativeFileExtensionProvider(), new NativeFileContentProvider(), nativesDir);
			IFileGenerator textGenerator = new FileGenerator.FileGenerator(new EqualFileSizeCalculatorStrategy(), new TextFileExtensionProvider(), new AsciiExtractedTextFileContentProvider(), textDir);

			IEnumerable<FileInfo> natives = await nativesGenerator.GenerateAsync(filesCount: settings.NumberOfDocuments, totalSizeInMB: settings.TotalNativesSizeInMB).ConfigureAwait(false);
			IEnumerable<FileInfo> texts = await textGenerator.GenerateAsync(filesCount: settings.NumberOfDocuments, totalSizeInMB: settings.TotalExtractedTextSizeInMB).ConfigureAwait(false);
		}
	}
}
