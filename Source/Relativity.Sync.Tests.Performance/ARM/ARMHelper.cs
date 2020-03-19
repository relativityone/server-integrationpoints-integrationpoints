using Relativity.Services.Interfaces.LibraryApplication.Models;
using Relativity.Sync.Tests.Performance.ARM;
using Relativity.Sync.Tests.Performance.Helpers;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Orchestrators;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Relativity.Sync.Tests.Performance.ARM
{
	public class ARMHelper
	{
		private bool _isInitialized;
		private readonly ApiComponent _component;
		private readonly FileShareHelper _fileShare;
		private readonly RequestProvider _requestProvider;
		private readonly AzureStorageHelper _storageHelper;

		public const string RELATIVE_ARCHIVES_LOCATION = @"DefaultFileRepository\Archives"; //FileRepositoryID

		private static string REMOTE_ARCHIVES_LOCATION => Path.Combine(@"\\emttest\", RELATIVE_ARCHIVES_LOCATION);

		private ARMHelper(FileShareHelper fileShare, AzureStorageHelper storageHelper)
		{
			_component = RelativityFacade.Instance.GetComponent<ApiComponent>();

			_fileShare = fileShare;
			_storageHelper = storageHelper;
			_requestProvider = new RequestProvider(_component);
		}

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

		public void EnableAgents()
		{
			CreateAgent("ARM Agent");
			CreateAgent("ARM Metric Agent");
			CreateAgent("Structured Analytics Manager");
			CreateAgent("Structured Analytics Worker");

			//Wait till all agents will be enabled
		}

		public void RestoreWorkspace(string archivedWorkspace)
		{
			string uploadedFile = _fileShare.UploadFile(archivedWorkspace, RELATIVE_ARCHIVES_LOCATION);

			string archivedWorkspacePath = Path.Combine(REMOTE_ARCHIVES_LOCATION, Path.GetFileNameWithoutExtension(uploadedFile));

			int jobID = CreateRestoreJob(archivedWorkspacePath);

			RunJob(jobID);

			//Wait till job end
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

			LibraryApplicationResponse app = new LibraryApplicationResponse() { Name = "ARM" };
			bool isAppInstalled = applicationsOrchestrator.IsApplicationInstalledInLibrary(app);

			if (!isAppInstalled)
			{
				string rapPath = GetARMRapPath();

				LibraryApplicationRequestOptions options = new LibraryApplicationRequestOptions() { CreateIfMissing = true };
				applicationsOrchestrator.InstallRelativityApplicationToLibrary(rapPath, options);
			}
		}

		private string GetARMRapPath()
		{
			string rapPath = Path.Combine(Path.GetTempPath(), "ARM.rap");
			if(!File.Exists(rapPath))
			{
				rapPath = _storageHelper.DownloadFile(@"ARM\ARM.rap", Path.GetTempPath());
			}

			return rapPath;
		}

		private void ConfigureARM()
		{
			IOrchestrateInstanceSettings instanceSettings = _component.OrchestratorFactory.Create<IOrchestrateInstanceSettings>();
			instanceSettings.SetInstanceSetting("BcpShareFolderName", @"\\emttest\BCPPath", "kCura.ARM");

			_fileShare.CreateDirectory(RELATIVE_ARCHIVES_LOCATION);

			string request = _requestProvider.ConfigurationRequest(REMOTE_ARCHIVES_LOCATION);
			SendAsync("Relativity.ARM/Configuration/SetConfigurationData", request);
		}

		private void CreateAgent(string type, bool ensureNew = true)
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

		private int CreateRestoreJob(string archivedWorkspacePath)
		{
			string request = _requestProvider.RestoreJobRequest(archivedWorkspacePath);
			SendAsync("Relativity.ARM/Jobs/Restore/Create", request);

			return 0; //TODO: Return proper Job ID
		}

		private void RunJob(int jobID)
		{
			string request = _requestProvider.RunJobRequest(jobID);
			SendAsync("Relativity.ARM/Jobs/Action/Run", request);
		}

		private static HttpResponseMessage SendAsync(string armUri, string request)
		{
			using (HttpClient client = GetARMClient())
			{
				return client.PostAsync(
					new Uri(TestSettingsConfig.RelativityRestUrl, armUri),
					new StringContent(request, Encoding.UTF8, "application/json")).Result;
			}
		}

		private static HttpClient GetARMClient()
		{
			HttpClient client = new HttpClient();

			client.BaseAddress = TestSettingsConfig.RelativityRestUrl;

			string encodedUserAndPass = Convert.ToBase64String(
				Encoding.ASCII.GetBytes($"{TestSettingsConfig.AdminUsername}:{TestSettingsConfig.AdminPassword}"));

			client.DefaultRequestHeaders.Clear();
			client.DefaultRequestHeaders.Add("X-CSRF-Header", "-");
			client.DefaultRequestHeaders.Add("Authorization", $"Basic {encodedUserAndPass}");

			return client;
		}
	}
}
