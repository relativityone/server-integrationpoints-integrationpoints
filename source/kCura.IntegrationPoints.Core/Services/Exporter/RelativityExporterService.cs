using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Newtonsoft.Json;
using Relativity;
using Relativity.Core;
using Relativity.Data;
using System.Text.RegularExpressions;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class RelativityExporterService : IExporterService
	{
		private readonly int[] _avfIds;
		private readonly BaseServiceContext _baseContext;
		private readonly global::Relativity.Core.Api.Shared.Manager.Export.IExporter _exporter;
		private readonly IILongTextStreamFactory _longTextStreamFactory;
		private readonly Export.InitializationResults _exportJobInfo;
		private readonly int[] _fieldArtifactIds;
		private readonly HashSet<int> _longTextFieldArtifactIds;
		private readonly FieldMap[] _mappedFields;
		private readonly HashSet<int> _multipleObjectFieldArtifactIds;
		private readonly int _retrievedDataCount;
		private readonly HashSet<int> _singleChoiceFieldsArtifactIds;
		private IDataReader _reader;
		private DataGridContext _dataGridContext;

		/// <summary>
		/// Testing only
		/// </summary>
		internal RelativityExporterService(
			global::Relativity.Core.Api.Shared.Manager.Export.IExporter exporter,
			IILongTextStreamFactory longTextStreamFactory,
			FieldMap[] mappedFields,
			HashSet<int> longTextField,
			int[] avfIds)
			: this(mappedFields)
		{
			_exporter = exporter;
			_longTextStreamFactory = longTextStreamFactory;
			_avfIds = avfIds;
			_exportJobInfo = _exporter.InitializeExport(0, null, 0);
			_longTextFieldArtifactIds = longTextField;
		}

		public RelativityExporterService(
			IRepositoryFactory repositoryFactory,
			ClaimsPrincipal claimsPrincipal,
			FieldMap[] mappedFields,
			int startAt,
			string config)
			: this(mappedFields)
		{
			var settings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(config);
			_baseContext = claimsPrincipal.GetUnversionContext(settings.SourceWorkspaceArtifactId);

			IQueryFieldLookup fieldLookupHelper = new global::Relativity.Core.QueryFieldLookup(_baseContext, (int)Relativity.Client.ArtifactType.Document);

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
						string identifierFieldName = (string)identifierField.Fields.First(field => field.Name == "Name").Value;
						IObjectRepository objectRepository = repositoryFactory.GetObjectRepository(settings.SourceWorkspaceArtifactId, fieldInfo.AssociativeArtifactTypeID);
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
						ICodeRepository codeRepository = repositoryFactory.GetCodeRepository(settings.SourceWorkspaceArtifactId);
						ArtifactDTO[] codes = codeRepository.RetrieveCodeAsync(fieldInfo.DisplayName).GetResultsWithoutContextSync();
						VerifyValidityOfTheNestedOrMultiValuesField(fieldInfo.DisplayName, codes, Constants.IntegrationPoints.InvalidMultiChoicesValueFormat);
						break;
				}

				if (fieldInfo.EnableDataGrid && _dataGridContext == null)
				{
					_dataGridContext = new DataGridContext(true);
				}
			}

			_avfIds = _fieldArtifactIds.Select(artifactId => fieldsReferences[artifactId]).ToArray(); // need to make sure that this is in order

			_exporter = new global::Relativity.Core.Api.Shared.Manager.Export.SavedSearchExporter
			(
					_baseContext,
					new global::Relativity.Core.UserPermissionsMatrix(_baseContext),
					global::Relativity.ArtifactType.Document,
					IntegrationPoints.Contracts.Constants.MULTI_VALUE_DELIMITER,
					IntegrationPoints.Contracts.Constants.NESTED_VALUE_DELIMITER,
					global::Relativity.Core.Api.Settings.RSAPI.Config.DynamicallyLoadedDllPaths
			);

			try
			{
				_exportJobInfo = _exporter.InitializeExport(settings.SavedSearchArtifactId, _avfIds, startAt);
			}
			catch
			{
				throw new Exception(Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_EXPORT);				
			}

			_longTextStreamFactory = new ExportApiDataHelper.RelativityLongTextStreamFactory(_baseContext, _dataGridContext, settings.SourceWorkspaceArtifactId);
		}

		private void VerifyValidityOfTheNestedOrMultiValuesField(string fieldName, ArtifactDTO[] dtos, Regex invalidPattern)
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
								 $" Please remove invalid character(s) - {kCura.IntegrationPoints.Contracts.Constants.MULTI_VALUE_DELIMITER} or {kCura.IntegrationPoints.Contracts.Constants.NESTED_VALUE_DELIMITER}, before proceeding further.";
				AggregateException exception = new AggregateException(message, exceptions);
				throw exception;
			}
		}

		private RelativityExporterService(FieldMap[] mappedFields)
		{
			_singleChoiceFieldsArtifactIds = new HashSet<int>();
			_multipleObjectFieldArtifactIds = new HashSet<int>();
			_longTextFieldArtifactIds = new HashSet<int>();
			_retrievedDataCount = 0;
			_mappedFields = mappedFields;
			_fieldArtifactIds = mappedFields.Select(field => Int32.Parse(field.SourceField.FieldIdentifier)).ToArray();
		}

		public bool HasDataToRetrieve
		{
			get
			{
				return TotalRecordsFound > _retrievedDataCount;
			}
		}

		public int TotalRecordsFound
		{
			get
			{
				return (int)_exportJobInfo.RowCount;
			}
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
				int artifactType = (int)Relativity.Client.ArtifactType.Document;
				foreach (object data in retrievedData)
				{
					ArtifactFieldDTO[] fields = new ArtifactFieldDTO[_avfIds.Length];

					object[] fieldsValue = (object[])data;
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
					result.Add(new ArtifactDTO(documentArtifactId, artifactType, String.Empty, fields));
				}
			}
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
	}
}