using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using kCura.Relativity.Client.Repositories;
using NSubstitute;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.ObjectQuery;
using Relativity.Services.ServiceProxy;

namespace kCura.IntegrationPoint.Tests.Core
{
	using global::Relativity.Services.Permission;

	public interface ITestHelper : IHelper
	{
		string RelativityUserName { get; set; }

		string RelativityPassword { get; set; }

		IPermissionRepository PermissionManager { get; }

		T CreateUserProxy<T>() where T : IDisposable;

		T CreateAdminProxy<T>() where T : IDisposable;
	}

	public class TestHelper : ITestHelper
	{
		private readonly IServicesMgr _serviceManager;

		public string RelativityUserName { get; set; } = SharedVariables.RelativityUserName;
		public string RelativityPassword { get; set; } = SharedVariables.RelativityPassword;

		public IPermissionRepository PermissionManager { get; }

		public TestHelper()
		{
			PermissionManager = Substitute.For<IPermissionRepository>();
			_serviceManager = Substitute.For<IServicesMgr>();
			_serviceManager.CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser).Returns(new ExtendedIRSAPIClient(this, ExecutionIdentity.CurrentUser));
			_serviceManager.CreateProxy<IRSAPIClient>(ExecutionIdentity.System).Returns(new ExtendedIRSAPIClient(this, ExecutionIdentity.System));
			_serviceManager.CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser).Returns(new ExtendedIPermissionManager(this, ExecutionIdentity.CurrentUser));
			_serviceManager.CreateProxy<IPermissionManager>(ExecutionIdentity.System).Returns(new ExtendedIPermissionManager(this, ExecutionIdentity.System));
			_serviceManager.CreateProxy<IObjectQueryManager>(ExecutionIdentity.System).Returns(new ExtendedIObjectQueryManager(this, ExecutionIdentity.System));
			_serviceManager.CreateProxy<IObjectQueryManager>(ExecutionIdentity.CurrentUser).Returns(new ExtendedIObjectQueryManager(this, ExecutionIdentity.CurrentUser));
		}

		public T CreateUserProxy<T>() where T : IDisposable
		{
			var userCredential = new global::Relativity.Services.ServiceProxy.UsernamePasswordCredentials(RelativityUserName, RelativityPassword);
			ServiceFactorySettings userSettings = new ServiceFactorySettings(SharedVariables.RsapiClientServiceUri, SharedVariables.RestClientServiceUri, userCredential);
			ServiceFactory userServiceFactory = new ServiceFactory(userSettings);
			return userServiceFactory.CreateProxy<T>();
		}

		public T CreateAdminProxy<T>() where T : IDisposable
		{
			var credential = new global::Relativity.Services.ServiceProxy.UsernamePasswordCredentials("relativity.admin@kcura.com", "Test1234!");
			ServiceFactorySettings settings = new ServiceFactorySettings(SharedVariables.RsapiClientServiceUri, SharedVariables.RestClientServiceUri, credential);
			ServiceFactory adminServiceFactory = new ServiceFactory(settings);
			return adminServiceFactory.CreateProxy<T>();
		}

		public void Dispose()
		{
		}

		public IDBContext GetDBContext(int caseId)
		{
			kCura.Data.RowDataGateway.Context baseContext = null;
			if (caseId == -1)
			{
				baseContext = new kCura.Data.RowDataGateway.Context(SharedVariables.EddsConnectionString);
			}
			else
			{

				string connectionString = String.Format(SharedVariables.WorkspaceConnectionStringFormat, caseId);
				baseContext = new kCura.Data.RowDataGateway.Context(connectionString);
			}
			DBContext context = new DBContext(baseContext);
			return context;
		}

		public IUrlHelper GetUrlHelper()
		{
			throw new NotImplementedException();
		}

		public ILogFactory GetLoggerFactory()
		{
			throw new NotImplementedException();
		}

		public IServicesMgr GetServicesManager()
		{
			return _serviceManager;
		}

		#region Extended classes

		public class ExtendedIRSAPIClient : IRSAPIClient
		{
			private readonly ITestHelper _helper;
			private readonly ExecutionIdentity _identity;
			private Lazy<IRSAPIClient> _clientWrapper;
			private IRSAPIClient Client => _clientWrapper.Value;

			public ExtendedIRSAPIClient(ITestHelper helper, ExecutionIdentity identity)
			{
				_helper = helper;
				_identity = identity;
				_clientWrapper = new Lazy<IRSAPIClient>(() => Rsapi.CreateRsapiClient(identity));
			}

			public string Login()
			{
				return Client.Login();
			}

			public string TokenLogin(string sessionToken)
			{
				return Client.TokenLogin(sessionToken);
			}

			public ReadResult GenerateRelativityAuthenticationToken(APIOptions apiOpt)
			{
				return Client.GenerateRelativityAuthenticationToken(apiOpt);
			}

			public ReadResult GetAuthenticationToken(APIOptions apiOpt, string onBehalfOfUserName)
			{
				return Client.GetAuthenticationToken(apiOpt, onBehalfOfUserName);
			}

			public void Logout(APIOptions apiOpt)
			{
				Client.Logout(apiOpt);
			}

			public string LoginWithCredentials(string username, string password)
			{
				return Client.LoginWithCredentials(username, password);
			}

			public ResultSet Create(APIOptions apiOpt, List<ArtifactRequest> artifactRequests)
			{
				return Client.Create(apiOpt, artifactRequests);
			}

			public ReadResultSet Read(APIOptions apiOpt, List<ArtifactRequest> artifactRequests)
			{
				return Client.Read(apiOpt, artifactRequests);
			}

			public ResultSet Update(APIOptions apiOpt, List<ArtifactRequest> artifactRequests)
			{
				return Client.Update(apiOpt, artifactRequests);
			}

			public ResultSet Delete(APIOptions apiOpt, List<ArtifactRequest> artifactRequests)
			{
				return Client.Delete(apiOpt, artifactRequests);
			}

			public Dictionary<AdminChoice, int> GetAdminChoiceTypes(APIOptions apiOpt)
			{
				return Client.GetAdminChoiceTypes(apiOpt);
			}

			public ResultSet MassDeleteAllObjects(APIOptions apiOpt, MassDeleteOptions options)
			{
				return Client.MassDeleteAllObjects(apiOpt, options);
			}

			public ResultSet MassDeleteAllDocuments(APIOptions apiOpt, DocumentMassDeleteOptions options)
			{
				return Client.MassDeleteAllDocuments(apiOpt, options);
			}

			public ProcessInformation GetProcessState(APIOptions apiOpt, Guid processID)
			{
				return Client.GetProcessState(apiOpt, processID);
			}

			public ProcessOperationResult FlagProcessForCancellationAsync(APIOptions apiOpt, Guid processID)
			{
				return Client.FlagProcessForCancellationAsync(apiOpt, processID);
			}

			public ProcessOperationResult CreateBatchesForBatchSetAsync(APIOptions apiOpt, int batchSetArtifactID)
			{
				return Client.CreateBatchesForBatchSetAsync(apiOpt, batchSetArtifactID);
			}

			public ProcessOperationResult PurgeBatchesOfBatchSetAsync(APIOptions apiOpt, int batchSetArtifactID)
			{
				return Client.PurgeBatchesOfBatchSetAsync(apiOpt, batchSetArtifactID);
			}

			public SerialLicense GetLicense(APIOptions apiOpt, Guid appGuid, string password)
			{
				return Client.GetLicense(apiOpt, appGuid, password);
			}

			public ProcessOperationResult InstallApplication(APIOptions apiOpt, AppInstallRequest appInstallRequest)
			{
				return Client.InstallApplication(apiOpt, appInstallRequest);
			}

			public ResultSet InstallLibraryApplication(APIOptions apiOpt, AppInstallRequest appInstallRequest)
			{
				return Client.InstallLibraryApplication(apiOpt, appInstallRequest);
			}

			public ResultSet ExportApplication(APIOptions apiOpt, AppExportRequest appExportRequest)
			{
				return Client.ExportApplication(apiOpt, appExportRequest);
			}

			public ResultSet PushResourceFiles(APIOptions apiOpt, List<ResourceFileRequest> rfPushRequests)
			{
				return Client.PushResourceFiles(apiOpt, rfPushRequests);
			}

			public ResultSet MassDeleteDocuments(APIOptions apiOpt, DocumentMassDeleteOptions options, List<int> artifactIDs)
			{
				return Client.MassDeleteDocuments(apiOpt, options, artifactIDs);
			}

			public ResultSet MassDelete(APIOptions apiOpt, MassDeleteOptions options, List<int> artifactIDs)
			{
				return Client.MassDelete(apiOpt, options, artifactIDs);
			}

			public MassCreateResult MassCreateWithAPIParameters(APIOptions apiOpt, ArtifactRequest templateArtifactRequest,
				List<ArtifactRequest> artifactRequests, List<APIParameters> apiParms)
			{
				return Client.MassCreateWithAPIParameters(apiOpt, templateArtifactRequest, artifactRequests, apiParms);
			}

			public MassCreateResult MassCreateWithDetails(APIOptions apiOpt, ArtifactRequest templateArtifactRequest, List<ArtifactRequest> artifactRequests)
			{
				return Client.MassCreateWithDetails(apiOpt, templateArtifactRequest, artifactRequests);
			}

			public MassCreateResult MassCreate(APIOptions apiOpt, ArtifactRequest templateArtifactRequest, List<ArtifactRequest> artifactRequests)
			{
				return Client.MassCreate(apiOpt, templateArtifactRequest, artifactRequests);
			}

			public MassEditResult MassEdit(APIOptions apiOpt, ArtifactRequest templateArtifactRequest, List<int> artifactIDs)
			{
				return Client.MassEdit(apiOpt, templateArtifactRequest, artifactIDs);
			}

			public void Clear(FileRequest fileRequest)
			{
				Client.Clear(fileRequest);
			}

			public KeyValuePair<DownloadResponse, Stream> Download(FileRequest fileRequest)
			{
				return Client.Download(fileRequest);
			}

			public DownloadResponse Download(FileRequest fileRequest, string outputPath)
			{
				return Client.Download(fileRequest, outputPath);
			}

			public void Upload(UploadRequest uploadRequest)
			{
				Client.Upload(uploadRequest);
			}

			public DownloadURLResponse GetFileFieldDownloadURL(DownloadURLRequest downloadUrlRequest)
			{
				return Client.GetFileFieldDownloadURL(downloadUrlRequest);
			}

			public event CancelEventHandler Cancel { add { Client.Cancel += value; } remove { Client.Cancel -= value; } }
			public event DownloadCompleteEventHandler DownloadComplete { add { Client.DownloadComplete += value; } remove { Client.DownloadComplete -= value; } }
			public event FailureEventHandler Failure { add { Client.Failure += value; } remove { Client.Failure -= value; } }
			public event ProgressEventHandler Progress { add { Client.Progress += value; } remove { Client.Progress -= value; } }
			public event UploadCompleteEventHandler UploadComplete { add { Client.UploadComplete += value; } remove { Client.UploadComplete -= value; } }
			public QueryResult Query(APIOptions apiOpt, Relativity.Client.Query queryObject, int length = 0)
			{
				return Client.Query(apiOpt, queryObject, length);
			}

			public QueryResult QuerySubset(APIOptions apiOpt, string queryToken, int start, int length)
			{
				return Client.QuerySubset(apiOpt, queryToken, start, length);
			}

			public List<RelativityScriptInputDetails> GetRelativityScriptInputs(APIOptions apiOpt, int scriptArtifactID)
			{
				return Client.GetRelativityScriptInputs(apiOpt, scriptArtifactID);
			}

			public RelativityScriptResult ExecuteRelativityScript(APIOptions apiOpt, int scriptArtifactID, List<RelativityScriptInput> inputs)
			{
				return Client.ExecuteRelativityScript(apiOpt, scriptArtifactID, inputs);
			}

			public ExecuteBatchResultSet ExecuteBatch(APIOptions apiOpt, List<Command> commands, TransactionType transType)
			{
				return Client.ExecuteBatch(apiOpt, commands, transType);
			}

			public ProcessOperationResult MonitorProcessState(APIOptions apiOpt, Guid processID)
			{
				return Client.MonitorProcessState(apiOpt, processID);
			}

			public event ProcessCancelEventHandler ProcessCancelled {  add { Client.ProcessCancelled += value; } remove { Client.ProcessCancelled -= value;} }
			public event ProcessFailureEventHandler ProcessFailure { add { Client.ProcessFailure += value; } remove { Client.ProcessFailure -= value; } }
			public event ProcessProgressEventHandler ProcessProgress { add { Client.ProcessProgress += value; } remove { Client.ProcessProgress -= value; } }
			public event ProcessCompleteEventHandler ProcessComplete { add { Client.ProcessComplete += value; } remove { Client.ProcessComplete -= value; } }
			public event ProcessCompleteWithErrorEventHandler ProcessCompleteWithError { add { Client.ProcessCompleteWithError += value; } remove { Client.ProcessCompleteWithError -= value; } }

			object obj = new object();	
			public void Dispose()
			{
				lock (obj)
				{
					Client.Dispose();
					_clientWrapper = new Lazy<IRSAPIClient>(() => Rsapi.CreateRsapiClient(_identity));
				}
			}

			public OperationResult ValidateEndpoint()
			{
				return Client.ValidateEndpoint();
			}

			public AuthenticationType AuthType { get { return Client.AuthType; } set { Client.AuthType = value; } }
			public Uri EndpointUri { get { return Client.EndpointUri; } set { Client.EndpointUri = value; } }
			public APIOptions APIOptions { get { return Client.APIOptions; } set { Client.APIOptions = value; } }
			public RepositoryGroup Repositories { get { return Client.Repositories;  } }
			public event RSAPIClientServiceOperationFailedHandler RSAPIClientServiceOperationFailed { add { Client.RSAPIClientServiceOperationFailed += value; } remove { Client.RSAPIClientServiceOperationFailed -= value; } }
			public List<RelativityScriptInput> ConvertToScriptInputList(List<RelativityScriptInputDetails> inputDetails)
			{
				return Client.ConvertToScriptInputList(inputDetails);
			}
		}

		public class ExtendedIObjectQueryManager : IObjectQueryManager
		{
			private readonly ITestHelper _helper;
			private readonly ExecutionIdentity _identity;
			private Lazy<IObjectQueryManager> _managerWrapper;
			private IObjectQueryManager Manager => _managerWrapper.Value;

			public ExtendedIObjectQueryManager(ITestHelper helper, ExecutionIdentity identity)
			{
				_helper = helper;
				_identity = identity;
				_managerWrapper = new Lazy<IObjectQueryManager>(helper.CreateUserProxy<IObjectQueryManager>);
			}

			object _lock = new object();
			public void Dispose()
			{
				lock (_lock)
				{
					// create a new Kepler when itself being disposed.
					Manager.Dispose();
					_managerWrapper = new Lazy<IObjectQueryManager>(_helper.CreateUserProxy<IObjectQueryManager>);
				}
			}

			public Task<ObjectQueryResultSet> QueryAsync(int workspaceId, int artifactTypeId, global::Relativity.Services.ObjectQuery.Query query, int start, int length, int[] includePermissions, string queryToken)
			{
				return Manager.QueryAsync(workspaceId, artifactTypeId, query, start, length, includePermissions,  queryToken);
			}

			public Task<ObjectQueryResultSet> QueryAsync(int workspaceId, int artifactTypeId, global::Relativity.Services.ObjectQuery.Query query, int start, int length, int[] includePermissions, string queryToken, IProgress<ProgressReport> progress)
			{
				return Manager.QueryAsync(workspaceId, artifactTypeId, query, start, length, includePermissions, queryToken, progress);
			}

			public Task<ObjectQueryResultSet> QueryAsync(int workspaceId, int artifactTypeId, global::Relativity.Services.ObjectQuery.Query query, int start, int length, int[] includePermissions, string queryToken, CancellationToken cancel)
			{
				return Manager.QueryAsync(workspaceId, artifactTypeId, query, start, length, includePermissions, queryToken, cancel);
			}

			public Task<ObjectQueryResultSet> QueryAsync(int workspaceId, int artifactTypeId, global::Relativity.Services.ObjectQuery.Query query, int start, int length, int[] includePermissions, string queryToken, CancellationToken cancel, IProgress<ProgressReport> progress)
			{
				return Manager.QueryAsync(workspaceId, artifactTypeId, query, start, length, includePermissions, queryToken, cancel, progress);
			}
		}

		public class ExtendedIPermissionManager : IPermissionManager
		{
			private readonly ITestHelper _helper;
			private readonly ExecutionIdentity _identity;
			private Lazy<IPermissionManager> _managerWrapper;
			private IPermissionManager Manager => _managerWrapper.Value;

			public ExtendedIPermissionManager(ITestHelper helper, ExecutionIdentity identity)
			{
				_helper = helper;
				_identity = identity;
				_managerWrapper = new Lazy<IPermissionManager>(helper.CreateUserProxy<IPermissionManager>);
			}

			public async Task AddRemoveAdminGroupsAsync(GroupSelector groupSelector)
			{
				await Manager.AddRemoveAdminGroupsAsync(groupSelector).ConfigureAwait(false);
			}

			public async Task AddRemoveItemGroupsAsync(int workspaceArtifactID, int artifactID, GroupSelector groupSelector)
			{
				await Manager.AddRemoveItemGroupsAsync(workspaceArtifactID, artifactID, groupSelector).ConfigureAwait(false);
			}

			public async Task AddRemoveWorkspaceGroupsAsync(int workspaceArtifactID, GroupSelector groupSelector)
			{
				await Manager.AddRemoveWorkspaceGroupsAsync(workspaceArtifactID, groupSelector).ConfigureAwait(false);
			}

			public async Task<int> CreateSingleAsync(int workspaceArtifactID, global::Relativity.Services.Permission.Permission permissionDTO)
			{
				return await Manager.CreateSingleAsync(workspaceArtifactID, permissionDTO).ConfigureAwait(false);
			}

			public async Task DeleteSingleAsync(int workspaceArtifactID, int permissionID)
			{
				await Manager.DeleteSingleAsync(workspaceArtifactID, permissionID).ConfigureAwait(false);
			}

			object obj = new object();
			public void Dispose()
			{
				lock (obj)
				{
					Manager.Dispose();
					_managerWrapper = new Lazy<IPermissionManager>(_helper.CreateUserProxy<IPermissionManager>);
				}
			}

			public async Task<GroupPermissions> GetAdminGroupPermissionsAsync(global::Relativity.Services.Group.GroupRef group)
			{
				return await Manager.GetAdminGroupPermissionsAsync(group).ConfigureAwait(false);
			}

			public async Task<GroupSelector> GetAdminGroupSelectorAsync()
			{
				return await Manager.GetAdminGroupSelectorAsync().ConfigureAwait(false);
			}

			public async Task<List<global::Relativity.Services.User.UserRef>> GetAdminGroupUsersAsync(global::Relativity.Services.Group.GroupRef group)
			{
				return await Manager.GetAdminGroupUsersAsync(group).ConfigureAwait(false);
			}

			public async Task<GroupPermissions> GetItemGroupPermissionsAsync(int workspaceArtifactID, int artifactID, global::Relativity.Services.Group.GroupRef group)
			{
				return await Manager.GetItemGroupPermissionsAsync(workspaceArtifactID, artifactID, group).ConfigureAwait(false);
			}

			public async Task<GroupSelector> GetItemGroupSelectorAsync(int workspaceArtifactID, int artifactID)
			{
				return await Manager.GetItemGroupSelectorAsync(workspaceArtifactID, artifactID).ConfigureAwait(false);
			}

			public async Task<List<global::Relativity.Services.User.UserRef>> GetItemGroupUsersAsync(int workspaceArtifactID, int artifactID, global::Relativity.Services.Group.GroupRef group)
			{
				return await Manager.GetItemGroupUsersAsync(workspaceArtifactID, artifactID, group).ConfigureAwait(false);
			}

			public async Task<ItemLevelSecurity> GetItemLevelSecurityAsync(int workspaceArtifactID, int artifactID)
			{
				return await Manager.GetItemLevelSecurityAsync(workspaceArtifactID, artifactID).ConfigureAwait(false);
			}

			public async Task<Dictionary<int, ItemLevelSecurity>> GetItemLevelSecurityListAsync(int workspaceArtifactID, IEnumerable<int> artifactIDs)
			{
				return await Manager.GetItemLevelSecurityListAsync(workspaceArtifactID, artifactIDs).ConfigureAwait(false);
			}

			public async Task<List<PermissionValue>> GetPermissionSelectedAsync(int workspaceArtifactID, List<PermissionRef> permissions)
			{
				return await Manager.GetPermissionSelectedAsync(workspaceArtifactID, permissions).ConfigureAwait(false);
			}

			public async Task<List<PermissionValue>> GetPermissionSelectedAsync(int workspaceArtifactID, List<PermissionRef> permissions, int artifactID)
			{
				return await Manager.GetPermissionSelectedAsync(workspaceArtifactID, permissions, artifactID).ConfigureAwait(false);
			}

			public async Task<List<PermissionValue>> GetPermissionSelectedForGroupAsync(int workspaceArtifactID, List<PermissionRef> permissions, global::Relativity.Services.Group.GroupRef group)
			{
				return await Manager.GetPermissionSelectedForGroupAsync(workspaceArtifactID, permissions, group).ConfigureAwait(false);
			}

			public async Task<List<PermissionValue>> GetPermissionSelectedForGroupAsync(int workspaceArtifactID, List<PermissionRef> permissions, global::Relativity.Services.Group.GroupRef group, int artifactID)
			{
				return await Manager.GetPermissionSelectedForGroupAsync(workspaceArtifactID, permissions, group, workspaceArtifactID).ConfigureAwait(false);
			}

			public async Task<Dictionary<int, List<PermissionValue>>> GetPermissionSelectedListAsync(int workspaceArtifactID, List<PermissionRef> permissions, IEnumerable<int> artifactIDs)
			{
				return await Manager.GetPermissionSelectedListAsync(workspaceArtifactID, permissions, artifactIDs).ConfigureAwait(false);
			}

			public async Task<GroupPermissions> GetWorkspaceGroupPermissionsAsync(int workspaceArtifactID, global::Relativity.Services.Group.GroupRef group)
			{
				return await Manager.GetWorkspaceGroupPermissionsAsync(workspaceArtifactID, group).ConfigureAwait(false);
			}

			public async Task<GroupSelector> GetWorkspaceGroupSelectorAsync(int workspaceArtifactID)
			{
				return await Manager.GetWorkspaceGroupSelectorAsync(workspaceArtifactID).ConfigureAwait(false);
			}

			public async Task<List<global::Relativity.Services.User.UserRef>> GetWorkspaceGroupUsersAsync(int workspaceArtifactID, global::Relativity.Services.Group.GroupRef group)
			{
				return await Manager.GetWorkspaceGroupUsersAsync(workspaceArtifactID, group).ConfigureAwait(false);
			}

			public async Task<PermissionQueryResultSet> QueryAsync(int workspaceArtifactID, global::Relativity.Services.Query query)
			{
				return await Manager.QueryAsync(workspaceArtifactID, query).ConfigureAwait(false);
			}

			public async Task<PermissionQueryResultSet> QueryAsync(int workspaceArtifactID, global::Relativity.Services.Query query, int length)
			{
				return await Manager.QueryAsync(workspaceArtifactID, query, length).ConfigureAwait(false);
			}

			public async Task<PermissionQueryResultSet> QuerySubsetAsync(int workspaceArtifactID, string queryToken, int start, int length)
			{
				return await Manager.QuerySubsetAsync(workspaceArtifactID, queryToken, start, length).ConfigureAwait(false);
			}

			public async Task<global::Relativity.Services.Permission.Permission> ReadSingleAsync(int workspaceArtifactID, int permissionID)
			{
				return await Manager.ReadSingleAsync(workspaceArtifactID, permissionID).ConfigureAwait(false);
			}

			public async Task SetAdminGroupPermissionsAsync(GroupPermissions groupPermissions)
			{
				await Manager.SetAdminGroupPermissionsAsync(groupPermissions).ConfigureAwait(false);
			}

			public async Task SetItemGroupPermissionsAsync(int workspaceArtifactID, GroupPermissions groupPermissions)
			{
				await Manager.SetItemGroupPermissionsAsync(workspaceArtifactID, groupPermissions).ConfigureAwait(false);
			}

			public async Task SetItemLevelSecurityAsync(int workspaceArtifactID, ItemLevelSecurity itemLevelSecurity)
			{
				await Manager.SetItemLevelSecurityAsync(workspaceArtifactID, itemLevelSecurity).ConfigureAwait(false);
			}

			public async Task SetPermissionSelectedForGroupAsync(int workspaceArtifactID, List<PermissionValue> permissionValues, global::Relativity.Services.Group.GroupRef group)
			{
				await Manager.SetPermissionSelectedForGroupAsync(workspaceArtifactID, permissionValues, group).ConfigureAwait(false);
			}

			public async Task SetPermissionSelectedForGroupAsync(int workspaceArtifactID, List<PermissionValue> permissionValues, global::Relativity.Services.Group.GroupRef group, int artifactID)
			{
				await Manager.SetPermissionSelectedForGroupAsync(workspaceArtifactID, permissionValues, group, artifactID).ConfigureAwait(false);
			}

			public async Task SetWorkspaceGroupPermissionsAsync(int workspaceArtifactID, GroupPermissions groupPermissions)
			{
				await Manager.SetWorkspaceGroupPermissionsAsync(workspaceArtifactID, groupPermissions).ConfigureAwait(false);
			}

			public async Task UpdateSingleAsync(int workspaceArtifactID, global::Relativity.Services.Permission.Permission permissionDTO)
			{
				await Manager.UpdateSingleAsync(workspaceArtifactID, permissionDTO).ConfigureAwait(false);
			}
		}
		#endregion
	}
}