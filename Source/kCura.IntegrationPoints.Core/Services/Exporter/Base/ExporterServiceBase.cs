using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime;
using System.Security.Claims;
using System.Text.RegularExpressions;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
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
using ArtifactType = kCura.Relativity.Client.ArtifactType;
using QueryFieldLookup = Relativity.Core.QueryFieldLookup;
using UserPermissionsMatrix = Relativity.Core.UserPermissionsMatrix;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public abstract class ExporterServiceBase : IExporterService
	{
		protected readonly int[] _avfIds;
		protected readonly BaseServiceContext _baseContext;
		protected readonly IExporter _exporter;
		protected readonly Export.InitializationResults _exportJobInfo;
		protected readonly int[] _fieldArtifactIds;
		protected readonly IJobStopManager _jobStopManager;
		protected readonly HashSet<int> _longTextFieldArtifactIds;
		protected readonly IILongTextStreamFactory _longTextStreamFactory;
		protected readonly FieldMap[] _mappedFields;
		protected readonly HashSet<int> _multipleObjectFieldArtifactIds;
		protected readonly HashSet<int> _singleChoiceFieldsArtifactIds;
		protected readonly IAPILog _logger;
		protected DataGridContext _dataGridContext;
		protected IDataTransferContext _context;
		protected SourceConfiguration _sourceConfiguration;
		protected int _retrievedDataCount;
		protected IQueryFieldLookupRepository QueryFieldLookupRepository;


		/// <summary>
		///     Testing only
		/// </summary>
		protected ExporterServiceBase(
			IExporter exporter,
			IILongTextStreamFactory longTextStreamFactory,
			IJobStopManager jobStopManager,
			IHelper helper,
			FieldMap[] mappedFields,
			HashSet<int> longTextField,
			int[] avfIds)
			: this(mappedFields, jobStopManager, helper)
		{
			_exporter = exporter;
			_longTextStreamFactory = longTextStreamFactory;
			_avfIds = avfIds;
			_exportJobInfo = _exporter.InitializeExport(0, null, 0);
			_longTextFieldArtifactIds = longTextField;
		}

		protected ExporterServiceBase(
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
			_sourceConfiguration = JsonConvert.DeserializeObject<SourceConfiguration>(config);
			_baseContext = claimsPrincipal.GetUnversionContext(_sourceConfiguration.SourceWorkspaceArtifactId);

			IFieldQueryRepository targetFieldQueryRepository = targetRepositoryFactory.GetFieldQueryRepository(_sourceConfiguration.TargetWorkspaceArtifactId);
			ValidateDestinationFields(targetFieldQueryRepository, _sourceConfiguration.TargetWorkspaceArtifactId, mappedFields);
            
            QueryFieldLookupRepository = sourceRepositoryFactory.GetQueryFieldLookupRepository(_sourceConfiguration.SourceWorkspaceArtifactId);
            
            Dictionary<int, int> fieldsReferences = new Dictionary<int, int>();
			foreach (FieldEntry source in mappedFields.Select(f => f.SourceField))
			{
				int artifactId = Convert.ToInt32(source.FieldIdentifier);
				ViewFieldInfo fieldInfo = QueryFieldLookupRepository.GetFieldByArtifactId(artifactId);

				fieldsReferences[artifactId] = fieldInfo.AvfId;
				switch (fieldInfo.FieldType)
				{
					case FieldTypeHelper.FieldType.Objects:
						_multipleObjectFieldArtifactIds.Add(artifactId);
						IFieldQueryRepository fieldQueryRepository = sourceRepositoryFactory.GetFieldQueryRepository(_sourceConfiguration.SourceWorkspaceArtifactId);
						ArtifactDTO identifierField = fieldQueryRepository.RetrieveTheIdentifierField(fieldInfo.AssociativeArtifactTypeID);
						string identifierFieldName = (string)identifierField.Fields.First(field => field.Name == "Name").Value;
						IObjectRepository objectRepository = sourceRepositoryFactory.GetObjectRepository(_sourceConfiguration.SourceWorkspaceArtifactId, fieldInfo.AssociativeArtifactTypeID);
						ArtifactDTO[] objects = objectRepository.GetFieldsFromObjects(new[] { identifierFieldName }).GetResultsWithoutContextSync();
						VerifyValidityOfTheNestedOrMultiValuesField(fieldInfo.DisplayName, objects, Constants.IntegrationPoints.InvalidMultiObjectsValueFormat);
						break;

					case FieldTypeHelper.FieldType.Code:
						_singleChoiceFieldsArtifactIds.Add(artifactId);
						break;

					case FieldTypeHelper.FieldType.Text:
						_longTextFieldArtifactIds.Add(artifactId);
						break;

					case FieldTypeHelper.FieldType.MultiCode:
						ICodeRepository codeRepository = sourceRepositoryFactory.GetCodeRepository(_sourceConfiguration.SourceWorkspaceArtifactId);
						ArtifactDTO[] codes = codeRepository.RetrieveCodeAsync(fieldInfo.DisplayName).GetResultsWithoutContextSync();
						VerifyValidityOfTheNestedOrMultiValuesField(fieldInfo.DisplayName, codes, Constants.IntegrationPoints.InvalidMultiChoicesValueFormat);
						break;
				}

				if (fieldInfo.EnableDataGrid && (_dataGridContext == null))
				{
					_dataGridContext = new DataGridContext(_baseContext, true);	
				}
			}

			_avfIds = _fieldArtifactIds.Select(artifactId => fieldsReferences[artifactId]).ToArray(); // need to make sure that this is in order
			
			_exporter = exporter;

			try
			{
				_exportJobInfo = _exporter.InitializeExport(searchArtifactId, _avfIds, startAt);
			}
			catch (Exception exception)
			{
				LogCreatingError(exception);
				// NOTE: If we get an exception, we cannot be exactly sure what the real error is,
				// however, it is more than likely that you do not have Export or Saved Search permissions.
				throw new Exception(Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_EXPORT, exception);
			}

			_longTextStreamFactory = new ExportApiDataHelper.RelativityLongTextStreamFactory(_baseContext, _dataGridContext, _sourceConfiguration.SourceWorkspaceArtifactId);
		}

		protected ExporterServiceBase(FieldMap[] mappedFields, IJobStopManager jobStopManager, IHelper helper)
		{
			_singleChoiceFieldsArtifactIds = new HashSet<int>();
			_multipleObjectFieldArtifactIds = new HashSet<int>();
			_longTextFieldArtifactIds = new HashSet<int>();
			_retrievedDataCount = 0;
			_mappedFields = mappedFields;
			_jobStopManager = jobStopManager;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RelativityExporterService>();
			_fieldArtifactIds = mappedFields.Select(field => int.Parse(field.SourceField.FieldIdentifier)).ToArray();
		}

		public virtual bool HasDataToRetrieve => (TotalRecordsFound > _retrievedDataCount) && !_jobStopManager.IsStopRequested();

		public virtual int TotalRecordsFound => (int)_exportJobInfo.RowCount;

		public abstract IDataTransferContext GetDataTransferContext(IExporterTransferConfiguration transferConfiguration);

		public abstract ArtifactDTO[] RetrieveData(int size);

		public void Dispose()
		{
			if (_context.DataReader != null)
			{
				_context.DataReader.Dispose();
				_context.DataReader = null;
				_context = null;
			}

			if (_dataGridContext != null)
			{
				// dispose and cleanup won't do
				_dataGridContext.BaseDataGridContext.BufferPool.BufferPoolBaseCollection.Clear();
				_dataGridContext.BaseDataGridContext.Cleanup();
				_dataGridContext.BaseDataGridContext.Dispose();
				_dataGridContext = null;
			}

			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
			GC.Collect();
		}

		protected void SetupBaseFields(int documentArtifactId, object[] fieldsValue, List<ArtifactFieldDTO> fields)
		{
			for (int index = 0; index < _avfIds.Length; index++)
			{
				int artifactId = _fieldArtifactIds[index];
				object value = fieldsValue[index];

				Exception exception = null;
				try
				{
					if (_multipleObjectFieldArtifactIds.Contains(artifactId))
					{
						value = ExportApiDataHelper.SanitizeMultiObjectField(value);
					}
					else if (_singleChoiceFieldsArtifactIds.Contains(artifactId))
					{
						value = ExportApiDataHelper.SanitizeSingleChoiceField(value);
					}
				}
				catch (Exception ex)
				{
					LogRetrievingDataError(ex);
					exception = ex;
				}

				fields.Add(new LazyExceptArtifactFieldDto(exception)
				{
					Name = _exportJobInfo.ColumnNames[index],
					ArtifactId = artifactId,
					Value = value,
					FieldType = QueryFieldLookupRepository.GetFieldByArtifactId(artifactId).FieldType.ToString()
				});
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
			_logger.LogError(exception, "Failed to create RelativityExporterService.");
		}

		protected virtual void LogRetrievingDataError(Exception ex)
		{
			_logger.LogError(ex, "Error occurred during data retrieval.");
		}

		private void LogFieldValidationError(Exception e)
		{
			_logger.LogError(e, "Error occurred during destination field validation.");
		}

		protected virtual void LogInvalidFieldMappingError()
		{
			_logger.LogError("Job failed. Fields mapped may no longer be available or have been renamed. Please validate your field mapping settings.");
		}

		protected virtual void LogFieldValuesValidationError(string message)
		{
			_logger.LogError(message);
		}

		#endregion
	}
}