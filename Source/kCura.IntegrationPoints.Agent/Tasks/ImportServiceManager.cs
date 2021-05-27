using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.ImportProvider.Parser;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.WinEDDS.Api;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Relativity.AutomatedWorkflows.Services.Interfaces;
using Relativity.AutomatedWorkflows.Services.Interfaces.DataContracts.Triggers;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;
using kCura.IntegrationPoints.ImportProvider;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class ImportServiceManager : ServiceManagerBase
	{
		private const int _MAX_NUMBER_OF_RAW_RETRIES = 4;

		private const int _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID = 1000014;
		private const string _AUTOMATED_WORKFLOWS_APPLICATION_NAME = "Automated Workflows";

		private readonly IHelper _helper;
		private readonly IRetryHandler _automatedWorkflowsRetryHandler;
		private readonly IDataReaderFactory _dataReaderFactory;
		private readonly IImportFileLocationService _importFileLocationService;
		private readonly IJobStatusUpdater _jobStatusUpdater;

		public const string RAW_STATE_COMPLETE_WITH_ERRORS = "complete-with-errors";
		public const string RAW_STATE_COMPLETE = "complete";
		public const string RAW_RIP_TRIGGER_NAME = "relativity@on-new-documents-added";
		public const string RAW_TRIGGER_INPUT_ID = "type";
		public const string RAW_TRIGGER_INPUT_VALUE = "rip";
		
		public ImportServiceManager(
			IHelper helper,
			IRetryHandlerFactory retryHandlerFactory,
			ICaseServiceContext caseServiceContext,
			ISynchronizerFactory synchronizerFactory,
			IManagerFactory managerFactory,
			IEnumerable<IBatchStatus> statuses,
			ISerializer serializer,
			IJobService jobService,
			IScheduleRuleFactory scheduleRuleFactory,
			IJobHistoryService jobHistoryService,
			IJobHistoryErrorService jobHistoryErrorService,
			JobStatisticsService statisticsService,
			IDataReaderFactory dataReaderFactory,
			IImportFileLocationService importFileLocationService,
			IAgentValidator agentValidator,
			IIntegrationPointRepository integrationPointRepository,
			IJobStatusUpdater jobStatusUpdater)
			: base(
				helper,
				jobService,
				serializer,
				jobHistoryService,
				jobHistoryErrorService,
				scheduleRuleFactory,
				managerFactory,
				statuses,
				caseServiceContext,
				statisticsService,
				synchronizerFactory,
				agentValidator,
				integrationPointRepository)
		{
			_helper = helper;
			_automatedWorkflowsRetryHandler = retryHandlerFactory.Create(_MAX_NUMBER_OF_RAW_RETRIES);
			_dataReaderFactory = dataReaderFactory;
			_importFileLocationService = importFileLocationService;
			_jobStatusUpdater = jobStatusUpdater;
			Logger = _helper.GetLoggerFactory().GetLogger().ForContext<ImportServiceManager>();
		}

		public override void Execute(Job job)
		{
			try
			{
				LogExecuteStart(job);

				InitializeService(job);

				JobStopManager.ThrowIfStopRequested();

				IDataSynchronizer synchronizer = CreateDestinationProvider(IntegrationPointDto.DestinationConfiguration);

				JobStopManager.ThrowIfStopRequested();

				SetupSubscriptions(synchronizer, job);

				JobStopManager.ThrowIfStopRequested();

				ImportSettings settings = GetImportApiSettingsObjectForUser(job);
				string providerSettings = UpdatedProviderSettingsLoadFile();
				int sourceRecordCount = UpdateSourceRecordCount(settings);
				if (sourceRecordCount > 0)
				{
					using (var context = new ImportTransferDataContext(_dataReaderFactory, providerSettings, MappedFields))
					{
						synchronizer.SyncData(context, MappedFields, Serializer.Serialize(settings));
					}
				}
				LogExecuteSuccesfulEnd(job);
			}
			catch (OperationCanceledException e)
			{
				LogJobStoppedException(job, e);
				// ignore error.
			}
			catch (Exception ex)
			{
				HandleGenericException(ex, job);
				if (ex is PermissionException || ex is IntegrationPointValidationException || ex is IntegrationPointsException)
				{
					throw;
				}
			}
			finally
			{
				SetJobStateAsUnstoppable(job);
				JobHistoryErrorService.CommitErrors();
				FinalizeService(job);
				LogExecuteFinalize(job);
				SendAutomatedWorkflowsTriggerAsync(job).GetAwaiter().GetResult();
			}
		}

		protected override void RunValidation(Job job)
		{
			base.RunValidation(job);

			ValidateLoadFile(job);
		}

		private async Task SendAutomatedWorkflowsTriggerAsync(Job job)
		{
			TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
			JobHistory jobHistory = JobHistoryService.GetRdo(taskParameters.BatchInstance);
			ChoiceRef status = _jobStatusUpdater.GenerateStatus(jobHistory);

			if (status.EqualsToChoice(JobStatusChoices.JobHistoryCompleted))
			{
				await SendAutomatedWorkflowsTriggerAsync(job.WorkspaceID, RAW_RIP_TRIGGER_NAME, RAW_STATE_COMPLETE).ConfigureAwait(false);
			}
			else if (status.EqualsToChoice(JobStatusChoices.JobHistoryCompletedWithErrors))
			{
				await SendAutomatedWorkflowsTriggerAsync(job.WorkspaceID, RAW_RIP_TRIGGER_NAME, RAW_STATE_COMPLETE_WITH_ERRORS).ConfigureAwait(false);
			}
		}

		private async Task SendAutomatedWorkflowsTriggerAsync(int workspaceId, string triggerName, string state)
		{
			try
			{
				Logger.LogInformation("For workspace artifact ID : {0} {1} trigger called with status {2}.", workspaceId, triggerName, state);

				if (!await IsAutomatedWorkflowsInstalledAsync(workspaceId).ConfigureAwait(false))
				{
					Logger.LogInformation(_AUTOMATED_WORKFLOWS_APPLICATION_NAME + " isn't installed in workspace {workspaceArtifactId}.", workspaceId);

					return;
				}

				SendTriggerBody body = new SendTriggerBody
				{
					Inputs = new List<TriggerInput>
					{
						new TriggerInput
						{
							ID = RAW_TRIGGER_INPUT_ID,
							Value = RAW_TRIGGER_INPUT_VALUE
						}
					},
					State = state
				};

				await _automatedWorkflowsRetryHandler.ExecuteWithRetriesAsync(async () =>
				{
					using (IAutomatedWorkflowsService triggerProcessor = _helper.GetServicesManager().CreateProxy<IAutomatedWorkflowsService>(ExecutionIdentity.System))
					{
						await triggerProcessor.SendTriggerAsync(workspaceId, triggerName, body).ConfigureAwait(false);
					}
				}).ConfigureAwait(false);

				Logger.LogInformation("For workspace : {0} trigger {1} finished sending.", workspaceId, triggerName);
			}
			catch (Exception ex)
			{
				string message = "Error occured while executing trigger : {0} for workspace artifact ID : {1}";
				Logger.LogError(ex, message, triggerName, workspaceId);
			}
		}

		private async Task<bool> IsAutomatedWorkflowsInstalledAsync(int workspaceId)
		{
			using (IObjectManager objectManager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				QueryRequest automatedWorkflowsInstalledRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef { ArtifactTypeID = _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID },
					Condition = $"'Name' == '{_AUTOMATED_WORKFLOWS_APPLICATION_NAME}'"
				};
				QueryResultSlim automatedWorkflowsInstalledResult = await objectManager.QuerySlimAsync(workspaceId, automatedWorkflowsInstalledRequest, 0, 0).ConfigureAwait(false);

				return automatedWorkflowsInstalledResult.TotalCount > 0;
			}
		}

		private ImportSettings GetImportApiSettingsObjectForUser(Job job)
		{
			LogGetImportApiSettingsObjectForUserStart(job);
			ImportProviderSettings providerSettings = Serializer.Deserialize<ImportProviderSettings>(IntegrationPointDto.SourceConfiguration);
			ImportSettings importSettings = Serializer.Deserialize<ImportSettings>(IntegrationPointDto.DestinationConfiguration);
			importSettings.CorrelationId = ImportSettings.CorrelationId;
			importSettings.JobID = ImportSettings.JobID;
			importSettings.Provider = nameof(ProviderType.ImportLoadFile);
			importSettings.OnBehalfOfUserId = job.SubmittedBy;
			importSettings.ErrorFilePath = _importFileLocationService.ErrorFilePath(IntegrationPointDto);

			//For LoadFile imports, correct an off-by-one error introduced by WinEDDS.LoadFileReader interacting with
			//ImportAPI process. This is introduced by the fact that the first record is the column header row.
			//Opticon files have no column header row
			if (importSettings.ImageImport)
			{
				importSettings.StartRecordNumber = Int32.Parse(providerSettings.LineNumber);
			}
			else
			{
				importSettings.StartRecordNumber = Int32.Parse(providerSettings.LineNumber) + 1;
			}

			importSettings.DestinationFolderArtifactId = providerSettings.DestinationFolderArtifactId;

			//Copy multi-value and nested delimiter settings chosen on configuration page to importAPI settings
			importSettings.MultiValueDelimiter = (char)providerSettings.AsciiMultiLine;
			importSettings.NestedValueDelimiter = (char)providerSettings.AsciiNestedValue;
			LogGetImportApiSettingsObjectForUserSuccesfulEnd(job);
			return importSettings;
		}

		private int UpdateSourceRecordCount(ImportSettings settings)
		{
			LogUpdateSourceRecordCountStart();
			//Cannot re-use the LoadFileDataReader once record count has been obtained (error file is not created properly due to an off-by-one error)
			using (IDataReader sourceReader = _dataReaderFactory.GetDataReader(MappedFields.ToArray(), IntegrationPointDto.SourceConfiguration))
			{
				int recordCount =
					settings.ImageImport ?
					(int)((IOpticonDataReader)sourceReader).CountRecords() :
					(int)((IArtifactReader)sourceReader).CountRecords();

				lock (JobStopManager.SyncRoot)
				{
					JobHistory = JobHistoryService.GetRdo(Identifier);
					JobHistory.TotalItems = recordCount;
					UpdateJobStatus(JobHistory);
				}

				LogUpdateSourceRecordSuccesfulEnd();
				return recordCount;
			}
		}
		
		private string UpdatedProviderSettingsLoadFile()
		{
			ImportProviderSettings providerSettings = Serializer.Deserialize<ImportProviderSettings>(IntegrationPointDto.SourceConfiguration);
			providerSettings.LoadFile = _importFileLocationService.LoadFileFullPath(IntegrationPointDto);
			return Serializer.Serialize(providerSettings);
		}

		private void ValidateLoadFile(Job job)
		{
			TaskParameters parameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
			LoadFileTaskParameters loadFileParameters = (LoadFileTaskParameters)parameters.BatchParameters;

			System.IO.FileInfo loadFile = _importFileLocationService.LoadFileInfo(IntegrationPointDto);

			if(loadFile.Length != loadFileParameters.Size || loadFile.LastWriteTimeUtc != loadFileParameters.ModifiedDate)
			{
				ValidationResult result = new ValidationResult(false, "Load File has been modified.");
				throw new IntegrationPointValidationException(result);
			}
		}

		#region Logging
		private void LogExecuteFinalize(Job job)
		{
			Logger.LogInformation("Finalized execution of job in Import Service Manager. job: {JobId}.", job.JobId);
		}

		private void LogExecuteSuccesfulEnd(Job job)
		{
			Logger.LogInformation("Succesfully finished execution of job in Import Service Manager. job: {JobId}.", job.JobId);
		}

		private void LogExecuteStart(Job job)
		{
			Logger.LogInformation("Starting execution of job in Import Service Manager. job: {JobId}.", job.JobId);
		}

		private void LogGetImportApiSettingsObjectForUserSuccesfulEnd(Job job)
		{
			Logger.LogInformation("Succesfully finished getting Import API settings for user. job: {JobId}.", job.JobId);
		}

		private void LogGetImportApiSettingsObjectForUserStart(Job job)
		{
			Logger.LogInformation("Getting Import API settings for user. job: {JobId}.", job.JobId);
		}

		private void LogUpdateSourceRecordSuccesfulEnd()
		{
			Logger.LogInformation("Succesfully finished updating source record count.");
		}

		private void LogUpdateSourceRecordCountStart()
		{
			Logger.LogInformation("Started updating source record count.");
		}
		#endregion
	}
}
