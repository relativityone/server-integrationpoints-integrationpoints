using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
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
		private const string _RELATIVITY_SYNC_TEST_HELPER_RAP = "Relativity_Sync_Test_Helper.xml";
		private readonly List<WorkspaceRef> _workspaces = new List<WorkspaceRef>();
		private readonly SemaphoreSlim _templateWorkspaceSemaphore = new SemaphoreSlim(1);
		private readonly ServiceFactory _serviceFactory;
		private static readonly Guid _HELPER_APP_GUID = new Guid("e08fd0d9-c3a1-4654-87ad-104f08980b84");

		public TestEnvironment()
		{
			_serviceFactory = new ServiceFactoryFromAppConfig().CreateServiceFactory();
		}

		public async Task<WorkspaceRef> CreateWorkspaceAsync(string templateWorkspaceName = "Relativity Starter Template")
		{
			string name = Guid.NewGuid().ToString();

			using (var workspaceManager = _serviceFactory.CreateProxy<IWorkspaceManager>())
			{
				WorkspaceRef template = await GetTemplateWorkspace(workspaceManager, templateWorkspaceName).ConfigureAwait(false);
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

		private async Task<WorkspaceRef> GetTemplateWorkspace(IWorkspaceManager workspaceManager, string templateWorkspaceName)
		{
			try
			{
				await _templateWorkspaceSemaphore.WaitAsync().ConfigureAwait(false);
				if (_templateWorkspace == null)
				{
					IEnumerable<WorkspaceRef> workspaces = await workspaceManager.RetrieveAllActive().ConfigureAwait(false);
					return _templateWorkspace = workspaces.First(w => w.Name == templateWorkspaceName);
				}

				return _templateWorkspace;
			}
			finally
			{
				_templateWorkspaceSemaphore.Release();
			}
		}

		public async Task DoCleanup()
		{
			using (var manager = _serviceFactory.CreateProxy<IWorkspaceManager>())
			{
				// ReSharper disable once AccessToDisposedClosure - False positive. We're awaiting all tasks, so we can be sure dispose will be done after each call is handled
				await Task.WhenAll(_workspaces.Select(w => manager.DeleteAsync(new WorkspaceRef(w.ArtifactID)))).ConfigureAwait(false);
			}
			_workspaces.Clear();
		}

		public async Task CreateFieldsInWorkspace(int workspaceArtifactId)
		{
			await InstallHelperAppIfNeeded().ConfigureAwait(false);
			using (var applicationInstallManager = _serviceFactory.CreateProxy<IApplicationInstallManager>())
			{
				await applicationInstallManager.InstallLibraryApplicationByGuid(workspaceArtifactId, _HELPER_APP_GUID).ConfigureAwait(false);
			}
		}

		private async Task InstallHelperAppIfNeeded()
		{
			string appFilePath = GetHelperApplicationFilePath();
			using (var fileStream = File.OpenRead(appFilePath))
			using (var applicationLibraryManager = _serviceFactory.CreateProxy<ILibraryApplicationsManager>())
			{
				ICollection<LibraryApplication> apps = await applicationLibraryManager.GetAllLibraryApplicationsAsync().ConfigureAwait(false);
				LibraryApplication libraryApp = apps.FirstOrDefault(app => app.GUID == _HELPER_APP_GUID);
				var libraryAppVersion = new Version(libraryApp?.Version ?? "0.0.0.0");
				Version appXmlAppVersion = GetVersionFromApplicationXmlStream(fileStream);

				if (libraryAppVersion < appXmlAppVersion)
				{
					// Rewinding stream as it will be reused.
					fileStream.Seek(0, SeekOrigin.Begin);
					using (var outStream = await CreateRapFileInMemory(fileStream).ConfigureAwait(false))
					using (var keplerStream = new KeplerStream(outStream))
					{
						await applicationLibraryManager.EnsureApplication(Path.ChangeExtension(_RELATIVITY_SYNC_TEST_HELPER_RAP, "rap"), keplerStream, false, false).ConfigureAwait(false);
					}
				}
			}
		}

		private static async Task<MemoryStream> CreateRapFileInMemory(FileStream applicationXmlFileStream)
		{
			MemoryStream outStream = new MemoryStream();
			using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
			{
				ZipArchiveEntry fileEntry = archive.CreateEntry("application.xml", CompressionLevel.Fastest);
				using (var entryStream = fileEntry.Open())
				{
					await applicationXmlFileStream.CopyToAsync(entryStream).ConfigureAwait(false);
				}
			}

			// Rewinding stream as it is meant to be reused.
			outStream.Seek(0, SeekOrigin.Begin);
			return outStream;
		}

		private static Version GetVersionFromApplicationXmlStream(FileStream fileStream)
		{
			XmlDocument appXml = SafeLoadXml(fileStream);
			string versionStringFromXml = appXml?.SelectSingleNode("//Version")?.InnerText;
			if (versionStringFromXml == null)
			{
				throw new InvalidOperationException("Application XML could not be parsed. Cannot find Version node.");
			}

			Version appXmlAppVersion = new Version(versionStringFromXml);
			return appXmlAppVersion;
		}

		/// <summary>
		/// To prevent insecure DTD processing we have to load the XML in a specific way.
		/// See https://docs.microsoft.com/en-us/visualstudio/code-quality/ca3075-insecure-dtd-processing?view=vs-2017 for more info
		/// </summary>
		/// <param name="fileStream">Stream to be read</param>
		/// <returns>XML document loaded from given stream</returns>
		private static XmlDocument SafeLoadXml(FileStream fileStream)
		{
			var xmlReaderSettings = new XmlReaderSettings {XmlResolver = null};
			XmlReader reader = XmlReader.Create(fileStream, xmlReaderSettings);
			XmlDocument appXml = new XmlDocument {XmlResolver = null};
			appXml.Load(reader);
			return appXml;
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
			_templateWorkspaceSemaphore.Dispose();
		}
	}
}
