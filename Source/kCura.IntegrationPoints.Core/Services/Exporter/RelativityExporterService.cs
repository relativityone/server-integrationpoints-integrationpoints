using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime;
using System.Security.Claims;
using System.Text.RegularExpressions;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
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
	public class RelativityExporterService : IExporterService
	{
		private readonly int[] _avfIds;
		private readonly BaseServiceContext _baseContext;
		private readonly IExporter _exporter;
		private readonly Export.InitializationResults _exportJobInfo;
		private readonly int[] _fieldArtifactIds;
		private readonly IJobStopManager _jobStopManager;
		private readonly HashSet<int> _longTextFieldArtifactIds;
		private readonly IILongTextStreamFactory _longTextStreamFactory;
		private readonly FieldMap[] _mappedFields;
		private readonly HashSet<int> _multipleObjectFieldArtifactIds;
		private readonly HashSet<int> _singleChoiceFieldsArtifactIds;
		private readonly IAPILog _logger;
		private DataGridContext _dataGridContext;
		private IDataReader _reader;
		private int _retrievedDataCount;

		/// <summary>
		///     Testing only
		/// </summary>
		internal RelativityExporterService(
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

		public RelativityExporterService(
			IRepositoryFactory repositoryFactory,
			IJobStopManager jobStopManager,
			IHelper helper,
			ClaimsPrincipal claimsPrincipal,
			FieldMap[] mappedFields,
			int startAt,
			string config,
			int savedSearchArtifactId)
			: this(mappedFields, jobStopManager, helper)
		{
			var settings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(config);
			_baseContext = claimsPrincipal.GetUnversionContext(settings.SourceWorkspaceArtifactId);

			ValidateDestinationFields(claimsPrincipal, settings.TargetWorkspaceArtifactId, mappedFields);

			IQueryFieldLookup fieldLookupHelper = new QueryFieldLookup(_baseContext, (int) ArtifactType.Document);

			Dictionary<int, int> fieldsReferences = new Dictionary<int, int>();
			foreach (FieldEntry source in mappedFields.Select(f => f.SourceField))
			{
				int artifactId = Convert.ToInt32(source.FieldIdentifier);
				ViewFieldInfo fieldInfo = fieldLookupHelper.GetFieldByArtifactID(artifactId);

				fieldsReferences[artifactId] = fieldInfo.AvfId;
				switch (fieldInfo.FieldType)
				{
					case FieldTypeHelper.FieldType.Objects:
						_multipleObjectFieldArtifactIds.Add(artifactId);
						IFieldRepository fieldRepository = repositoryFactory.GetFieldRepository(settings.SourceWorkspaceArtifactId);
						ArtifactDTO identifierField = fieldRepository.RetrieveTheIdentifierField(fieldInfo.AssociativeArtifactTypeID);
						string identifierFieldName = (string) identifierField.Fields.First(field => field.Name == "Name").Value;
						IObjectRepository objectRepository = repositoryFactory.GetObjectRepository(settings.SourceWorkspaceArtifactId, fieldInfo.AssociativeArtifactTypeID);
						ArtifactDTO[] objects = objectRepository.GetFieldsFromObjects(new[] {identifierFieldName}).GetResultsWithoutContextSync();
						VerifyValidityOfTheNestedOrMultiValuesField(fieldInfo.DisplayName, objects, Constants.IntegrationPoints.InvalidMultiObjectsValueFormat);
						break;

					case FieldTypeHelper.FieldType.Code:
						_singleChoiceFieldsArtifactIds.Add(artifactId);
						break;

					case FieldTypeHelper.FieldType.Text:
						_longTextFieldArtifactIds.Add(artifactId);
						break;

					case FieldTypeHelper.FieldType.MultiCode:
						ICodeRepository codeRepository = repositoryFactory.GetCodeRepository(settings.SourceWorkspaceArtifactId);
						ArtifactDTO[] codes = codeRepository.RetrieveCodeAsync(fieldInfo.DisplayName).GetResultsWithoutContextSync();
						VerifyValidityOfTheNestedOrMultiValuesField(fieldInfo.DisplayName, codes, Constants.IntegrationPoints.InvalidMultiChoicesValueFormat);
						break;
				}

				if (fieldInfo.EnableDataGrid && (_dataGridContext == null))
				{
					_dataGridContext = new DataGridContext(true);
				}
			}

			_avfIds = _fieldArtifactIds.Select(artifactId => fieldsReferences[artifactId]).ToArray(); // need to make sure that this is in order

			_exporter = new SavedSearchExporter
			(
				_baseContext,
				new UserPermissionsMatrix(_baseContext),
				global::Relativity.ArtifactType.Document,
				IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER,
				IntegrationPoints.Domain.Constants.NESTED_VALUE_DELIMITER,
				global::Relativity.Core.Api.Settings.RSAPI.Config.DynamicallyLoadedDllPaths
			);

			try
			{
				_exportJobInfo = _exporter.InitializeExport(savedSearchArtifactId, _avfIds, startAt);
			}
			catch (Exception exception)
			{
				LogCreatingError(exception);
				// NOTE: If we get an exception, we cannot be exactly sure what the real error is,
				// however, it is more than likely that you do not have Export or Saved Search permissions.
				throw new Exception(Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_EXPORT, exception);
			}

			_longTextStreamFactory = new ExportApiDataHelper.RelativityLongTextStreamFactory(_baseContext, _dataGridContext, settings.SourceWorkspaceArtifactId);
		}

		private RelativityExporterService(FieldMap[] mappedFields, IJobStopManager jobStopManager, IHelper helper)
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

		public bool HasDataToRetrieve
		{
			get { return (TotalRecordsFound > _retrievedDataCount) && !_jobStopManager.IsStopRequested(); }
		}

		public int TotalRecordsFound
		{
			get { return (int) _exportJobInfo.RowCount; }
		}

		public IDataReader GetDataReader(IScratchTableRepository[] scratchTableRepositories)
		{
			if (_reader == null)
			{
				_reader = new DocumentTransferDataReader(this, _mappedFields, _baseContext, scratchTableRepositories);
			}
			return _reader;
		}

		public ArtifactDTO[] RetrieveData(int size)
		{
			List<ArtifactDTO> result = new List<ArtifactDTO>(size);
			object[] retrievedData = _exporter.RetrieveResults(_exportJobInfo.RunId, _avfIds, size);
			if (retrievedData != null)
			{
				int artifactType = (int) ArtifactType.Document;
				foreach (object data in retrievedData)
				{
					ArtifactFieldDTO[] fields = new ArtifactFieldDTO[_avfIds.Length];

					object[] fieldsValue = (object[]) data;
					int documentArtifactId = Convert.ToInt32(fieldsValue[_avfIds.Length]);

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
							// export api will return a string constant represent the state of the string of which is too big. We will have to go and read this our self.
							else if (_longTextFieldArtifactIds.Contains(artifactId)
									&& global::Relativity.Constants.LONG_TEXT_EXCEEDS_MAX_LENGTH_FOR_LIST_TOKEN.Equals(value))
							{
								value = ExportApiDataHelper.RetrieveLongTextFieldAsync(_longTextStreamFactory, documentArtifactId, artifactId)
									.GetResultsWithoutContextSync();
							}
						}
						catch (Exception ex)
						{
							LogRetrievingDataError(ex);
							exception = ex;
						}

						fields[index] = new LazyExceptArtifactFieldDto(exception)
						{
							Name = _exportJobInfo.ColumnNames[index],
							ArtifactId = artifactId,
							Value = value
						};
					}

					// TODO: replace String.empty
					result.Add(new ArtifactDTO(documentArtifactId, artifactType, string.Empty, fields));
				}
			}
			_retrievedDataCount += result.Count;
			return result.ToArray();
		}

		public void Dispose()
		{
			if (_reader != null)
			{
				_reader.Dispose();
				_reader = null;
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

		private void ValidateDestinationFields(ClaimsPrincipal claimsPrincipal, int destinationWorkspace, FieldMap[] mappedFields)
		{
			IList<string> missingFields = new List<string>();
			BaseServiceContext destinationWorkspaceContext = claimsPrincipal.GetUnversionContext(destinationWorkspace);
			IQueryFieldLookup fieldLookupHelper = new QueryFieldLookup(destinationWorkspaceContext, (int) ArtifactType.Document);

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
							ViewFieldInfo fieldInfo = fieldLookupHelper.GetFieldByArtifactID(artifactId);
							if ((fieldInfo == null) || (string.Equals(fieldInfo.DisplayName, fieldEntry.ActualName, StringComparison.OrdinalIgnoreCase) == false))
							{
								missingFields.Add(fieldEntry.ActualName);
							}
						}
						catch(Exception e)
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

		private void VerifyValidityOfTheNestedOrMultiValuesField(string fieldName, ArtifactDTO[] dtos, Regex invalidPattern)
		{
			List<Exception> exceptions = new List<Exception>(dtos.Length);
			for (int index = 0; index < dtos.Length; index++)
			{
				ArtifactDTO dto = dtos[index];
				string name = (string) dto.Fields[0].Value;
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

		private void LogCreatingError(Exception exception)
		{
			_logger.LogError(exception, "Failed to create RelativityExporterService.");
		}

		private void LogRetrievingDataError(Exception ex)
		{
			_logger.LogError(ex, "Error occurred during data retrieval.");
		}

		private void LogFieldValidationError(Exception e)
		{
			_logger.LogError(e, "Error occurred during destination field validation.");
		}

		private void LogInvalidFieldMappingError()
		{
			_logger.LogError("Job failed. Fields mapped may no longer be available or have been renamed. Please validate your field mapping settings.");
		}

		private void LogFieldValuesValidationError(string message)
		{
			_logger.LogError(message);
		}

		#endregion
	}
}