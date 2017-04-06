using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain;
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
		IDataReaderFactory _dataReaderFactory;
		IImportFileLocationService _importFileLocationService;

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
			IImportFileLocationService importFileLocationService)
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
				  synchronizerFactory)
		{
			Logger = helper.GetLoggerFactory().GetLogger().ForContext<ImportServiceManager>();
			_dataReaderFactory = dataReaderFactory;
			_importFileLocationService = importFileLocationService;
		}

		public override void Execute(Job job)
		{
			try
			{
				InitializeService(job);

				JobStopManager.ThrowIfStopRequested();

				IDataSynchronizer synchronizer = CreateDestinationProvider(IntegrationPointDto.DestinationConfiguration);

				JobStopManager.ThrowIfStopRequested();

				SetupSubscriptions(synchronizer, job);

				JobStopManager.ThrowIfStopRequested();

				ImportSettings settings = GetImportApiSettingsObjectForUser(job);
				string providerSettings = UpdatedProviderSettingsLoadFile();
				if (UpdateSourceRecordCount(settings) > 0)
				{
					using (ImportTransferDataContext context = new ImportTransferDataContext(_dataReaderFactory,
						settings,
						providerSettings,
						MappedFields))
					{
						synchronizer.SyncData(context, MappedFields, Serializer.Serialize(settings));
					}
				}
			}
			catch (OperationCanceledException e)
			{
				LogJobStoppedException(job, e);
				// ignore error.
			}
			catch (Exception ex)
			{
				LogExecutingTaskError(job, ex);
				Result.Status = TaskStatusEnum.Fail;
				JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}
			finally
			{
				SetJobStateAsUnstoppable(job);
				JobHistoryErrorService.CommitErrors();
				FinalizeService(job);
			}
		}

		protected override void SetupSubscriptions(IDataSynchronizer synchronizer, Job job)
		{
			StatisticsService?.Subscribe(synchronizer as IBatchReporter, job);
			JobHistoryErrorService.SubscribeToBatchReporterEvents(synchronizer);
		}

		private ImportSettings GetImportApiSettingsObjectForUser(Job job)
		{
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

			return importSettings;
		}

		private int UpdateSourceRecordCount(ImportSettings settings)
		{
			//Cannot re-use the LoadFileDataReader once record count has been obtained (error file is not created properly due to an off-by-one error)
			using (IDataReader sourceReader = _dataReaderFactory.GetDataReader(MappedFields.ToArray(), IntegrationPointDto.SourceConfiguration))
			{
				int recordCount =
					settings.ImageImport ?
					(int)((IOpticonDataReader)sourceReader).CountRecords() :
					(int)((IArtifactReader)sourceReader).CountRecords();

				lock (JobStopManager.SyncRoot)
				{
					JobHistoryDto = JobHistoryService.GetRdo(Identifier);
					JobHistoryDto.TotalItems = recordCount;
					UpdateJobStatus();
				}

				return recordCount;
			}
		}

		private string UpdatedProviderSettingsLoadFile()
		{
			ImportProviderSettings providerSettings = Serializer.Deserialize<ImportProviderSettings>(IntegrationPointDto.SourceConfiguration);
			providerSettings.LoadFile = _importFileLocationService.LoadFileFullPath(IntegrationPointDto.ArtifactId);
			return Serializer.Serialize(providerSettings);
		}
	}
}
