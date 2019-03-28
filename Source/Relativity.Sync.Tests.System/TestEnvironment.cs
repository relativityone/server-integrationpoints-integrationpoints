using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Kepler.Transport;
using Relativity.Services.ApplicationInstallManager;
using Relativity.Services.LibraryApplicationsManager;
using Relativity.Services.ServiceProxy;
using Relativity.Services.Workspace;
using Relativity.Sync.Tests.System.Stubs;

namespace Relativity.Sync.Tests.System
{
	public sealed class TestEnvironment : IDisposable
	{
		private WorkspaceRef _templateWorkspace;
		private const string _RELATIVITY_SYNC_TEST_HELPER_RAP = "Relativity_Sync_Test_Helper.rap";
		private readonly List<WorkspaceRef> _workspaces = new List<WorkspaceRef>();
		private readonly SemaphoreSlim _templateWorkspaceSettingSemaphore = new SemaphoreSlim(1);
		private readonly ServiceFactory _serviceFactory;
		private static readonly Guid _libraryApplicationGuid = new Guid("e08fd0d9-c3a1-4654-87ad-104f08980b84");

		public TestEnvironment()
		{
			_serviceFactory = new ServiceFactoryFromAppConfig().CreateServiceFactory();
		}

		public async Task<WorkspaceRef> CreateWorkspaceAsync()
		{
			string name = $"{Guid.NewGuid().ToString()}";

			using (var workspaceManager = _serviceFactory.CreateProxy<IWorkspaceManager>())
			{
				WorkspaceRef template = await GetTemplateWorkspace(workspaceManager).ConfigureAwait(false);
				WorkspaceSetttings settings = new WorkspaceSetttings { Name = name, TemplateArtifactId = template.ArtifactID};
				WorkspaceRef newWorkspace =  await workspaceManager.CreateWorkspaceAsync(settings).ConfigureAwait(false);
				if (newWorkspace == null)
				{
					throw new InvalidOperationException("Workspace creation failed. WorkspaceManager kepler service returned null");
				}
				_workspaces.Add(newWorkspace);
				return newWorkspace;
			}
		}

		private async Task<WorkspaceRef> GetTemplateWorkspace(IWorkspaceManager workspaceManager)
		{
			try
			{
				await _templateWorkspaceSettingSemaphore.WaitAsync().ConfigureAwait(false);
				if (_templateWorkspace == null)
				{
					IEnumerable<WorkspaceRef> workspaces = await workspaceManager.RetrieveAllActive().ConfigureAwait(false);
					return _templateWorkspace = workspaces.First(w => w.Name == "Relativity Starter Template");
				}

				return _templateWorkspace;
			}
			finally
			{
				_templateWorkspaceSettingSemaphore.Release();
			}
		}

		public async Task DoCleanup()
		{
			using (var manager = _serviceFactory.CreateProxy<IWorkspaceManager>())
			{
				// ReSharper disable once AccessToDisposedClosure
				await Task.WhenAll(_workspaces.Select(w => manager.DeleteAsync(new WorkspaceRef(w.ArtifactID)))).ConfigureAwait(false);
			}
			_workspaces.Clear();
		}

		public async Task CreateFieldsInWorkspace(int workspaceArtifactId)
		{
			using (var applicationLibraryManager = _serviceFactory.CreateProxy<ILibraryApplicationsManager>())
			{
				ICollection<LibraryApplication> apps = await applicationLibraryManager.GetAllLibraryApplicationsAsync().ConfigureAwait(false);
				if (!apps.Any(app => app.GUID == _libraryApplicationGuid))
				{
					string appFilePath = GetHelperApplicationFilePath();
					using (var fileStream = File.OpenRead(appFilePath))
					using(var keplerStream = new KeplerStream(fileStream))
					{
						await applicationLibraryManager.EnsureApplication(_RELATIVITY_SYNC_TEST_HELPER_RAP, keplerStream, false, false).ConfigureAwait(false);
					}
				}
			}
			using (var applicationInstallManager = _serviceFactory.CreateProxy<IApplicationInstallManager>())
			{
				await applicationInstallManager.InstallLibraryApplicationByGuid(workspaceArtifactId, _libraryApplicationGuid).ConfigureAwait(false);
			}
		}

		private static string GetHelperApplicationFilePath()
		{
			string dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (string.IsNullOrWhiteSpace(dllDir))
			{
				throw new InvalidOperationException("Cannot locate executing dll folder.");
			}

			string appFilePath = Path.Combine(dllDir, "Resources", _RELATIVITY_SYNC_TEST_HELPER_RAP);
			return appFilePath;
		}

		public void Dispose()
		{
			_templateWorkspaceSettingSemaphore.Dispose();
		}
	}
}
