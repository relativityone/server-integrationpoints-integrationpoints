using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter.Validators;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using Newtonsoft.Json;
using Relativity;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Api.Shared.Manager.Export;
using Relativity.Data;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Base
{
	public abstract class ExporterServiceBase : IExporterService
	{
		protected DataGridContext DataGridContext;
		protected IDataTransferContext Context;
		protected int RetrievedDataCount;
		protected IQueryFieldLookupRepository QueryFieldLookupRepository;
		protected SourceConfiguration SourceConfiguration;
		protected readonly BaseServiceContext BaseContext;
		protected readonly Export.InitializationResults ExportJobInfo;
		protected readonly FieldMap[] MappedFields;
		protected readonly HashSet<int> LongTextFieldArtifactIds;
		protected readonly HashSet<int> MultipleObjectFieldArtifactIds;
		protected readonly HashSet<int> SingleChoiceFieldsArtifactIds;
		protected readonly IAPILog Logger;
		protected readonly IExporter Exporter;
		protected readonly IRelativityObjectManager RelativityObjectManager;
		protected readonly IJobStopManager JobStopManager;
		protected readonly int[] ArtifactViewFieldIds;
		protected readonly int[] FieldArtifactIds;


		/// <summary>
		///     Testing only
		/// </summary>
		protected ExporterServiceBase(
			IExporter exporter,
			IRelativityObjectManager relativityObjectManager,
			IJobStopManager jobStopManager,
			IHelper helper,
			IQueryFieldLookupRepository queryFieldLookupRepository,
			FieldMap[] mappedFields,
			HashSet<int> longTextField,
			int[] artifactViewFieldIds)
			: this(mappedFields, jobStopManager, helper)
		{
			Exporter = exporter;
			RelativityObjectManager = relativityObjectManager;
			ArtifactViewFieldIds = artifactViewFieldIds;
			ExportJobInfo = Exporter.InitializeExport(0, null, 0);
			LongTextFieldArtifactIds = longTextField;
			QueryFieldLookupRepository = queryFieldLookupRepository;
		}

		internal ExporterServiceBase(
			IExporter exporter,
			IRepositoryFactory sourceRepositoryFactory,
			IRepositoryFactory targetRepositoryFactory,
			IJobStopManager jobStopManager,
			IHelper helper,
			IBaseServiceContextProvider baseServiceContextProvider,
			FieldMap[] mappedFields,
			int startAt,
			string config,
			int searchArtifactId)
			: this(mappedFields, jobStopManager, helper)
		{
			SourceConfiguration = JsonConvert.DeserializeObject<SourceConfiguration>(config);
			BaseContext = baseServiceContextProvider.GetUnversionContext(SourceConfiguration.SourceWorkspaceArtifactId);

			ValidateDestinationFields(targetRepositoryFactory, mappedFields);

			QueryFieldLookupRepository = sourceRepositoryFactory.GetQueryFieldLookupRepository(SourceConfiguration.SourceWorkspaceArtifactId);

			Dictionary<int, int> fieldsReferences = InitializeSourceFields(sourceRepositoryFactory, mappedFields);
			ArtifactViewFieldIds = FieldArtifactIds.Select(artifactId => fieldsReferences[artifactId]).ToArray(); // need to make sure that this is in order

			Exporter = exporter;
			try
			{
				ExportJobInfo = Exporter.InitializeExport(searchArtifactId, ArtifactViewFieldIds, startAt);
				Logger.LogInformation("Retrieved ExportJobInfo in ExporterServiceBase. Run: {runId}, rows: {rowCount}", ExportJobInfo?.RunId, ExportJobInfo?.RowCount);
			}
			catch (Exception exception)
			{
				LogCreatingExporterServiceError(exception);
				// NOTE: If we get an exception, we cannot be exactly sure what the real error is,
				// however, it is more than likely that you do not have Export or Saved Search permissions.
				throw new IntegrationPointsException(Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_EXPORT, exception);
			}

			Logger.LogInformation("Creating LongTextStreamFactory. DataGridContext == null : {isDgContextNull}", DataGridContext == null);
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

		private Dictionary<int, int> InitializeSourceFields(IRepositoryFactory sourceRepositoryFactory, FieldMap[] mappedFields)
		{
			var nestedAndMultiValuesValidator = new NestedAndMultiValuesFieldValidator(Logger);
			var fieldsReferences = new Dictionary<int, int>();
			foreach (FieldEntry source in mappedFields.Select(f => f.SourceField))
			{
				int artifactId = Convert.ToInt32(source.FieldIdentifier);
				ViewFieldInfo fieldInfo = QueryFieldLookupRepository.GetFieldByArtifactId(artifactId);

				fieldsReferences[artifactId] = fieldInfo.AvfId;
				switch (fieldInfo.FieldType)
				{
					case FieldTypeHelper.FieldType.Objects:
						MultipleObjectFieldArtifactIds.Add(artifactId);
						IFieldQueryRepository fieldQueryRepository = sourceRepositoryFactory.GetFieldQueryRepository(SourceConfiguration.SourceWorkspaceArtifactId);
						ArtifactDTO identifierField = fieldQueryRepository.RetrieveTheIdentifierField(fieldInfo.AssociativeArtifactTypeID);
						string identifierFieldName = (string)identifierField.Fields.First(field => field.Name == "Name").Value;
						IObjectRepository objectRepository = sourceRepositoryFactory.GetObjectRepository(SourceConfiguration.SourceWorkspaceArtifactId, fieldInfo.AssociativeArtifactTypeID);
						ArtifactDTO[] objects = objectRepository.GetFieldsFromObjects(new[] { identifierFieldName }).GetResultsWithoutContextSync();
						nestedAndMultiValuesValidator.VerifyObjectField(fieldInfo.DisplayName, objects);
						break;

					case FieldTypeHelper.FieldType.Code:
						SingleChoiceFieldsArtifactIds.Add(artifactId);
						break;

					case FieldTypeHelper.FieldType.Text:
						LongTextFieldArtifactIds.Add(artifactId);
						break;

					case FieldTypeHelper.FieldType.MultiCode:
						ICodeRepository codeRepository = sourceRepositoryFactory.GetCodeRepository(SourceConfiguration.SourceWorkspaceArtifactId);
						ArtifactDTO[] codes = codeRepository.RetrieveCodeAsync(fieldInfo.DisplayName).GetResultsWithoutContextSync();
						nestedAndMultiValuesValidator.VerifyChoiceField(fieldInfo.DisplayName, codes);
						break;
				}

				if (fieldInfo.EnableDataGrid && (DataGridContext == null))
				{
					Logger.LogInformation("DataGridContext was null - creating new instance.");
					DataGridContext = new DataGridContext(BaseContext, true);
				}
			}

			return fieldsReferences;
		}

		public virtual bool HasDataToRetrieve => (TotalRecordsFound > RetrievedDataCount) && !JobStopManager.IsStopRequested();

		public virtual int TotalRecordsFound => (int)ExportJobInfo.RowCount;

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

			if (DataGridContext != null)
			{
				// dispose and cleanup won't do
				DataGridContext.BaseDataGridContext.BufferPool.BufferPoolBaseCollection.Clear();
				DataGridContext.BaseDataGridContext.Cleanup();
				DataGridContext.BaseDataGridContext.Dispose();
				DataGridContext = null;
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