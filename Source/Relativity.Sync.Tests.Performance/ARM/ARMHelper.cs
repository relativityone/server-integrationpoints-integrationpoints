using Refit;
using Relativity.Automation.Utility;
using Relativity.Automation.Utility.Api;
using Relativity.Automation.Utility.Models;
using Relativity.Automation.Utility.Orchestrators;
using Relativity.Services.Interfaces.LibraryApplication.Models;
using Relativity.Sync.Tests.Performance.ARM.Contracts;
using Relativity.Sync.Tests.Performance.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using kCura.Utility.Extensions;
using Relativity.Services.Workspace;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;

namespace Relativity.Sync.Tests.Performance.ARM
{
	public class ARMHelper
	{
		private static bool _isInitialized;


		private readonly ApiComponent _component;
		private readonly IARMApi _armApi;
		private readonly FileShareHelper _fileShare;
		private readonly AzureStorageHelper _storage;

		private static readonly string _INITIAL_BCP_PATH = "BCPPath";
		private static readonly string _RELATIVE_ARCHIVES_LOCATION = AppSettings.RelativeArchivesLocation;
		private static readonly string _UNC_ARCHIVE_LOCATION = Path.Combine(AppSettings.RemoteArchivesLocation, _RELATIVE_ARCHIVES_LOCATION);


		private ARMHelper(IARMApi armApi, FileShareHelper fileShare, AzureStorageHelper storage)
		{
			_component = RelativityFacade.Instance.GetComponent<ApiComponent>();

			_armApi = armApi;
			_fileShare = fileShare;
			_storage = storage;

			Logger = TestLogHelper.GetLogger();
		}

		protected ISyncLog Logger { get; }

		public static ARMHelper CreateInstance()
		{
			HttpClient armClient = RestService.CreateHttpClient(
				AppSettings.RelativityRestUrl.AbsoluteUri,
				new RefitSettings());
			armClient.DefaultRequestHeaders.Authorization =
				AuthenticationHeaderValue.Parse($"Basic {AppSettings.BasicAccessToken}");

			ARMHelper instance = new ARMHelper(
				RestService.For<IARMApi>(armClient),
				FileShareHelper.CreateInstance(),
				AzureStorageHelper.CreateFromTestConfig());

			instance.Initialize();

			return instance;
		}

		public void Initialize()
		{
			if (!_isInitialized)
			{
				Logger.LogInformation("ARM is being initialized...");
				if (ShouldBeInstalled())
				{
					InstallARM();
				}

				EnsureConfigurationAsync().GetAwaiter().GetResult();

				_isInitialized = true;
				Logger.LogInformation("ARM has been initialized.");
			}
		}

		private async Task EnsureConfigurationAsync()
		{
			Logger.LogInformation("Checking BCP path ARM configuration");

			IOrchestrateInstanceSettings settingOrchestrator = _component.OrchestratorFactory.Create<IOrchestrateInstanceSettings>();
			if (settingOrchestrator.GetInstanceSetting("BcpShareFolderName", "kCura.ARM").Value == _INITIAL_BCP_PATH)
			{
				Logger.LogInformation("ARM BCP path not set, using default value");

				settingOrchestrator.SetInstanceSetting("BcpShareFolderName", @"\\emttest\BCPPath", "kCura.ARM", InstanceSettingValueTypeEnum.Text);
			}

			Logger.LogInformation("Checking ARM locations");
			var currentArmConfiguration = await _armApi.GetConfigurationData().ConfigureAwait(false);

			if (currentArmConfiguration.ArmArchiveLocations.IsNullOrEmpty())
			{
				Logger.LogInformation("Remote ARM location not configured, creating default");
				await _fileShare.CreateDirectoryAsync(_RELATIVE_ARCHIVES_LOCATION).ConfigureAwait(false);

				Logger.LogInformation(
					$"ARM Archive Location has been created on fileshare: {_RELATIVE_ARCHIVES_LOCATION}");

				currentArmConfiguration.ArmArchiveLocations = new[] { new ArchiveLocation { Location = _UNC_ARCHIVE_LOCATION } };

				Logger.LogInformation("Updating ARM configuration");
				ContractEnvelope<ArmConfiguration> request = new ContractEnvelope<ArmConfiguration> { Contract = currentArmConfiguration };

				await _armApi.SetConfigurationAsync(request).ConfigureAwait(false);
			}
		}

		private bool ShouldBeInstalled()
		{
			Version installedVersion = GetInstalledArmVersion();
			Version rapArmVersion = GetRapArmVersion(GetARMRapPath());
			return installedVersion == null || rapArmVersion > installedVersion;
				}

		private static Version GetRapArmVersion(string armRapPath)
		{
			try
			{
				using (var rapFileStream = new FileStream(armRapPath, FileMode.Open))
				{
					using (var archive = new ZipArchive(rapFileStream, ZipArchiveMode.Read))
					{
						ZipArchiveEntry applicationXml = archive.GetEntry("application.xml");
						using (var stream = applicationXml.Open())
						{
							var xmlDoc = XDocument.Load(stream);
							var version = xmlDoc.XPathSelectElement(@"//Application/Version").Value;

							return new Version(version);
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new ArmHelperException("Could not get ARM version name from RAP", ex);
			}
		}

		private Version GetInstalledArmVersion()
		{
			const string armAppName = "ARM";
			LibraryApplicationResponse app = new LibraryApplicationResponse() { Name = armAppName };
			bool isAppInstalled = _component.OrchestratorFactory.Create<IOrchestrateRelativityApplications>()
				.IsApplicationInstalledInLibrary(app);

			if (isAppInstalled)
			{
				string version = _component.OrchestratorFactory.Create<IOrchestrateRelativityApplications>()
					.GetLibraryApplicationByName(armAppName).Version;

				return new Version(version);
			}

			return null;
		}

		private void InstallARM()
		{
			IOrchestrateRelativityApplications applicationsOrchestrator =
				_component.OrchestratorFactory.Create<IOrchestrateRelativityApplications>();

			string rapPath = GetARMRapPath();
			try
			{
			Logger.LogInformation("Installing ARM...");
			LibraryApplicationRequestOptions options = new LibraryApplicationRequestOptions() { CreateIfMissing = true, };
			applicationsOrchestrator.InstallRelativityApplicationToLibrary(rapPath, options);
			Logger.LogInformation("ARM has been successfully installed.");
			}
			finally
			{
				File.Delete(rapPath);
			}
		}

		private string GetARMRapPath()
		{
			Logger.LogInformation("Trying to get ARM RAP path...");
			string rapPath = Path.Combine(Path.GetTempPath(), @"ARM.rap");
			if (!File.Exists(rapPath))
			{
				Logger.LogInformation("ARM RAP doesn't exist. Trying to download ARM from AzureStorage...");
				rapPath = _storage.DownloadFileAsync(@"ARM\ARM.rap", Path.GetTempPath()).GetAwaiter().GetResult();
				Logger.LogInformation("ARM has been downloaded.");
			}

			Logger.LogInformation($"ARM Rap is located in path {rapPath}");
			return rapPath;
		}

		public void EnableAgents(bool ensureNew = false)
		{
			EnableAgent("ARM Agent", ensureNew);
			EnableAgent("ARM Metric Agent", ensureNew);
			EnableAgent("Structured Analytics Manager", ensureNew);
			EnableAgent("Structured Analytics Worker", ensureNew);
		}

		private void EnableAgent(string type, bool ensureNew = true)
		{
			IOrchestrateAgents agentsOrchestrator = _component.OrchestratorFactory.Create<IOrchestrateAgents>();

			AgentType agentType = agentsOrchestrator.GetAllAgentTypes()
				.First(x => x.Name == type);

			ResourceServer server = agentsOrchestrator.GetAvailableAgentServersForAgentType(agentType.ArtifactID)
				.First(x => x.Type == ResourceServerTypeEnum.Agent);

			Agent agent = new Agent()
			{
				AgentType = agentType,
				AgentServer = server,
				RunInterval = (int)agentType.DefaultInterval.Value,
				LoggingLevel = (AgentLoggingLevelTypeEnum)agentType.DefaultLoggingLevel
			};

			agentsOrchestrator.GetBasicAgent(agent, ensureNew);
		}

		public async Task<int> RestoreWorkspaceAsync(string archivedWorkspaceLocalPath, TestEnvironment environment)
		{
			try
			{
				var workspaceName = GetWorkspaceNameFromArmZip(archivedWorkspaceLocalPath);

				await ThrowIfWorkspaceAlreadyExistsAsync(workspaceName, environment).ConfigureAwait(false);

				string remoteLocation = await GetRemoteLocationAsync().ConfigureAwait(false);

				string uploadedFile = await _fileShare.UploadFileAsync(archivedWorkspaceLocalPath, remoteLocation)
					.ConfigureAwait(false);
				Logger.LogInformation($"Archived workspace has been uploaded to fileshare ({uploadedFile})");

				string archivedWorkspacePath = Path.Combine(remoteLocation,
					Path.GetFileNameWithoutExtension(uploadedFile));

				Logger.LogInformation($"Restoring workspace from: {archivedWorkspacePath}");

				Job job = await CreateRestoreJobAsync(archivedWorkspacePath, AppSettings.ResourcePoolId).ConfigureAwait(false);
				Logger.LogInformation($"Restore job {job.JobId} has been created");

				await RunJobAsync(job).ConfigureAwait(false);
				Logger.LogInformation($"Job {job.JobId} is running...");

				await WaitUntilJobIsCompletedAsync(job).ConfigureAwait(false);
				Logger.LogInformation("Restore job has been completed successfully.");

				int restoredWorkspaceId = await environment.GetWorkspaceArtifactIdByNameAsync(workspaceName).ConfigureAwait(true);

				Logger.LogInformation($"Restored workspace {workspaceName} with ArtifactId {restoredWorkspaceId}");
				return restoredWorkspaceId;
			}
			catch (Exception ex)
			{
				Logger.LogInformation(ex.Message);
				throw;
			}
		}

		private static async Task ThrowIfWorkspaceAlreadyExistsAsync(string workspaceName, TestEnvironment environment)
		{
			WorkspaceRef workspace = await environment.GetWorkspaceAsync(workspaceName).ConfigureAwait(false);
			if (workspace != null)
			{
				throw new SyncException($"Workspace {workspaceName} already exists. Delete it or rename it to run the tests");
			}
		}

		private async Task<string> GetRemoteLocationAsync()
		{
			return (await _armApi.GetConfigurationData().ConfigureAwait(false)).ArmArchiveLocations.First().Location;
		}

		private static string GetWorkspaceNameFromArmZip(string archivedWorkspaceLocalPath)
		{
			try
			{
				using (var fileStream = new FileStream(archivedWorkspaceLocalPath, FileMode.Open))
				{
					using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
					{
						ZipArchiveEntry applicationXml = archive.Entries.First(e => e.Name == "ArchiveInformation.xml");
						using (var stream = applicationXml.Open())
						{
							var xmlDoc = XDocument.Load(stream);
							return xmlDoc.XPathSelectElement(@"//ArchiveInformation/CaseInformation/Name").Value;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debugger.Break();
				throw new ArmHelperException("Could not get workspace name from file", ex);
			}
		}

		private async Task<Job> CreateRestoreJobAsync(string archivedWorkspacePath, int resourcePoolId)
		{
			ContractEnvelope<RestoreJob> request = RestoreJob.GetRequest(archivedWorkspacePath, resourcePoolId);

			try
			{
				return await _armApi.CreateRestoreJobAsync(request).ConfigureAwait(false);
			}
			catch (ApiException ex)
			{
				Logger.LogError(ex, "API Exception occurred during ARM restore. Content: {content}", ex.Content);
				throw;
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Exception occurred during ARM restore");
				throw;
			}
		}

		private Task RunJobAsync(Job job)
		{
			return _armApi.RunJobAsync(job);
		}

		private async Task WaitUntilJobIsCompletedAsync(Job job)
		{
			// restoring workspaces takes a long time
			const int delay = 30000;
			JobStatus jobStatus;
			do
			{
				jobStatus = await _armApi.GetJobStatus(job).ConfigureAwait(false);
				Logger.LogInformation($"Job is still processing ({jobStatus.Status})...");
				if (jobStatus.Status == "Errored")
				{
					throw new InvalidOperationException("Error occured during restoring workspace");
				}

				await Task.Delay(delay).ConfigureAwait(false);
			}
			while (jobStatus.Status != "Complete");
		}
	}
}
