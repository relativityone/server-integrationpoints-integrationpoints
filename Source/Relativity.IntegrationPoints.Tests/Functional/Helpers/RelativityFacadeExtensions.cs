using System;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers
{
	internal static class RelativityFacadeExtensions
	{
		public static Workspace CreateWorkspace(this IRelativityFacade instance, string name, string templateWorkspace = null)
		{
			IWorkspaceService workspaceService = instance.Resolve<IWorkspaceService>();

			Workspace workspace = new Workspace
			{
				Name = name
			};

			if (!String.IsNullOrWhiteSpace(templateWorkspace))
			{
				workspace.TemplateWorkspace = new NamedArtifact {Name = templateWorkspace};
			}

			return workspaceService.Create(workspace);
		}

		public static void DeleteWorkspace(this IRelativityFacade instance, Workspace workspace)
		{
			IWorkspaceService workspaceService = instance.Resolve<IWorkspaceService>();

			workspaceService.Delete(workspace.ArtifactID);
		}

		public static void ImportDocumentsFromCsv(this IRelativityFacade instance, Workspace workspace, string pathToFile,
			string nativeFilePathColumnName = "FILE_PATH", string folderColumnName = null,
			DocumentOverwriteMode overwriteMode = DocumentOverwriteMode.Append, DocumentOverlayBehavior overlayBehavior = DocumentOverlayBehavior.UseRelativityDefaults)
		{
			IDocumentService documentService = instance.Resolve<IDocumentService>();

			NativeImportOptions options = new NativeImportOptions
			{
				NativeFilePathColumnName = nativeFilePathColumnName,
				OverwriteMode = overwriteMode,
				FolderColumnName = folderColumnName,
				OverlayBehavior = overlayBehavior
			};

			int documentImportTimeout = Int32.Parse(TestContext.Parameters["DocumentImportTimeout"]);

			SetImportMode();

			Task documentImportTask = Task.Run(() => documentService.ImportNativesFromCsv(workspace.ArtifactID, pathToFile, options));

			if (!documentImportTask.Wait(TimeSpan.FromSeconds(documentImportTimeout)))
			{
				throw new Exception($"IDocumentService.ImportNativesFromCsv timeout ({documentImportTimeout}) exceeded.");
			}
		}

		private static void SetImportMode()
		{
			if (Boolean.Parse(TestContext.Parameters["DocumentImportEnforceWebMode"]))
			{
				DataExchange.AppSettings.Instance.TapiForceHttpClient = true;
				DataExchange.AppSettings.Instance.TapiForceBcpHttpClient = true;
			}
		}
	}
}
