using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Relativity.API;
using Relativity.ARM.Services.Interfaces.V1.JobAction;
using Relativity.ARM.Services.Interfaces.V1.JobStatus;
using Relativity.ARM.Services.Interfaces.V1.Models;
using Relativity.ARM.Services.Interfaces.V1.Models.JobStatus;
using Relativity.ARM.Services.Interfaces.V1.Models.Restore;
using Relativity.ARM.Services.Interfaces.V1.Restore;
using Relativity.Services.Workspace;
using Relativity.Sync.Tests.Performance.Helpers;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Kepler;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;

namespace Relativity.Sync.Tests.Performance.ARM
{
	public class ARMHelper
	{
		private static bool _isInitialized;

		private readonly FileShareHelper _fileShare;
		private readonly AzureStorageHelper _storage;

		private static readonly string _RELATIVE_ARCHIVES_LOCATION = AppSettings.RelativeArchivesLocation;
		private readonly IKeplerServiceFactory _serviceFactory;

		private ARMHelper(FileShareHelper fileShare, AzureStorageHelper storage)
		{
			_fileShare = fileShare;
			_storage = storage;

			_serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;

			Logger = TestLogHelper.GetLogger();
		}

		protected IAPILog Logger { get; }

		public static ARMHelper CreateInstance()
		{
			ARMHelper instance = new ARMHelper(
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

				ConfigureBCPPath();

				CreateRemoteLocationExistsAsync().GetAwaiter().GetResult();

				_isInitialized = true;
				Logger.LogInformation("ARM has been initialized.");
			}
		}

		private Task CreateRemoteLocationExistsAsync()
		{
			Logger.LogInformation("Creating remote archive location directory");

			return _fileShare.CreateDirectoryAsync(_RELATIVE_ARCHIVES_LOCATION);
		}

		private void ConfigureBCPPath()
		{
			RelativityFacade.Instance.Resolve<IInstanceSettingsService>()
				.UpdateValue("BcpShareFolderName", "kCura.ARM", AppSettings.RemoteBCPPathLocation);
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
			LibraryApplication armApplication = RelativityFacade.Instance.Resolve<ILibraryApplicationService>()
					.Get("ARM");

			return armApplication != null ? new Version(armApplication.Version) : null;
		}

		private void InstallARM()
		{
			string rapPath = GetARMRapPath();
			try
			{
				Logger.LogInformation("Installing ARM...");

				ILibraryApplicationService applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();

				applicationService.InstallToLibrary(rapPath, new LibraryApplicationInstallOptions()
				{
					CreateIfMissing = true
				});

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

		public void EnableAgents()
		{
			ResourceServer agentServer = RelativityFacade.Instance.Resolve<IResourceServerService>().GetAll()
				.FirstOrDefault(x => x.Type == ResourceServerType.Agent);

			EnableAgent("ARM Agent", agentServer);
			EnableAgent("ARM Metric Agent", agentServer);
			EnableAgent("Structured Analytics Manager", agentServer);
			EnableAgent("Structured Analytics Worker", agentServer);
		}

		private void EnableAgent(string type, ResourceServer agentServer)
		{
			IAgentService agentService = RelativityFacade.Instance.Resolve<IAgentService>();

			AgentType agentType = agentService.GetAgentType(type);

			agentService.Create(new Agent()
			{
				AgentServer = agentServer,
				AgentType = agentType,
				Enabled = true,
				LoggingLevel = (AgentLoggingLevelType)agentType.DefaultLoggingLevel.Value,
				RunInterval = (int)(agentType.DefaultInterval ?? 5)
			});
		}

		public async Task<int> RestoreWorkspaceAsync(string archivedWorkspaceLocalPath, TestEnvironment environment)
		{
			try
			{
				var workspaceName = GetWorkspaceNameFromArmZip(archivedWorkspaceLocalPath);

				await ThrowIfWorkspaceAlreadyExistsAsync(workspaceName, environment).ConfigureAwait(false);

				string uploadedFile = await _fileShare.UploadFileAsync(archivedWorkspaceLocalPath, _RELATIVE_ARCHIVES_LOCATION)
					.ConfigureAwait(false);
				Logger.LogInformation($"Archived workspace has been uploaded to fileshare ({uploadedFile})");

				Logger.LogInformation($"Restoring workspace from: {uploadedFile}");

				string remoteArchiveLocation =
					Path.Combine(AppSettings.RemoteArchivesLocation, Path.GetFileNameWithoutExtension(uploadedFile));

				int jobId = await CreateRestoreJobAsync(remoteArchiveLocation).ConfigureAwait(false);
				Logger.LogInformation($"Restore job {jobId} has been created");

				await RunJobAsync(jobId).ConfigureAwait(false);
				Logger.LogInformation($"Job {jobId} is running...");

				await WaitUntilJobIsCompletedAsync(jobId).ConfigureAwait(false);
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

		private static string GetWorkspaceNameFromArmZip(string archivedWorkspaceLocalPath)
		{
			try
			{
				using (ZipArchive archive = ZipFile.OpenRead(archivedWorkspaceLocalPath))
				{
					ZipArchiveEntry archiveInformationEntry =
						archive.Entries.First(e => e.Name == "ArchiveInformation.xml");

					string archiveInformationFile = Path.GetTempFileName();
					try
					{
						archiveInformationEntry.ExtractToFile(archiveInformationFile, true);
						using (var stream = File.OpenRead(archiveInformationFile))
						{
							var xmlDoc = XDocument.Load(stream);
							return xmlDoc.XPathSelectElement(@"//ArchiveInformation/CaseInformation/Name").Value;
						}
					}
					finally
					{
						File.Delete(archiveInformationFile);
					}

				}
			}
			catch (Exception ex)
			{
				Debugger.Break();
				throw new ArmHelperException("Could not get workspace name from file", ex);
			}
		}

		private async Task<int> CreateRestoreJobAsync(string archivedWorkspacePath)
		{
			try
			{
				using (var restoreJobManager = _serviceFactory.GetServiceProxy<IArmRestoreJobManager>())
				{
					RestoreJobRequest request = new RestoreJobRequest()
					{
						ArchivePath = archivedWorkspacePath,
						DestinationOptions = new RestoreDestinationOptions
						{
							ResourcePoolID = GetResourcePoolId(AppSettings.ResourcePoolName),
							DatabaseServerID = GetRestoreSqlServerId(),
							MatterID = AppSettings.ArmRelativityTemplateMatterId,
							CacheLocationID = AppSettings.ArmCacheLocationId,
							FileRepositoryID = AppSettings.ArmFileRepositoryId
						},

					};
					return await restoreJobManager.CreateAsync(request).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Exception occurred during ARM restore");
				throw;
			}
		}

		private async Task RunJobAsync(int jobId)
		{
			using (var armJobActionManager = _serviceFactory.GetServiceProxy<IArmJobActionManager>())
			{
				await armJobActionManager.RunAsync(jobId).ConfigureAwait(false);
			}
		}

		private async Task WaitUntilJobIsCompletedAsync(int jobId)
		{
			// restoring workspaces takes a long time
			const int delay = 30000;
			ArmJobStatusResponse jobStatus;
			using (var statusManager = _serviceFactory.GetServiceProxy<IArmJobStatusManager>())
			{
				do
				{
					jobStatus = await statusManager.ReadAsync(jobId).ConfigureAwait(false);
					Logger.LogInformation($"Job is still processing ({jobStatus.JobState})...");
					if (jobStatus.JobState == JobState.Errored)
					{
						throw new InvalidOperationException("Error occurred during restoring workspace");
					}

					await Task.Delay(delay).ConfigureAwait(false);
				} while (jobStatus.JobState != JobState.Complete);
			}
		}

		private int GetResourcePoolId(string name)
		{
			IResourcePoolService resourcePoolService = RelativityFacade.Instance.Resolve<IResourcePoolService>();

			var resourcePool = resourcePoolService.Get(name);

			return resourcePool.ArtifactID;
		}

		private int GetRestoreSqlServerId()
		{
			ResourceServer[] sqlServers = RelativityFacade.Instance.Resolve<IResourceServerService>().GetAll()
				.Where(x => x.Type == ResourceServerType.SqlDistributed || x.Type == ResourceServerType.SqlPrimary)
				.ToArray();

			return sqlServers.FirstOrDefault(x => x.Type == ResourceServerType.SqlDistributed)?.ArtifactID
				   ?? sqlServers.Single(x => x.Type == ResourceServerType.SqlPrimary).ArtifactID;
		}
	}
}
