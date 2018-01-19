using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Security.Claims;
using System.Text.RegularExpressions;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Toggles;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using Newtonsoft.Json;
using Relativity;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Api.Shared.Manager.Export;
using Relativity.Data;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Services.Exporter
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
		protected readonly IILongTextStreamFactory LongTextStreamFactory;
		protected readonly IJobStopManager JobStopManager;
		protected readonly int[] AvfIds;
		protected readonly int[] FieldArtifactIds;


		/// <summary>
		///     Testing only
		/// </summary>
		protected ExporterServiceBase(
			IExporter exporter,
			IILongTextStreamFactory longTextStreamFactory,
			IJobStopManager jobStopManager,
			IHelper helper,
			IQueryFieldLookupRepository queryFieldLookupRepository,
			FieldMap[] mappedFields,
			HashSet<int> longTextField,
			int[] avfIds)
			: this(mappedFields, jobStopManager, helper)
		{
			Exporter = exporter;
			LongTextStreamFactory = longTextStreamFactory;
			AvfIds = avfIds;
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
			ClaimsPrincipal claimsPrincipal,
			FieldMap[] mappedFields,
			int startAt,
			string config,
			int searchArtifactId)
			: this(mappedFields, jobStopManager, helper)
		{
			SourceConfiguration = JsonConvert.DeserializeObject<SourceConfiguration>(config);
			BaseContext = claimsPrincipal.GetUnversionContext(SourceConfiguration.SourceWorkspaceArtifactId);

			IFieldQueryRepository targetFieldQueryRepository = targetRepositoryFactory.GetFieldQueryRepository(SourceConfiguration.TargetWorkspaceArtifactId);
			ValidateDestinationFields(targetFieldQueryRepository, SourceConfiguration.TargetWorkspaceArtifactId, mappedFields);
            
            QueryFieldLookupRepository = sourceRepositoryFactory.GetQueryFieldLookupRepository(SourceConfiguration.SourceWorkspaceArtifactId);
            
            Dictionary<int, int> fieldsReferences = new Dictionary<int, int>();
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
						VerifyValidityOfTheNestedOrMultiValuesField(fieldInfo.DisplayName, objects, Constants.IntegrationPoints.InvalidMultiObjectsValueFormat);
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
						VerifyValidityOfTheNestedOrMultiValuesField(fieldInfo.DisplayName, codes, Constants.IntegrationPoints.InvalidMultiChoicesValueFormat);
						break;
				}

				if (fieldInfo.EnableDataGrid && (DataGridContext == null))
				{
					DataGridContext = new DataGridContext(BaseContext, true);	
				}
			}

			AvfIds = FieldArtifactIds.Select(artifactId => fieldsReferences[artifactId]).ToArray(); // need to make sure that this is in order
			
			Exporter = exporter;

			try
			{
				ExportJobInfo = Exporter.InitializeExport(searchArtifactId, AvfIds, startAt);
			}
			catch (Exception exception)
			{
				LogCreatingError(exception);
				// NOTE: If we get an exception, we cannot be exactly sure what the real error is,
				// however, it is more than likely that you do not have Export or Saved Search permissions.
				throw new Exception(Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_EXPORT, exception);
			}

			LongTextStreamFactory = new ExportApiDataHelper.RelativityLongTextStreamFactory(BaseContext, DataGridContext, SourceConfiguration.SourceWorkspaceArtifactId);
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

		protected virtual void ValidateDestinationFields(IFieldQueryRepository fieldQueryRepository, int destinationWorkspace, FieldMap[] mappedFields)
		{
			IList<string> missingFields = new List<string>();
			//BaseServiceContext destinationWorkspaceContext = claimsPrincipal.GetUnversionContext(destinationWorkspace);
			//IQueryFieldLookup fieldLookupHelper = new QueryFieldLookup(destinationWorkspaceContext, (int) ArtifactType.Document);

			IDictionary<int, string> targetFields = fieldQueryRepository
				.RetrieveFieldsAsync(
					(int)Relativity.Client.ArtifactType.Document,
					new HashSet<string>(new string[0]))
				.GetResultsWithoutContextSync()
				.ToDictionary(k => k.ArtifactId, v => v.TextIdentifier);

			for (int index = 0; index < mappedFields.Length; index++)
			{
				FieldEntry fieldEntry = mappedFields[index].DestinationField;
				if ((fieldEntry != null) && !string.IsNullOrEmpty(fieldEntry.FieldIdentifier))
				{
					int artifactId = 0;
					if (int.TryParse(fieldEntry.FieldIdentifier, out artifactId))
					{
						try
						{
							string fieldName;
							if (!targetFields.TryGetValue(artifactId, out fieldName)
							    || String.Equals(fieldEntry.ActualName, fieldName, StringComparison.OrdinalIgnoreCase))
							{
								missingFields.Add(fieldEntry.ActualName);
							}

							/*
							ViewFieldInfo fieldInfo = fieldLookupHelper.GetFieldByArtifactID(artifactId);
							if ((fieldInfo == null) || (string.Equals(fieldInfo.DisplayName, fieldEntry.ActualName, StringComparison.OrdinalIgnoreCase) == false))
							{
								missingFields.Add(fieldEntry.ActualName);
							}
							*/
						}
						catch (Exception e)
						{
							LogFieldValidationError(e);
							missingFields.Add(fieldEntry.ActualName);
						}
					}
				}
			}

			if (missingFields.Count > 0)
			{
				LogInvalidFieldMappingError();
				// TODO : We may want to just update the field's name instead. Sorawit - 6/24/2016.
				throw new Exception("Job failed. Fields mapped may no longer be available or have been renamed. Please validate your field mapping settings.");
			}
		}

		protected virtual void VerifyValidityOfTheNestedOrMultiValuesField(string fieldName, ArtifactDTO[] dtos, Regex invalidPattern)
		{
			List<Exception> exceptions = new List<Exception>(dtos.Length);
			for (int index = 0; index < dtos.Length; index++)
			{
				ArtifactDTO dto = dtos[index];
				string name = (string)dto.Fields[0].Value;
				if (invalidPattern.IsMatch(name))
				{
					Exception exception = new Exception($"Invalid '{fieldName}' : {name}");
					exceptions.Add(exception);
				}
			}

			if (exceptions.Count > 0)
			{
				string message = $"Invalid '{fieldName}' found." +
				                 $" Please remove invalid character(s) - {IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER} or {IntegrationPoints.Domain.Constants.NESTED_VALUE_DELIMITER}, before proceeding further.";
				LogFieldValuesValidationError(message);
				AggregateException exception = new AggregateException(message, exceptions);
				throw exception;
			}
		}

		#region Logging

		protected virtual void LogCreatingError(Exception exception)
		{
			Logger.LogError(exception, "Failed to create RelativityExporterService.");
		}

		protected virtual void LogRetrievingDataError(Exception ex)
		{
			Logger.LogError(ex, "Error occurred during data retrieval.");
		}

		private void LogFieldValidationError(Exception e)
		{
			Logger.LogError(e, "Error occurred during destination field validation.");
		}

		protected virtual void LogInvalidFieldMappingError()
		{
			Logger.LogError("Job failed. Fields mapped may no longer be available or have been renamed. Please validate your field mapping settings.");
		}

		protected virtual void LogFieldValuesValidationError(string message)
		{
			Logger.LogError(message);
		}

		#endregion
	}
}