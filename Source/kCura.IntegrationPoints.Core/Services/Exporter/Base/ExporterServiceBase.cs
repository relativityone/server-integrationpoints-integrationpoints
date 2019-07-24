using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter.Validators;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using Relativity.API;
using Relativity.Core;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Base
{
	public abstract class ExporterServiceBase : IExporterService
	{
		protected IDataTransferContext Context;
		protected int RetrievedDataCount;
		protected IQueryFieldLookupRepository QueryFieldLookupRepository;
		protected SourceConfiguration SourceConfiguration;
		protected readonly BaseServiceContext BaseContext;
		protected readonly ExportInitializationResultsDto ExportJobInfo;
		protected readonly FieldMap[] MappedFields;
		protected readonly HashSet<int> LongTextFieldArtifactIds;
		protected readonly HashSet<int> MultipleObjectFieldArtifactIds;
		protected readonly HashSet<int> SingleChoiceFieldsArtifactIds;
		protected readonly IAPILog Logger;
		protected readonly IDocumentRepository DocumentRepository;
		protected readonly IRelativityObjectManager RelativityObjectManager;
		protected readonly IJobStopManager JobStopManager;
		protected readonly int[] FieldArtifactIds;

		internal ExporterServiceBase(
			IDocumentRepository documentRepository,
			IRelativityObjectManager relativityObjectManager,
			IRepositoryFactory sourceRepositoryFactory,
			IRepositoryFactory targetRepositoryFactory,
			IJobStopManager jobStopManager,
			IHelper helper,
			IBaseServiceContextProvider baseServiceContextProvider,
			FieldMap[] mappedFields,
			int startAt,
			SourceConfiguration sourceConfiguration,
			int searchArtifactId)
			: this(mappedFields, jobStopManager, helper)
		{

			DocumentRepository = documentRepository;
			RelativityObjectManager = relativityObjectManager;
			SourceConfiguration = sourceConfiguration;
			BaseContext = baseServiceContextProvider.GetUnversionContext(SourceConfiguration.SourceWorkspaceArtifactId);

			ValidateDestinationFields(targetRepositoryFactory, mappedFields);

			QueryFieldLookupRepository = sourceRepositoryFactory.GetQueryFieldLookupRepository(SourceConfiguration.SourceWorkspaceArtifactId);

			try
			{
				ExportJobInfo = SourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.SavedSearch
					? DocumentRepository.InitializeSearchExportAsync(searchArtifactId, FieldArtifactIds, startAt).GetAwaiter().GetResult()
					: DocumentRepository.InitializeProductionExportAsync(searchArtifactId, FieldArtifactIds, startAt).GetAwaiter().GetResult();
				Logger.LogInformation("Retrieved ExportJobInfo in ExporterServiceBase. Run: {runId}, rows: {rowCount}", ExportJobInfo?.RunID, ExportJobInfo?.RecordCount);
			}
			catch (Exception exception)
			{
				LogCreatingExporterServiceError(exception);
				// NOTE: If we get an exception, we cannot be exactly sure what the real error is,
				// however, it is more than likely that you do not have Export or Saved Search permissions.
				throw new IntegrationPointsException(Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_EXPORT, exception);
			}
		}

		protected ExporterServiceBase(FieldMap[] mappedFields, IJobStopManager jobStopManager, IHelper helper)
		{
			SingleChoiceFieldsArtifactIds = new HashSet<int>();
			MultipleObjectFieldArtifactIds = new HashSet<int>();
			LongTextFieldArtifactIds = new HashSet<int>();
			RetrievedDataCount = 0;
			MappedFields = mappedFields;
			JobStopManager = jobStopManager;
			Logger = helper.GetLoggerFactory().GetLogger().ForContext<RelativityExporterService>();
			FieldArtifactIds = mappedFields.Select(field => int.Parse(field.SourceField.FieldIdentifier)).ToArray();
		}

		public virtual bool HasDataToRetrieve => (TotalRecordsFound > RetrievedDataCount) && !JobStopManager.IsStopRequested();

		public virtual int TotalRecordsFound => (int) ExportJobInfo.RecordCount;

		public abstract IDataTransferContext GetDataTransferContext(IExporterTransferConfiguration transferConfiguration);

		public abstract ArtifactDTO[] RetrieveData(int size);

		public void Dispose()
		{
			if (Context.DataReader != null)
			{
				Context.DataReader.Dispose();
				Context.DataReader = null;
				Context = null;
			}
		}

		private void ValidateDestinationFields(IRepositoryFactory targetRepositoryFactory, FieldMap[] mappedFields)
		{
			IFieldQueryRepository targetFieldQueryRepository = targetRepositoryFactory.GetFieldQueryRepository(SourceConfiguration.TargetWorkspaceArtifactId);
			var destinationFieldsValidator = new DestinationFieldsValidator(targetFieldQueryRepository, Logger);
			destinationFieldsValidator.ValidateDestinationFields(mappedFields);
		}

		#region Logging

		protected void LogCreatingExporterServiceError(Exception exception)
		{
			Logger.LogError(exception, "Failed to create RelativityExporterService.");
		}

		protected virtual void LogRetrievingDataError(Exception ex)
		{
			Logger.LogError(ex, "Error occurred during data retrieval.");
		}

		#endregion
	}
}