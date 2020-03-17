using Newtonsoft.Json;
using Relativity.Services.Interfaces.LibraryApplication.Models;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Orchestrators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Relativity.Sync.Tests.Performance.Helpers
{
	public class ARMHelper
	{
		private bool _isInitialized;
		private ApiComponent _component;
		private readonly FileShareHelper _fileShare;

		public const string RELATIVE_ARCHIVES_LOCATION = @"DefaultFileRepository\Archives"; //FileRepositoryID

		private static string REMOTE_ARCHIVES_LOCATION => Path.Combine(@"\\emttest\", RELATIVE_ARCHIVES_LOCATION);

		private ARMHelper(FileShareHelper fileShare)
		{
			_fileShare = fileShare;
		}

		public static ARMHelper CreateInstance()
		{
			ARMHelper instance = new ARMHelper(
				FileShareHelper.CreateInstance());
			instance.Initialize();

			return instance;
		}

		public void Initialize()
		{
			if(!_isInitialized)
			{
				RelativityFacade.Instance.RelyOn<ApiComponent>();
				_component = RelativityFacade.Instance.GetComponent<ApiComponent>();

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
			CreateAgent("Analytics Core ARM Manager Agent");
			CreateAgent("Analytics Core ARM Worker Agent");

			//Wait till all agents will be enabled
		}

		public void RestoreWorkspace(string armedWorkspace)
		{
			// Gather workspace from azure storage account
			_fileShare.UploadFile(armedWorkspace, RELATIVE_ARCHIVES_LOCATION);

			string remoteWorkspacePath = Path.Combine(REMOTE_ARCHIVES_LOCATION, Path.GetFileNameWithoutExtension(armedWorkspace));

			int jobID = CreateRestoreJob(remoteWorkspacePath);

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
				if (!File.Exists(TestSettingsConfig.ARMRapPath))
				{
					throw new FileNotFoundException(@"ARM Rap not found. To get latest release version run .\DevelopmentScripts\Get-ARM.ps1");
				}

				LibraryApplicationRequestOptions options = new LibraryApplicationRequestOptions() { CreateIfMissing = true };
				applicationsOrchestrator.InstallRelativityApplicationToLibrary(TestSettingsConfig.ARMRapPath, options);
			}
		}

		private void ConfigureARM()
		{
			IOrchestrateInstanceSettings instanceSettings = _component.OrchestratorFactory.Create<IOrchestrateInstanceSettings>();
			instanceSettings.SetInstanceSetting("BcpShareFolderName", @"\\emttest\BCPPath", "kCura.ARM");

			_fileShare.CreateDirectory(RELATIVE_ARCHIVES_LOCATION);

			var request = new
			{
				Contract = new
				{
					RelativityWebApiUrl = "https://emttest/RelativityWebAPI/",
					ArmArchiveLocations = new List<object>
					{
						new
						{
							ArchiveLocationType = 1,
							Location = REMOTE_ARCHIVES_LOCATION
						}
					},
					EmailNotificationSettings = Enumerable.Empty<object>()
				}
			};

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
			var request = new
			{
				Contract = new
				{
					MatterId = RelativityConst.RELATIVITY_TEMPLATE_MATTER_ARTIFACT_ID,
					ArchivePath = archivedWorkspacePath,
					JobPriority = "Medium",
					ResourcePoolId = RelativityConst.DEFAULT_RESOURCE_POOL_ID,
					DatabaseServerId = RelativityConst.DATABASE_SERVER_ID,
					FileRepositoryId = RelativityConst.DEFAULT_FILE_REPOSITORY_ID,
					CacheLocationId = RelativityConst.DEFAULT_CACHE_LOCATION_ID,
					StructuredAnalyticsServerId = RelativityConst.STRUCTURED_ANALYTICS_SERVER_ID,
					ConceptualAnalyticsServerId = RelativityConst.CONCEPTUAL_ANALYTICS_SERVER_ID
				}
			};

			SendAsync("Relativity.ARM/Jobs/Restore/Create", request);

			return 0;
		}

		private static void RunJob(int jobID)
		{
			var request = new
			{
				JobId = jobID
			};

			SendAsync("Relativity.ARM/Jobs/Action/Run", request);
		}

		private static HttpResponseMessage SendAsync(string armUri, object request)
		{
			using (HttpClient client = GetARMClient())
			{
				string serializedRequest = JsonConvert.SerializeObject(request);

				return client.PostAsync(
					new Uri(TestSettingsConfig.RelativityRestUrl, armUri),
					new StringContent(serializedRequest, Encoding.UTF8, "application/json")).Result;
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
