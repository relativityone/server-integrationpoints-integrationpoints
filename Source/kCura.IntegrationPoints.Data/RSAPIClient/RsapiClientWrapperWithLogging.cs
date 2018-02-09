using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.Client;
using kCura.Relativity.Client.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.RSAPIClient
{
	public class RsapiClientWrapperWithLogging : IRSAPIClient
	{
		private readonly IRSAPIClient _rsapiClient;
		private readonly IAPILog _logger;
		public RsapiClientWrapperWithLogging(IRSAPIClient rsapiClient, IAPILog logger)
		{
			_rsapiClient = rsapiClient;
			_logger = logger.ForContext<RsapiClientWrapperWithLogging>();

			Failure += OnFailure;
			ProcessFailure += OnProcessFailure;
			RSAPIClientServiceOperationFailed += OnRsapiClientServiceOperationFailed;
			ProcessCompleteWithError += OnProcessCompleteWithError;
		}

		public string Login()
		{
			return LogAndRethrowException(() =>
				_rsapiClient.Login()
			);
		}

		public string TokenLogin(string sessionToken)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.TokenLogin(sessionToken)
			);
		}

		public ReadResult GenerateRelativityAuthenticationToken(APIOptions apiOpt)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.GenerateRelativityAuthenticationToken(apiOpt)
			);
		}

		public ReadResult GetAuthenticationToken(APIOptions apiOpt, string onBehalfOfUserName)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.GetAuthenticationToken(apiOpt, onBehalfOfUserName)
			);
		}

		public void Logout(APIOptions apiOpt)
		{
			LogAndRethrowException(() => 
				_rsapiClient.Logout(apiOpt)
			);
		}

		public string LoginWithCredentials(string username, string password)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.LoginWithCredentials(username, password)
			);
		}

		public ResultSet Create(APIOptions apiOpt, List<ArtifactRequest> artifactRequests)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.Create(apiOpt, artifactRequests)
			);
		}

		public ReadResultSet Read(APIOptions apiOpt, List<ArtifactRequest> artifactRequests)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.Read(apiOpt, artifactRequests)
			);
		}

		public ResultSet Update(APIOptions apiOpt, List<ArtifactRequest> artifactRequests)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.Update(apiOpt, artifactRequests)
			);
		}

		public ResultSet Delete(APIOptions apiOpt, List<ArtifactRequest> artifactRequests)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.Delete(apiOpt, artifactRequests)
			);
		}

		public Dictionary<AdminChoice, int> GetAdminChoiceTypes(APIOptions apiOpt)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.GetAdminChoiceTypes(apiOpt)
			);
		}

		public MassEditResult MassEdit(APIOptions apiOpt, ArtifactRequest templateArtifactRequest, List<int> artifactIDs)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.MassEdit(apiOpt, templateArtifactRequest, artifactIDs)
			);
		}

		public MassCreateResult MassCreate(APIOptions apiOpt, ArtifactRequest templateArtifactRequest, List<ArtifactRequest> artifactRequests)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.MassCreate(apiOpt, templateArtifactRequest, artifactRequests)
			);
		}

		public MassCreateResult MassCreateWithDetails(APIOptions apiOpt, ArtifactRequest templateArtifactRequest,
			List<ArtifactRequest> artifactRequests)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.MassCreateWithDetails(apiOpt, templateArtifactRequest, artifactRequests)
			);
		}

		public MassCreateResult MassCreateWithAPIParameters(APIOptions apiOpt, ArtifactRequest templateArtifactRequest,
			List<ArtifactRequest> artifactRequests, List<APIParameters> apiParms)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.MassCreateWithAPIParameters(apiOpt, templateArtifactRequest, artifactRequests, apiParms)
			);
		}

		public ResultSet MassDelete(APIOptions apiOpt, MassDeleteOptions options, List<int> artifactIDs)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.MassDelete(apiOpt, options, artifactIDs)
			);
		}

		public ResultSet MassDeleteAllObjects(APIOptions apiOpt, MassDeleteOptions options)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.MassDeleteAllObjects(apiOpt, options)
			);
		}

		public ResultSet MassDeleteDocuments(APIOptions apiOpt, DocumentMassDeleteOptions options, List<int> artifactIDs)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.MassDeleteDocuments(apiOpt, options, artifactIDs)
			);
		}

		public ResultSet MassDeleteAllDocuments(APIOptions apiOpt, DocumentMassDeleteOptions options)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.MassDeleteAllDocuments(apiOpt, options)
			);
		}

		public ProcessInformation GetProcessState(APIOptions apiOpt, Guid processID)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.GetProcessState(apiOpt, processID)
			);
		}

		public ProcessOperationResult FlagProcessForCancellationAsync(APIOptions apiOpt, Guid processID)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.FlagProcessForCancellationAsync(apiOpt, processID)
			);
		}

		public ProcessOperationResult CreateBatchesForBatchSetAsync(APIOptions apiOpt, int batchSetArtifactID)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.CreateBatchesForBatchSetAsync(apiOpt, batchSetArtifactID)
			);
		}

		public ProcessOperationResult PurgeBatchesOfBatchSetAsync(APIOptions apiOpt, int batchSetArtifactID)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.PurgeBatchesOfBatchSetAsync(apiOpt, batchSetArtifactID)
			);
		}

		public SerialLicense GetLicense(APIOptions apiOpt, Guid appGuid, string password)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.GetLicense(apiOpt, appGuid, password)
			);
		}

		public ProcessOperationResult InstallApplication(APIOptions apiOpt, AppInstallRequest appInstallRequest)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.InstallApplication(apiOpt, appInstallRequest)
			);
		}

		public ResultSet InstallLibraryApplication(APIOptions apiOpt, AppInstallRequest appInstallRequest)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.InstallLibraryApplication(apiOpt, appInstallRequest)
			);
		}

		public ResultSet ExportApplication(APIOptions apiOpt, AppExportRequest appExportRequest)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.ExportApplication(apiOpt, appExportRequest)
			);
		}

		public ResultSet PushResourceFiles(APIOptions apiOpt, List<ResourceFileRequest> rfPushRequests)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.PushResourceFiles(apiOpt, rfPushRequests)
			);
		}

		public void Clear(FileRequest fileRequest)
		{
			LogAndRethrowException(() => 
				_rsapiClient.Clear(fileRequest)
			);
		}

		public KeyValuePair<DownloadResponse, Stream> Download(FileRequest fileRequest)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.Download(fileRequest)
			);
		}

		public DownloadResponse Download(FileRequest fileRequest, string outputPath)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.Download(fileRequest, outputPath)
			);
		}

		public void Upload(UploadRequest uploadRequest)
		{
			LogAndRethrowException(() => 
				_rsapiClient.Upload(uploadRequest)
			);
		}

		public DownloadURLResponse GetFileFieldDownloadURL(DownloadURLRequest downloadUrlRequest)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.GetFileFieldDownloadURL(downloadUrlRequest)
			);
		}

		public event CancelEventHandler Cancel
		{
			add { LogAndRethrowException(() => _rsapiClient.Cancel += value); }
			remove { LogAndRethrowException(() => _rsapiClient.Cancel -= value); }
		}

		public event DownloadCompleteEventHandler DownloadComplete
		{
			add { LogAndRethrowException(() => _rsapiClient.DownloadComplete += value); }
			remove { LogAndRethrowException(() => _rsapiClient.DownloadComplete -= value); }
		}

		public event FailureEventHandler Failure
		{
			add { LogAndRethrowException(() => _rsapiClient.Failure += value); }
			remove { LogAndRethrowException(() => _rsapiClient.Failure -= value); }
		}

		public event ProgressEventHandler Progress
		{
			add { LogAndRethrowException(() => _rsapiClient.Progress += value); }
			remove { LogAndRethrowException(() => _rsapiClient.Progress -= value); }
		}

		public event UploadCompleteEventHandler UploadComplete
		{
			add { LogAndRethrowException(() => _rsapiClient.UploadComplete += value); }
			remove { LogAndRethrowException(() => _rsapiClient.UploadComplete -= value); }
		}

		public QueryResult Query(APIOptions apiOpt, Query queryObject, int length = 0)
		{
			return LogAndRethrowException(() => _rsapiClient.Query(apiOpt, queryObject, length));
		}

		public QueryResult QuerySubset(APIOptions apiOpt, string queryToken, int start, int length)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.QuerySubset(apiOpt, queryToken, start, length)
			);
		}

		public ExecuteBatchResultSet ExecuteBatch(APIOptions apiOpt, List<Command> commands, TransactionType transType)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.ExecuteBatch(apiOpt, commands, transType)
			);
		}

		public RelativityScriptResult ExecuteRelativityScript(APIOptions apiOpt, int scriptArtifactID, List<RelativityScriptInput> inputs)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.ExecuteRelativityScript(apiOpt, scriptArtifactID, inputs)
			);
		}

		public List<RelativityScriptInputDetails> GetRelativityScriptInputs(APIOptions apiOpt, int scriptArtifactID)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.GetRelativityScriptInputs(apiOpt, scriptArtifactID)
			);
		}

		public ProcessOperationResult MonitorProcessState(APIOptions apiOpt, Guid processID)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.MonitorProcessState(apiOpt, processID)
			);
		}

		public event ProcessCancelEventHandler ProcessCancelled
		{
			add { LogAndRethrowException(() => _rsapiClient.ProcessCancelled += value); }
			remove { LogAndRethrowException(() => _rsapiClient.ProcessCancelled -= value); }
		}

		public event ProcessFailureEventHandler ProcessFailure
		{
			add { LogAndRethrowException(() => _rsapiClient.ProcessFailure += value); }
			remove { LogAndRethrowException(() => _rsapiClient.ProcessFailure -= value); }
		}

		public event ProcessProgressEventHandler ProcessProgress
		{
			add { LogAndRethrowException(() => _rsapiClient.ProcessProgress += value); }
			remove { LogAndRethrowException(() => _rsapiClient.ProcessProgress -= value); }
		}

		public event ProcessCompleteEventHandler ProcessComplete
		{
			add { LogAndRethrowException(() => _rsapiClient.ProcessComplete += value); }
			remove { LogAndRethrowException(() => _rsapiClient.ProcessComplete -= value); }
		}

		public event ProcessCompleteWithErrorEventHandler ProcessCompleteWithError
		{
			add { LogAndRethrowException(() => _rsapiClient.ProcessCompleteWithError += value); }
			remove { LogAndRethrowException(() => _rsapiClient.ProcessCompleteWithError -= value); }
		}

		public void Dispose()
		{
			LogAndRethrowException(() => 
				_rsapiClient.Dispose()
			);
		}

		public OperationResult ValidateEndpoint()
		{
			return LogAndRethrowException(() => 
				_rsapiClient.ValidateEndpoint()
			);
		}

		public List<RelativityScriptInput> ConvertToScriptInputList(List<RelativityScriptInputDetails> inputDetails)
		{
			return LogAndRethrowException(() => 
				_rsapiClient.ConvertToScriptInputList(inputDetails)
			);
		}

		public AuthenticationType AuthType
		{
			get { return LogAndRethrowException(() => _rsapiClient.AuthType); }
			set { LogAndRethrowException(() => _rsapiClient.AuthType = value); }
		}

		public Uri EndpointUri
		{
			get { return LogAndRethrowException(() => _rsapiClient.EndpointUri); }
			set { LogAndRethrowException(() => _rsapiClient.EndpointUri = value); }
		}

		public APIOptions APIOptions
		{
			get { return LogAndRethrowException(() => _rsapiClient.APIOptions); }
			set { LogAndRethrowException(() => _rsapiClient.APIOptions = value); }
		}

		public RepositoryGroup Repositories
		{
			get { return LogAndRethrowException(() => _rsapiClient.Repositories); }
		}

		public event RSAPIClientServiceOperationFailedHandler RSAPIClientServiceOperationFailed
		{
			add { LogAndRethrowException(() => _rsapiClient.RSAPIClientServiceOperationFailed += value); }
			remove { LogAndRethrowException(() => _rsapiClient.RSAPIClientServiceOperationFailed -= value); }
		}

		private T LogAndRethrowException<T>(Func<T> wrappedFunction, [CallerMemberName] string callerName = "")
		{
			try
			{
				return wrappedFunction();
			}
			catch (Exception e)
			{
				throw LogAndCreateException(e, callerName);
			}
		}

		private void LogAndRethrowException(Action wrappedAction, [CallerMemberName] string callerName = "")
		{
			try
			{
				wrappedAction();
			}
			catch (Exception e)
			{
				throw LogAndCreateException(e, callerName);
			}
		}

		private IntegrationPointsException LogAndCreateException(Exception e, string callerName)
		{
			_logger.LogError(e, "Exception occured in IRSAPIClient implementation of {callerName} method", callerName);
			return new IntegrationPointsException($"Exception occured in IRSAPIClient implementation of {callerName} method", e)
			{
				ExceptionSource = IntegrationPointsExceptionSource.RSAPI,
				ShouldAddToErrorsTab = true
			};
		}

		private void OnRsapiClientServiceOperationFailed(object sender, ServiceOperationFailedEventArgs eventArgs)
		{
			string operationName = eventArgs.OperationName;
			string serviceType = eventArgs.ServiceType.ToString();
			_logger.LogError(eventArgs.OperationException, "OnRsapiClientServiceOperationFailed event of RsapiClient was raised. Operation name: {operationName}, service type: {serviceType}",
				operationName, serviceType);
		}

		private void OnProcessFailure(object sender, ProcessFailureEventArgs eventArgs)
		{
			ProcessInformation processInfo = eventArgs.ProcessInformation;
			_logger.LogWarning("OnProcessFailure event of RsapiClient was raised. Process information: {@processInfo}", processInfo);
		}

		private void OnFailure(FailureEventArgs eventArgs)
		{
			TargetField targetField = eventArgs.TargetField;
			_logger.LogError(eventArgs.Exception, "OnFailure event of RsapiClient was raised. Target fields: {@targetField}", targetField);
		}

		private void OnProcessCompleteWithError(object sender, ProcessCompleteWithErrorEventArgs eventArgs)
		{
			ProcessInformation processInfo = eventArgs.ProcessInformation;
			_logger.LogWarning("OnProcessCompleteWithError event of RsapiClient was raised. Process information: {@processInfo}", processInfo);
		}
	}
}
