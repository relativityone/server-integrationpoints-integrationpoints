using System;
using System.Collections.Generic;
using System.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Validation;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Monitoring;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.ImportProvider.Parser;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.WinEDDS.Api;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class ImportServiceManager : ServiceManagerBase
	{
		private readonly IDataReaderFactory _dataReaderFactory;
		private readonly IImportFileLocationService _importFileLocationService;

		public ImportServiceManager(IHelper helper,
			ICaseServiceContext caseServiceContext,
			IContextContainerFactory contextContainerFactory,
			ISynchronizerFactory synchronizerFactory,
			IOnBehalfOfUserClaimsPrincipalFactory onBehalfOfUserClaimsPrincipalFactory,
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
			IAgentValidator agentValidator)
			: base(helper,
				  jobService,
				  serializer,
				  jobHistoryService,
				  jobHistoryErrorService,
				  scheduleRuleFactory,
				  managerFactory,
				  contextContainerFactory,
				  statuses,
				  caseServiceContext,
				  onBehalfOfUserClaimsPrincipalFactory,
				  statisticsService,
				  synchronizerFactory,
				agentValidator)
		{
			Logger = helper.GetLoggerFactory().GetLogger().ForContext<ImportServiceManager>();
			_dataReaderFactory = dataReaderFactory;
			_importFileLocationService = importFileLocationService;
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
			}
		}

		private ImportSettings GetImportApiSettingsObjectForUser(Job job)
		{
			LogGetImportApiSettingsObjectForUserStart(job);
			ImportProviderSettings providerSettings = Serializer.Deserialize<ImportProviderSettings>(IntegrationPointDto.SourceConfiguration);
			ImportSettings importSettings = Serializer.Deserialize<ImportSettings>(IntegrationPointDto.DestinationConfiguration);

			importSettings.OnBehalfOfUserId = job.SubmittedBy;
			importSettings.ErrorFilePath = _importFileLocationService.ErrorFilePath(IntegrationPointDto.ArtifactId);

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
			providerSettings.LoadFile = _importFileLocationService.LoadFileFullPath(IntegrationPointDto.ArtifactId);
			return Serializer.Serialize(providerSettings);
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
