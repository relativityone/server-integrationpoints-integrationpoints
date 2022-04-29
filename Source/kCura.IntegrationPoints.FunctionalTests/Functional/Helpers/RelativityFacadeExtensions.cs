using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers
{
	internal static class RelativityFacadeExtensions
	{
		public static Workspace CreateWorkspace(this IRelativityFacade instance, string name, string templateWorkspace = null)
		{
			Workspace workspace = String.IsNullOrWhiteSpace(templateWorkspace)
				? new Workspace { Name = name }
				: GetRequestByWorkspaceTemplate(instance, name, templateWorkspace);

			IWorkspaceService workspaceService = instance.Resolve<IWorkspaceService>();
			try
			{
				return workspaceService.Create(workspace);
			}
			catch(Exception ex)
			{
				Thread.Sleep(5000);
				Workspace createdWorkspace = workspaceService.Get(workspace.Name);
				if(createdWorkspace != null)
				{
					return createdWorkspace;
				}

				throw ex;
			}
		}

		public static Workspace GetExistingWorkspace(this IRelativityFacade instance, int workspaceArtifactID)
		{
			return instance.Resolve<IWorkspaceService>().Get(workspaceArtifactID);
		}

		public static void DeleteWorkspace(this IRelativityFacade instance, Workspace workspace)
		{
			IWorkspaceService workspaceService = instance.Resolve<IWorkspaceService>();

			workspaceService.Delete(workspace.ArtifactID);
		}

		public static void RequireAgent(this IRelativityFacade instance, string agentTypeName, int runInterval)
		{
			IAgentService agentService = instance.Resolve<IAgentService>();
			agentService.Require(new Agent
			{
				AgentType = agentService.GetAgentType(agentTypeName),
				AgentServer = agentService.GetAvailableAgentServers(agentTypeName).First(),
				RunInterval = runInterval
			});
		}

		public static void ImportDocumentsFromCsv(this IRelativityFacade instance, Workspace workspace, string pathToFile,
			string nativeFilePathColumnName = "FILE_PATH", string folderColumnName = null,
			DocumentOverwriteMode overwriteMode = DocumentOverwriteMode.AppendOverlay, DocumentOverlayBehavior overlayBehavior = DocumentOverlayBehavior.UseRelativityDefaults)
		{
			IDocumentService documentService = instance.Resolve<IDocumentService>();

			NativeImportOptions options = new NativeImportOptions
			{
				NativeFilePathColumnName = nativeFilePathColumnName,
				OverwriteMode = overwriteMode,
				FolderColumnName = folderColumnName,
				OverlayBehavior = overlayBehavior,
			};

			int documentImportTimeout = TestConfig.DocumentImportTimeout;

			SetImportMode();

			Task documentImportTask = Task.Run(() => documentService.ImportNativesFromCsv(workspace.ArtifactID, pathToFile, options));

			if (!documentImportTask.Wait(TimeSpan.FromSeconds(documentImportTimeout)))
			{
				throw new Exception($"IDocumentService.{nameof(documentService.ImportNativesFromCsv)} timeout ({documentImportTimeout}) exceeded.");
			}
		}

        public static void ImportImages(this IRelativityFacade instance, Workspace workspace, string pathToFile, int imagesCount)
        {
            IDocumentService documentService = instance.Resolve<IDocumentService>();

            int documentImportTimeout = TestConfig.DocumentImportTimeout;

            SetImportMode();

            DataTable dataTable = new DataTable();

		    dataTable.Columns.Add("BatesNumber");
            dataTable.Columns.Add("DocumentIdentifier");
            dataTable.Columns.Add("FileLocation");

            for (int i = 0; i < imagesCount; i++)
            {
                dataTable.Rows.Add(
                    $"DOC{i}",
                    $"DOC{i}",
                    pathToFile);
            }

            Task documentImportTask = Task.Run(() => documentService.ImportImages(workspace.ArtifactID, dataTable));

            if (!documentImportTask.Wait(TimeSpan.FromSeconds(documentImportTimeout)))
            {
                throw new Exception($"IDocumentService.{nameof(documentService.ImportImages)} timeout ({documentImportTimeout}) exceeded.");
            }
        }

		public static void ProduceProduction(this IRelativityFacade instance, Workspace workspace, Testing.Framework.Models.Production production)
		{
			IProductionService productionService = instance.Resolve<IProductionService>();
			IProductionDataSourceService productionDataSourceService = RelativityFacade.Instance.Resolve<IProductionDataSourceService>();

			var productionToProduce = productionService.Create(workspace.ArtifactID, production);

			if (production.DataSources.Count != 0)
			{
				foreach (var dataSource in production.DataSources)
				{
					dataSource.ProductionId = productionToProduce.ArtifactID;
					productionDataSourceService.Create(workspace.ArtifactID, productionToProduce.ArtifactID, dataSource);
				}
			}

			productionService.Stage(workspace.ArtifactID, productionToProduce.ArtifactID);
			productionService.Run(workspace.ArtifactID, productionToProduce.ArtifactID);

			Stopwatch productionRunTimeoutStopwatch = Stopwatch.StartNew();

			// You are looking at this and probably wondering why he didn't make it properly async.
			// Well, he did...
			// Apparently when you make RTF based UI test async all hell breaks loose, Atata looses context, and Being throws NullReferenceException.
			// So Thread.Sleep it is.
			ProductionStatus productionStatus;
			do
			{
				string status = productionService.GetStatus(workspace.ArtifactID, productionToProduce.ArtifactID).ToString().ToLower();

				if (!Enum.TryParse(status, true, out productionStatus))
				{
					Thread.Sleep(1000);
					continue;
				}

				if (status.Contains("error"))
				{
					throw new Exception("Production returns an unexpected error");
				}

				if (productionRunTimeoutStopwatch.Elapsed.TotalMinutes > 5)
				{
					throw new TaskCanceledException($"Production hasn't finished running in the 5 minute window. Check RelativityLogs or Errors for more information.");
				}

				Thread.Sleep(1000);
			}
			while (productionStatus != ProductionStatus.Produced);
		}

		private static Workspace GetRequestByWorkspaceTemplate(IRelativityFacade instance, string name, string templateName)
		{
			Workspace templateWorkspace = instance.Resolve<IWorkspaceService>().Get(templateName);

			ResourcePool resourcePool = instance.Resolve<IResourcePoolService>().Get(templateWorkspace.ResourcePool.Name);

			Group group = templateWorkspace.WorkspaceAdminGroup != null
				? instance.Resolve<IGroupService>().Get(templateWorkspace.WorkspaceAdminGroup.Name)
				: null;

			return new Workspace
			{
				Name = name,
				ResourcePool = resourcePool,
				TemplateWorkspace = templateWorkspace,
				DefaultCacheLocation = resourcePool.CacheLocationServers.FirstOrDefault(),
				WorkspaceAdminGroup = group
			};
		}

		private static void SetImportMode()
		{
			if (TestConfig.DocumentImportEnforceWebMode)
			{
				DataExchange.AppSettings.Instance.TapiForceHttpClient = true;
				DataExchange.AppSettings.Instance.TapiForceBcpHttpClient = true;
			}
		}
	}
}
