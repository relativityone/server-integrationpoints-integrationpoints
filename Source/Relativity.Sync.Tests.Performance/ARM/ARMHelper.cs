using Refit;
using Relativity.Automation.Utility;
using Relativity.Automation.Utility.Api;
using Relativity.Automation.Utility.Models;
using Relativity.Automation.Utility.Orchestrators;
using Relativity.Services.Interfaces.LibraryApplication.Models;
using Relativity.Sync.Tests.Performance.ARM.Contracts;
using Relativity.Sync.Tests.Performance.Helpers;
using Relativity.Sync.Tests.System;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SettingType = Relativity.Services.InstanceSetting.ValueType;

namespace Relativity.Sync.Tests.Performance.ARM
{
	public class ARMHelper
	{
		private static bool _isInitialized;

		private readonly ApiComponent _component;
		private readonly IARMApi _armApi;
		private readonly FileShareHelper _fileShare;
		private readonly AzureStorageHelper _storage;

		public const string RELATIVE_ARCHIVES_LOCATION = @"DefaultFileRepository\Archives"; //FileRepositoryID

		private static string REMOTE_ARCHIVES_LOCATION => Path.Combine(@"\\emttest\", RELATIVE_ARCHIVES_LOCATION);

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
				if (!IsInstalled())
				{
					InstallARM();
				}

				ConfigureARM();

				_isInitialized = true;
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

			LibraryApplicationRequestOptions options = new LibraryApplicationRequestOptions() { CreateIfMissing = true };
			applicationsOrchestrator.InstallRelativityApplicationToLibrary(rapPath, options);
		}

		private string GetARMRapPath()
		{
			string rapPath = Path.Combine(Path.GetTempPath(), @"ARM.rap");
			if (!File.Exists(rapPath))
			{
				rapPath = _storage.DownloadFileAsync(@"ARM\ARM.rap", Path.GetTempPath()).Result;
			}

			return rapPath;
		}

		private void ConfigureARM()
		{
			RTFSubstitute.CreateOrUpdateInstanceSetting(_component.ServiceFactory,
				"BcpShareFolderName", "kCura.ARM", SettingType.Text, @"\\emttest\BCPPath").Wait();
				
			_fileShare.CreateDirectoryAsync(RELATIVE_ARCHIVES_LOCATION).Wait();

			ContractEnvelope<ArmConfiguration> request = ArmConfiguration.GetRequest(REMOTE_ARCHIVES_LOCATION);
			_armApi.SetConfigurationAsync(request).Wait();
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

		public async Task RestoreWorkspaceAsync(string archivedWorkspace)
		{
			string uploadedFile = await _fileShare.UploadFileAsync(archivedWorkspace, RELATIVE_ARCHIVES_LOCATION).ConfigureAwait(false);

			string archivedWorkspacePath = Path.Combine(REMOTE_ARCHIVES_LOCATION, Path.GetFileNameWithoutExtension(uploadedFile));

			Job job = await CreateRestoreJobAsync(archivedWorkspacePath).ConfigureAwait(false);

			await RunJobAsync(job).ConfigureAwait(false);

			await WaitUntilJobIsCompleted(job).ConfigureAwait(false);
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
			const int delay = 10000;
			JobStatus jobStatus;
			do
			{
				jobStatus = await _armApi.GetJobStatus(job).ConfigureAwait(false);
				if(jobStatus.Status == "Errored")
				{
					throw new InvalidOperationException("Error occured during restoring workspace");
				}

				await Task.Delay(delay).ConfigureAwait(false);
			}
			while (jobStatus.Status != "Complete");
		}
	}
}
