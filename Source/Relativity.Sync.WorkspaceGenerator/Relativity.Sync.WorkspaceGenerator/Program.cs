using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services.ServiceProxy;
using Relativity.Services.Workspace;
using Relativity.Sync.WorkspaceGenerator.FileGenerator;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.FileContentProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.FileExtensionProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.SizeCalculator;

namespace Relativity.Sync.WorkspaceGenerator
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			//var credentials = new UsernamePasswordCredentials("relativity.admin@kcura.com", "Test1234!");
			//var relativityUri = new Uri("https://tvmdev/Relativity");
			//const string templateWorkspaceName = "Functional Tests Template";
			//const string newWorkspaceName = "Test";
			
			//var servicesUri = new Uri(relativityUri, "/Relativity.Services");
			//var restApiUri = new Uri(relativityUri, "/Relativity.Rest/api");
			//var settings = new ServiceFactorySettings(servicesUri, restApiUri, credentials);
			//var serviceFactory = new ServiceFactory(settings);

			//var workspaceService = new WorkspaceService(serviceFactory);
			//IEnumerable<WorkspaceRef> activeWorkspaces = workspaceService.GetAllActiveAsync().GetAwaiter().GetResult();
			//WorkspaceRef template = activeWorkspaces.FirstOrDefault(x => x.Name == templateWorkspaceName);

			//if (template == null)
			//{
			//	throw new Exception($"Cannot find workspace name: '{templateWorkspaceName}'");
			//}

			//WorkspaceRef newWorkspaceRef = await workspaceService.CreateWorkspaceAsync(newWorkspaceName, template.ArtifactID).ConfigureAwait(false);

			DirectoryInfo dataDir = new DirectoryInfo(@"C:\Data\WorkspaceGenerator");
			DirectoryInfo nativesDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"NATIVES"));
			DirectoryInfo textDir = new DirectoryInfo(Path.Combine(dataDir.FullName, @"TEXT"));

			IFileGenerator nativesGenerator = new FileGenerator.FileGenerator(new EqualFileSizeCalculatorStrategy(), new RandomNativeFileExtensionProvider(), new NativeFileContentProvider(), nativesDir);
			IFileGenerator textGenerator = new FileGenerator.FileGenerator(new EqualFileSizeCalculatorStrategy(), new TextFileExtensionProvider(), new AsciiExtractedTextFileContentProvider(), textDir);

			IEnumerable<FileInfo> natives = await nativesGenerator.GenerateAsync(count: 50, totalSizeInMB: 50).ConfigureAwait(false);
			IEnumerable<FileInfo> texts = await textGenerator.GenerateAsync(count: 50, totalSizeInMB: 20).ConfigureAwait(false);
		}
	}
}
