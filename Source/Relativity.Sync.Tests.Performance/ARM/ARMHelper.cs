using Refit;
using Relativity.Automation.Utility;
using Relativity.Automation.Utility.Api;
using Relativity.Automation.Utility.Models;
using Relativity.Automation.Utility.Orchestrators;
using Relativity.Services.Interfaces.LibraryApplication.Models;
using Relativity.Sync.Tests.Performance.ARM.Contracts;
using Relativity.Sync.Tests.Performance.Helpers;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Relativity.Sync.Tests.System.Core;

namespace Relativity.Sync.Tests.Performance.ARM
{
	public class ARMHelper
	{
		private static bool _isInitialized;

		private const string _RELATIVE_ARCHIVES_LOCATION = @"DefaultFileRepository\Archives";

		private readonly ApiComponent _component;
		private readonly IARMApi _armApi;
		private readonly FileShareHelper _fileShare;
		private readonly AzureStorageHelper _storage;

		private static string REMOTE_ARCHIVES_LOCATION => Path.Combine(@"\\emttest\", _RELATIVE_ARCHIVES_LOCATION);

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
			if(!_isInitialized)
			{
				Debug.WriteLine("ARM is being initialized...");
				if (!IsInstalled())
				{
					InstallARM();
				}

				ConfigureARM();

				_isInitialized = true;
				Debug.WriteLine("ARM has been initialized.");
			}
		}

		private bool IsInstalled()
		{
			LibraryApplicationResponse app = new LibraryApplicationResponse() { Name = "ARM" };
			bool isAppInstalled = _component.OrchestratorFactory.Create<IOrchestrateRelativityApplications>()
				.IsApplicationInstalledInLibrary(app);

			return isAppInstalled;
		}

		private void InstallARM()
		{
			IOrchestrateRelativityApplications applicationsOrchestrator =
				_component.OrchestratorFactory.Create<IOrchestrateRelativityApplications>();

			string rapPath = GetARMRapPath();

			Debug.WriteLine("Installing ARM...");
			LibraryApplicationRequestOptions options = new LibraryApplicationRequestOptions() { CreateIfMissing = true };
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
			settingOrchestrator.SetInstanceSetting("BcpShareFolderName", @"\\emttest\BCPPath", "kCura.ARM", InstanceSettingValueTypeEnum.Text);

			_fileShare.CreateDirectoryAsync(_RELATIVE_ARCHIVES_LOCATION).GetAwaiter().GetResult();
			Debug.WriteLine($"ARM Archive Location has been created on fileshare: {_RELATIVE_ARCHIVES_LOCATION}");

			string webApiPath = settingOrchestrator.GetInstanceSetting("RelativityWebApiUrl", "kCura.ARM")?.Value;

			if (webApiPath == null)
			{
				Debug.WriteLine("kCura.ARM.RelativityWebApiUrl instance setting not found, reverting to AppSettings");
				webApiPath = AppSettings.RelativityWebApiUrl.AbsoluteUri;
			}

			ContractEnvelope <ArmConfiguration> request = ArmConfiguration.GetRequest(webApiPath, REMOTE_ARCHIVES_LOCATION);

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

		public async Task<int> RestoreWorkspaceAsync(string archivedWorkspace)
		{
			string uploadedFile = await _fileShare.UploadFileAsync(archivedWorkspace, _RELATIVE_ARCHIVES_LOCATION).ConfigureAwait(false);
			Debug.WriteLine("Archived workspace has been uploaded to fileshare.");

			string archivedWorkspacePath = Path.Combine(REMOTE_ARCHIVES_LOCATION, Path.GetFileNameWithoutExtension(uploadedFile));

			Job job = await CreateRestoreJobAsync(archivedWorkspacePath).ConfigureAwait(false);
			Debug.WriteLine($"Restore job {job.JobId} has been created");

			await RunJobAsync(job).ConfigureAwait(false);
			Debug.WriteLine($"Job {job.JobId} is running...");

			await WaitUntilJobIsCompleted(job).ConfigureAwait(false);
			Debug.WriteLine("Restore job has been completed successfully.");

			return await GetRestoredWorkspaceID(job.JobId).ConfigureAwait(false);
		}

		private async Task<Job> CreateRestoreJobAsync(string archivedWorkspacePath)
		{
			ContractEnvelope<RestoreJob> request = RestoreJob.GetRequest(archivedWorkspacePath);
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
				if(jobStatus.Status == "Errored")
				{
					throw new InvalidOperationException("Error occured during restoring workspace");
				}

				await Task.Delay(delay).ConfigureAwait(false);
			}
			while (jobStatus.Status != "Complete");
		}

		private static async Task<int> GetRestoredWorkspaceID(int jobID)
		{
			using(SqlConnection connection = DbHelper.CreateConnectionFromAppConfig())
			{
				string sqlStatement = @"SELECT WorkspaceId
				  FROM [EDDSArchiving].[eddsdbo].[JobLocation]
				  WHERE IsDestination = 1
				   AND JobId = @jobId";
				SqlCommand command = new SqlCommand(sqlStatement, connection);
				command.Parameters.AddWithValue("jobId", jobID);

				object result = await command.ExecuteScalarAsync().ConfigureAwait(false);
				return (int)result;
			}
		}
	}
}
