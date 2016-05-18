using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using kCura.Relativity.Client.Repositories;
using NSubstitute;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.ObjectQuery;
using Relativity.Services.ServiceProxy;
using Context = kCura.Data.RowDataGateway.Context;
using Query = Relativity.Services.ObjectQuery.Query;

namespace kCura.IntegrationPoint.Tests.Core
{
	using global::Relativity.Services.Permission;

	public interface ITestHelper : IHelper
	{
		IPermissionRepository PermissionManager { get; }

		T CreateUserProxy<T>() where T : IDisposable;

		T CreateAdminProxy<T>() where T : IDisposable;
	}

	public class TestHelper : ITestHelper
	{
		private readonly IServicesMgr _serviceManager;
		public IPermissionRepository PermissionManager { get; }

		public TestHelper()
		{
			PermissionManager = Substitute.For<IPermissionRepository>();
			_serviceManager = Substitute.For<IServicesMgr>();
			_serviceManager.CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser).Returns(new ExtendedIRSAPIClient(this, ExecutionIdentity.CurrentUser));
			_serviceManager.CreateProxy<IRSAPIClient>(ExecutionIdentity.System).Returns(new ExtendedIRSAPIClient(this, ExecutionIdentity.System));
			_serviceManager.CreateProxy<IObjectQueryManager>(ExecutionIdentity.System).Returns(new ExtendedIObjectQueryManager(this, ExecutionIdentity.System));
			_serviceManager.CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser).Returns(CreateUserProxy<IPermissionManager>(), CreateUserProxy<IPermissionManager>(), CreateUserProxy<IPermissionManager>());
			_serviceManager.CreateProxy<IObjectQueryManager>(ExecutionIdentity.CurrentUser).Returns(new ExtendedIObjectQueryManager(this, ExecutionIdentity.CurrentUser));
		}

		public T CreateUserProxy<T>() where T : IDisposable
		{
			var userCredential = new global::Relativity.Services.ServiceProxy.UsernamePasswordCredentials(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
			ServiceFactorySettings userSettings = new ServiceFactorySettings(SharedVariables.RsapiClientServiceUri, SharedVariables.RestClientServiceUri, userCredential);
			ServiceFactory userServiceFactory = new ServiceFactory(userSettings);
			return userServiceFactory.CreateProxy<T>();
		}

		public T CreateAdminProxy<T>() where T : IDisposable
		{
			var credential = new global::Relativity.Services.ServiceProxy.UsernamePasswordCredentials("relativity.admin@kcura.com", "P@ssw0rd@1");
			ServiceFactorySettings settings = new ServiceFactorySettings(SharedVariables.RsapiClientServiceUri, SharedVariables.RestClientServiceUri, credential);
			ServiceFactory adminServiceFactory = new ServiceFactory(settings);
			return adminServiceFactory.CreateProxy<T>();
		}

		public void Dispose()
		{
		}

		public IDBContext GetDBContext(int caseId)
		{
			Context baseContext = null;
			if (caseId == -1)
			{
				baseContext = new Context(SharedVariables.EddsConnectionString);
			}
			else
			{

				string connectionString = String.Format(SharedVariables.WorkspaceConnectionStringFormat, caseId);
				baseContext = new Context(connectionString);
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


		public class ExtendedIRSAPIClient : IRSAPIClient
		{
			private readonly ITestHelper _helper;
			private readonly ExecutionIdentity _identity;
			private IRSAPIClient _client;

			public ExtendedIRSAPIClient(ITestHelper helper, ExecutionIdentity identity)
			{
				_helper = helper;
				_identity = identity;
				_client = Rsapi.CreateRsapiClient(identity);
			}

			public string Login()
			{
				return _client.Login();
			}

			public string TokenLogin(string sessionToken)
			{
				return _client.TokenLogin(sessionToken);
			}

			public ReadResult GenerateRelativityAuthenticationToken(APIOptions apiOpt)
			{
				return _client.GenerateRelativityAuthenticationToken(apiOpt);
			}

			public ReadResult GetAuthenticationToken(APIOptions apiOpt, string onBehalfOfUserName)
			{
				return _client.GetAuthenticationToken(apiOpt, onBehalfOfUserName);
			}

			public void Logout(APIOptions apiOpt)
			{
				_client.Logout(apiOpt);
			}

			public string LoginWithCredentials(string username, string password)
			{
				return _client.LoginWithCredentials(username, password);
			}

			public ResultSet Create(APIOptions apiOpt, List<ArtifactRequest> artifactRequests)
			{
				return _client.Create(apiOpt, artifactRequests);
			}

			public ReadResultSet Read(APIOptions apiOpt, List<ArtifactRequest> artifactRequests)
			{
				return _client.Read(apiOpt, artifactRequests);
			}

			public ResultSet Update(APIOptions apiOpt, List<ArtifactRequest> artifactRequests)
			{
				return _client.Update(apiOpt, artifactRequests);
			}

			public ResultSet Delete(APIOptions apiOpt, List<ArtifactRequest> artifactRequests)
			{
				return _client.Delete(apiOpt, artifactRequests);
			}

			public Dictionary<AdminChoice, int> GetAdminChoiceTypes(APIOptions apiOpt)
			{
				return _client.GetAdminChoiceTypes(apiOpt);
			}

			public ResultSet MassDeleteAllObjects(APIOptions apiOpt, MassDeleteOptions options)
			{
				return _client.MassDeleteAllObjects(apiOpt, options);
			}

			public ResultSet MassDeleteAllDocuments(APIOptions apiOpt, DocumentMassDeleteOptions options)
			{
				return _client.MassDeleteAllDocuments(apiOpt, options);
			}

			public ProcessInformation GetProcessState(APIOptions apiOpt, Guid processID)
			{
				return _client.GetProcessState(apiOpt, processID);
			}

			public ProcessOperationResult FlagProcessForCancellationAsync(APIOptions apiOpt, Guid processID)
			{
				return _client.FlagProcessForCancellationAsync(apiOpt, processID);
			}

			public ProcessOperationResult CreateBatchesForBatchSetAsync(APIOptions apiOpt, int batchSetArtifactID)
			{
				return _client.CreateBatchesForBatchSetAsync(apiOpt, batchSetArtifactID);
			}

			public ProcessOperationResult PurgeBatchesOfBatchSetAsync(APIOptions apiOpt, int batchSetArtifactID)
			{
				return _client.PurgeBatchesOfBatchSetAsync(apiOpt, batchSetArtifactID);
			}

			public SerialLicense GetLicense(APIOptions apiOpt, Guid appGuid, string password)
			{
				return _client.GetLicense(apiOpt, appGuid, password);
			}

			public ProcessOperationResult InstallApplication(APIOptions apiOpt, AppInstallRequest appInstallRequest)
			{
				return _client.InstallApplication(apiOpt, appInstallRequest);
			}

			public ResultSet InstallLibraryApplication(APIOptions apiOpt, AppInstallRequest appInstallRequest)
			{
				return _client.InstallLibraryApplication(apiOpt, appInstallRequest);
			}

			public ResultSet ExportApplication(APIOptions apiOpt, AppExportRequest appExportRequest)
			{
				return _client.ExportApplication(apiOpt, appExportRequest);
			}

			public ResultSet PushResourceFiles(APIOptions apiOpt, List<ResourceFileRequest> rfPushRequests)
			{
				return _client.PushResourceFiles(apiOpt, rfPushRequests);
			}

			public ResultSet MassDeleteDocuments(APIOptions apiOpt, DocumentMassDeleteOptions options, List<int> artifactIDs)
			{
				return _client.MassDeleteDocuments(apiOpt, options, artifactIDs);
			}

			public ResultSet MassDelete(APIOptions apiOpt, MassDeleteOptions options, List<int> artifactIDs)
			{
				return _client.MassDelete(apiOpt, options, artifactIDs);
			}

			public MassCreateResult MassCreateWithAPIParameters(APIOptions apiOpt, ArtifactRequest templateArtifactRequest,
				List<ArtifactRequest> artifactRequests, List<APIParameters> apiParms)
			{
				return _client.MassCreateWithAPIParameters(apiOpt, templateArtifactRequest, artifactRequests, apiParms);
			}

			public MassCreateResult MassCreateWithDetails(APIOptions apiOpt, ArtifactRequest templateArtifactRequest, List<ArtifactRequest> artifactRequests)
			{
				return _client.MassCreateWithDetails(apiOpt, templateArtifactRequest, artifactRequests);
			}

			public MassCreateResult MassCreate(APIOptions apiOpt, ArtifactRequest templateArtifactRequest, List<ArtifactRequest> artifactRequests)
			{
				return _client.MassCreate(apiOpt, templateArtifactRequest, artifactRequests);
			}

			public MassEditResult MassEdit(APIOptions apiOpt, ArtifactRequest templateArtifactRequest, List<int> artifactIDs)
			{
				return _client.MassEdit(apiOpt, templateArtifactRequest, artifactIDs);
			}

			public void Clear(FileRequest fileRequest)
			{
				_client.Clear(fileRequest);
			}

			public KeyValuePair<DownloadResponse, Stream> Download(FileRequest fileRequest)
			{
				return _client.Download(fileRequest);
			}

			public DownloadResponse Download(FileRequest fileRequest, string outputPath)
			{
				return _client.Download(fileRequest, outputPath);
			}

			public void Upload(UploadRequest uploadRequest)
			{
				_client.Upload(uploadRequest);
			}

			public DownloadURLResponse GetFileFieldDownloadURL(DownloadURLRequest downloadUrlRequest)
			{
				return _client.GetFileFieldDownloadURL(downloadUrlRequest);
			}

			public event CancelEventHandler Cancel { add { _client.Cancel += value; } remove { _client.Cancel -= value; } }
			public event DownloadCompleteEventHandler DownloadComplete { add { _client.DownloadComplete += value; } remove { _client.DownloadComplete -= value; } }
			public event FailureEventHandler Failure { add { _client.Failure += value; } remove { _client.Failure -= value; } }
			public event ProgressEventHandler Progress { add { _client.Progress += value; } remove { _client.Progress -= value; } }
			public event UploadCompleteEventHandler UploadComplete { add { _client.UploadComplete += value; } remove { _client.UploadComplete -= value; } }
			public QueryResult Query(APIOptions apiOpt, Relativity.Client.Query queryObject, int length = 0)
			{
				return _client.Query(apiOpt, queryObject, length);
			}

			public QueryResult QuerySubset(APIOptions apiOpt, string queryToken, int start, int length)
			{
				return _client.QuerySubset(apiOpt, queryToken, start, length);
			}

			public List<RelativityScriptInputDetails> GetRelativityScriptInputs(APIOptions apiOpt, int scriptArtifactID)
			{
				return _client.GetRelativityScriptInputs(apiOpt, scriptArtifactID);
			}

			public RelativityScriptResult ExecuteRelativityScript(APIOptions apiOpt, int scriptArtifactID, List<RelativityScriptInput> inputs)
			{
				return _client.ExecuteRelativityScript(apiOpt, scriptArtifactID, inputs);
			}

			public ExecuteBatchResultSet ExecuteBatch(APIOptions apiOpt, List<Command> commands, TransactionType transType)
			{
				return _client.ExecuteBatch(apiOpt, commands, transType);
			}

			public ProcessOperationResult MonitorProcessState(APIOptions apiOpt, Guid processID)
			{
				return _client.MonitorProcessState(apiOpt, processID);
			}

			public event ProcessCancelEventHandler ProcessCancelled {  add { _client.ProcessCancelled += value; } remove { _client.ProcessCancelled -= value;} }
			public event ProcessFailureEventHandler ProcessFailure { add { _client.ProcessFailure += value; } remove { _client.ProcessFailure -= value; } }
			public event ProcessProgressEventHandler ProcessProgress { add { _client.ProcessProgress += value; } remove { _client.ProcessProgress -= value; } }
			public event ProcessCompleteEventHandler ProcessComplete { add { _client.ProcessComplete += value; } remove { _client.ProcessComplete -= value; } }
			public event ProcessCompleteWithErrorEventHandler ProcessCompleteWithError { add { _client.ProcessCompleteWithError += value; } remove { _client.ProcessCompleteWithError -= value; } }

			object obj = new object();	
			public void Dispose()
			{
				lock (obj)
				{
					IRSAPIClient newClient = new ExtendedIRSAPIClient(_helper, _identity);
					_client.Dispose();
					_client = newClient;
				}
			}

			public OperationResult ValidateEndpoint()
			{
				return _client.ValidateEndpoint();
			}

			public AuthenticationType AuthType { get { return _client.AuthType; } set { _client.AuthType = value; } }
			public Uri EndpointUri { get { return _client.EndpointUri; } set { _client.EndpointUri = value; } }
			public APIOptions APIOptions { get { return _client.APIOptions; } set { _client.APIOptions = value; } }
			public RepositoryGroup Repositories { get { return _client.Repositories;  } }
			public event RSAPIClientServiceOperationFailedHandler RSAPIClientServiceOperationFailed { add { _client.RSAPIClientServiceOperationFailed += value; } remove { _client.RSAPIClientServiceOperationFailed -= value; } }
			public List<RelativityScriptInput> ConvertToScriptInputList(List<RelativityScriptInputDetails> inputDetails)
			{
				return _client.ConvertToScriptInputList(inputDetails);
			}
		}

		public class ExtendedIObjectQueryManager : IObjectQueryManager
		{
			private readonly ITestHelper _helper;
			private readonly ExecutionIdentity _identity;
			private IObjectQueryManager _manager;

			public ExtendedIObjectQueryManager(ITestHelper helper, ExecutionIdentity identity)
			{
				_helper = helper;
				_identity = identity;
				_manager = helper.CreateUserProxy<IObjectQueryManager>();
			}

			object _lock = new object();
			public void Dispose()
			{
				lock (_lock)
				{
					// create a new Kepler when itself being disposed.
					var newManager =  new ExtendedIObjectQueryManager(_helper, _identity);
					_manager.Dispose();
					_manager = newManager;
				}
			}

			public Task<ObjectQueryResultSet> QueryAsync(int workspaceId, int artifactTypeId, Query query, int start, int length, int[] includePermissions, string queryToken)
			{
				return _manager.QueryAsync(workspaceId, artifactTypeId, query, start, length, includePermissions,  queryToken);
			}

			public Task<ObjectQueryResultSet> QueryAsync(int workspaceId, int artifactTypeId, Query query, int start, int length, int[] includePermissions, string queryToken, IProgress<ProgressReport> progress)
			{
				return _manager.QueryAsync(workspaceId, artifactTypeId, query, start, length, includePermissions, queryToken, progress);
			}

			public Task<ObjectQueryResultSet> QueryAsync(int workspaceId, int artifactTypeId, Query query, int start, int length, int[] includePermissions, string queryToken, CancellationToken cancel)
			{
				return _manager.QueryAsync(workspaceId, artifactTypeId, query, start, length, includePermissions, queryToken, cancel);
			}

			public Task<ObjectQueryResultSet> QueryAsync(int workspaceId, int artifactTypeId, Query query, int start, int length, int[] includePermissions, string queryToken, CancellationToken cancel, IProgress<ProgressReport> progress)
			{
				return _manager.QueryAsync(workspaceId, artifactTypeId, query, start, length, includePermissions, queryToken, cancel, progress);
			}
		}
	}
}