using Refit;
using Relativity.Automation.Utility;
using Relativity.Automation.Utility.Api;
using Relativity.Automation.Utility.Models;
using Relativity.Automation.Utility.Orchestrators;
using Relativity.Services.Interfaces.LibraryApplication.Models;
using Relativity.Sync.Tests.Performance.ARM.Contracts;
using Relativity.Sync.Tests.Performance.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using ARMTestServices.Services.Interfaces;
using Relativity.Services.Workspace;
using Relativity.Sync.Tests.System.Core;

namespace Relativity.Sync.Tests.Performance.ARM
{
	public class ARMHelper
	{
		private static bool _isInitialized;


		private readonly ApiComponent _component;
		private readonly IARMApi _armApi;
		private readonly FileShareHelper _fileShare;
		private readonly AzureStorageHelper _storage;
		private string _INITIAL_BCP_PATH = "BCPPath";

		private static string _RELATIVE_ARCHIVES_LOCATION => AppSettings.RelativeArchivesLocation;
		private static string REMOTE_ARCHIVES_LOCATION => Path.Combine(AppSettings.RemoteArchivesLocation, _RELATIVE_ARCHIVES_LOCATION);


		private ARMHelper(IARMApi armApi, FileShareHelper fileShare, AzureStorageHelper storage)
		{
			_component = RelativityFacade.Instance.GetComponent<ApiComponent>();

			_armApi = armApi;
			_fileShare = fileShare;
			_storage = storage;
		}

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
				Debug.WriteLine("ARM is being initialized...");
				if (ShouldBeInstalled())
				{
					InstallARM();
				}

				ConfigureARM();

				_isInitialized = true;
				Debug.WriteLine("ARM has been initialized.");
			}
		}

		private bool ShouldBeInstalled()
		{
			string installedVersion = GetInstalledArmVersion();
			string rapArmVersion = GetRapArmVersion(GetARMRapPath());
			return string.IsNullOrEmpty(installedVersion) || ShouldArmBeInstalledBasedOnVersion(installedVersion, rapArmVersion);
		}

		private static bool ShouldArmBeInstalledBasedOnVersion(string installedVersion, string rapArmVersion)
		{
			if (installedVersion == null || rapArmVersion == null)
			{
				return true;
			}

			// compare each version segment, starting from Major
			return installedVersion.Split('.').Zip(rapArmVersion.Split('.'), (installed, rap) =>
			{
				if (int.TryParse(installed, out var installedVersionValue) && int.TryParse(rap, out var rapVersionValue))
				{
					return rapVersionValue > installedVersionValue;
				}

				return true;
			}).Any(x => x);
		}

		private static string GetRapArmVersion(string armRapPath)
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
							return xmlDoc.XPathSelectElement(@"//Application/Version").Value;
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new ArmHelperException("Could not get ARM version name from RAP", ex);
			}
		}

		private string GetInstalledArmVersion()
		{
			const string armAppName = "ARM";
			LibraryApplicationResponse app = new LibraryApplicationResponse() { Name = armAppName };
			bool isAppInstalled = _component.OrchestratorFactory.Create<IOrchestrateRelativityApplications>()
				.IsApplicationInstalledInLibrary(app);

			if (isAppInstalled)
			{
				return _component.OrchestratorFactory.Create<IOrchestrateRelativityApplications>()
					.GetLibraryApplicationByName(armAppName).Version;
			}

			return null;
		}

		private void InstallARM()
		{
			IOrchestrateRelativityApplications applicationsOrchestrator =
				_component.OrchestratorFactory.Create<IOrchestrateRelativityApplications>();

			string rapPath = GetARMRapPath();

			Debug.WriteLine("Installing ARM...");
			LibraryApplicationRequestOptions options = new LibraryApplicationRequestOptions() { CreateIfMissing = true, };
			applicationsOrchestrator.InstallRelativityApplicationToLibrary(rapPath, options);
			Debug.WriteLine("ARM has been successfully installed.");
		}

		private string GetARMRapPath()
		{
			Debug.WriteLine("Trying to get ARM RAP path...");
			string rapPath = Path.Combine(Path.GetTempPath(), @"ARM.rap");
			if (!File.Exists(rapPath))
			{
				Debug.WriteLine("ARM RAP doesn't exist. Trying to download ARM from AzureStorage...");
				rapPath = _storage.DownloadFileAsync(@"ARM\ARM.rap", Path.GetTempPath()).Result;
				Debug.WriteLine("ARM has been downloaded.");
			}

			Debug.WriteLine($"ARM Rap is located in path {rapPath}");
			return rapPath;
		}

		private void ConfigureARM()
		{
			Debug.WriteLine("ARM configuration has been started...");

			IOrchestrateInstanceSettings settingOrchestrator = _component.OrchestratorFactory.Create<IOrchestrateInstanceSettings>();
			if (settingOrchestrator.GetInstanceSetting("BcpShareFolderName", "kCura.ARM").Value == _INITIAL_BCP_PATH)
			{
				settingOrchestrator.SetInstanceSetting("BcpShareFolderName", @"\\emttest\BCPPath", "kCura.ARM", InstanceSettingValueTypeEnum.Text);
			}

			_fileShare.CreateDirectoryAsync(_RELATIVE_ARCHIVES_LOCATION).GetAwaiter().GetResult();
			Debug.WriteLine($"ARM Archive Location has been created on fileshare: {_RELATIVE_ARCHIVES_LOCATION}");

			string webApiPath = settingOrchestrator.GetInstanceSetting("RelativityWebApiUrl", "kCura.ARM")?.Value;

			if (webApiPath == null)
			{
				Debug.WriteLine("kCura.ARM.RelativityWebApiUrl instance setting not found, reverting to AppSettings");
				webApiPath = AppSettings.RelativityWebApiUrl.AbsoluteUri;
			}

			ContractEnvelope<ArmConfiguration> request = ArmConfiguration.GetRequest(webApiPath, REMOTE_ARCHIVES_LOCATION);

			_armApi.SetConfigurationAsync(request).GetAwaiter().GetResult();

			Debug.WriteLine("ARM has been successfully configured.");
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
				Debug.WriteLine($"Removing all workspaces with name {workspaceName}");
				await RemoveAllWorkspacesWithName(workspaceName, environment).ConfigureAwait(false);

				string uploadedFile = await _fileShare.UploadFileAsync(archivedWorkspaceLocalPath, _RELATIVE_ARCHIVES_LOCATION)
					.ConfigureAwait(false);
				Debug.WriteLine("Archived workspace has been uploaded to fileshare.");

				string archivedWorkspacePath = Path.Combine(REMOTE_ARCHIVES_LOCATION,
					Path.GetFileNameWithoutExtension(uploadedFile));

				Job job = await CreateRestoreJobAsync(archivedWorkspacePath, AppSettings.ResourcePoolId).ConfigureAwait(false);
				Debug.WriteLine($"Restore job {job.JobId} has been created");

				await RunJobAsync(job).ConfigureAwait(false);
				Debug.WriteLine($"Job {job.JobId} is running...");

				await WaitUntilJobIsCompleted(job).ConfigureAwait(false);
				Debug.WriteLine("Restore job has been completed successfully.");

				int restoredWorkspaceId = await environment.GetWorkspaceArtifactIdByName(workspaceName).ConfigureAwait(true);
				return restoredWorkspaceId;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				throw;
			}
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

		private static async Task RemoveAllWorkspacesWithName(string workspaceName, TestEnvironment environment)
		{
			IEnumerable<WorkspaceRef> workspacesToDelete = await environment.GetWorkspacesAsync(workspaceName).ConfigureAwait(false);
			await environment.DeleteWorkspaces(workspacesToDelete.Select(x => x.ArtifactID)).ConfigureAwait(false);
		}

		private async Task<Job> CreateRestoreJobAsync(string archivedWorkspacePath, int resourcePoolId)
		{
			ContractEnvelope<RestoreJob> request = RestoreJob.GetRequest(archivedWorkspacePath, resourcePoolId);
			return await _armApi.CreateRestoreJobAsync(request).ConfigureAwait(false);
		}

		private async Task RunJobAsync(Job job)
		{
			await _armApi.RunJobAsync(job).ConfigureAwait(false);
		}

		private async Task WaitUntilJobIsCompleted(Job job)
		{
			// restoring workspaces takes a long time
			const int delay = 30000;
			JobStatus jobStatus;
			do
			{
				jobStatus = await _armApi.GetJobStatus(job).ConfigureAwait(false);
				Debug.WriteLine($"Job is still processing ({jobStatus.Status})...");
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
