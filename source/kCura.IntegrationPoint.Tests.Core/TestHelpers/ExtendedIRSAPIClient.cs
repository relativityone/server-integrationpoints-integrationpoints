using System;
using System.Collections.Generic;
using System.IO;
using kCura.Relativity.Client;
using kCura.Relativity.Client.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
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

		public ProcessInformation GetProcessState(APIOptions apiOpt, Guid processId)
		{
			return Client.GetProcessState(apiOpt, processId);
		}

		public ProcessOperationResult FlagProcessForCancellationAsync(APIOptions apiOpt, Guid processId)
		{
			return Client.FlagProcessForCancellationAsync(apiOpt, processId);
		}

		public ProcessOperationResult CreateBatchesForBatchSetAsync(APIOptions apiOpt, int batchSetArtifactId)
		{
			return Client.CreateBatchesForBatchSetAsync(apiOpt, batchSetArtifactId);
		}

		public ProcessOperationResult PurgeBatchesOfBatchSetAsync(APIOptions apiOpt, int batchSetArtifactId)
		{
			return Client.PurgeBatchesOfBatchSetAsync(apiOpt, batchSetArtifactId);
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

		public QueryResult Query(APIOptions apiOpt, Query queryObject, int length = 0)
		{
			return Client.Query(apiOpt, queryObject, length);
		}

		public QueryResult QuerySubset(APIOptions apiOpt, string queryToken, int start, int length)
		{
			return Client.QuerySubset(apiOpt, queryToken, start, length);
		}

		public List<RelativityScriptInputDetails> GetRelativityScriptInputs(APIOptions apiOpt, int scriptArtifactId)
		{
			return Client.GetRelativityScriptInputs(apiOpt, scriptArtifactId);
		}

		public RelativityScriptResult ExecuteRelativityScript(APIOptions apiOpt, int scriptArtifactId, List<RelativityScriptInput> inputs)
		{
			return Client.ExecuteRelativityScript(apiOpt, scriptArtifactId, inputs);
		}

		public ExecuteBatchResultSet ExecuteBatch(APIOptions apiOpt, List<Command> commands, TransactionType transType)
		{
			return Client.ExecuteBatch(apiOpt, commands, transType);
		}

		public ProcessOperationResult MonitorProcessState(APIOptions apiOpt, Guid processId)
		{
			return Client.MonitorProcessState(apiOpt, processId);
		}

		public event ProcessCancelEventHandler ProcessCancelled { add { Client.ProcessCancelled += value; } remove { Client.ProcessCancelled -= value; } }

		public event ProcessFailureEventHandler ProcessFailure { add { Client.ProcessFailure += value; } remove { Client.ProcessFailure -= value; } }

		public event ProcessProgressEventHandler ProcessProgress { add { Client.ProcessProgress += value; } remove { Client.ProcessProgress -= value; } }

		public event ProcessCompleteEventHandler ProcessComplete { add { Client.ProcessComplete += value; } remove { Client.ProcessComplete -= value; } }

		public event ProcessCompleteWithErrorEventHandler ProcessCompleteWithError { add { Client.ProcessCompleteWithError += value; } remove { Client.ProcessCompleteWithError -= value; } }

		private readonly object _obj = new object();

		public void Dispose()
		{
			lock (_obj)
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
		public RepositoryGroup Repositories => Client.Repositories;

		public event RSAPIClientServiceOperationFailedHandler RSAPIClientServiceOperationFailed { add { Client.RSAPIClientServiceOperationFailed += value; } remove { Client.RSAPIClientServiceOperationFailed -= value; } }

		public List<RelativityScriptInput> ConvertToScriptInputList(List<RelativityScriptInputDetails> inputDetails)
		{
			return Client.ConvertToScriptInputList(inputDetails);
		}
	}
}
