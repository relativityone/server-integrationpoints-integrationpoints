using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using Newtonsoft.Json;
using Relativity;
using Relativity.Core;
using Relativity.Core.Authentication;
using Relativity.Data;
using ArtifactType = kCura.Relativity.Client.ArtifactType;
using QueryFieldLookup = Relativity.Core.QueryFieldLookup;
using UserPermissionsMatrix = Relativity.Core.UserPermissionsMatrix;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class RelativityExporterService : IExporterService
	{
		private const string _REQUEST_ORIGINATION = "RequestOrigination";
		private readonly int[] _avfIds;
		private readonly BaseServiceContext _baseContext;
		private readonly DataGridContext _dataGridContext;
		private readonly global::Relativity.Core.Api.Shared.Manager.Export.IExporter _exporter;
		private readonly Export.InitializationResults _exportJobInfo;
		private readonly int[] _fieldArtifactIds;
		private readonly HashSet<int> _longTextFieldArtifactIds;
		private readonly ISourceWorkspaceManager _sourceWorkspaceManager;
		private readonly FieldMap[] _mappedFields;
		private readonly HashSet<int> _multipleObjectFieldArtifactIds;
		private readonly int _retrievedDataCount;
		private readonly ExportUsingSavedSearchSettings _settings;
		private readonly HashSet<int> _singleChoiceFieldsArtifactIds;
		private IDataReader _reader;

		private RelativityExporterService()
		{
			_singleChoiceFieldsArtifactIds = new HashSet<int>();
			_multipleObjectFieldArtifactIds = new HashSet<int>();
			_longTextFieldArtifactIds = new HashSet<int>();
		}

		/// <summary>
		/// Testing only
		/// </summary>
		/// <param name="exporter"></param>
		public RelativityExporterService(
			global::Relativity.Core.Api.Shared.Manager.Export.IExporter exporter,
			int[] avfIds,
			int[] fieldArtifactIds)
			: this()
		{
			_exporter = exporter;
			_avfIds = avfIds;
			_exportJobInfo = _exporter.InitializeExport(0, null, 0);
			_fieldArtifactIds = fieldArtifactIds;
		}

		public RelativityExporterService(
			ISourceWorkspaceManager sourceWorkspaceManager,
			FieldMap[] mappedFields,
			int startAt,
			string config)
			: this()
		{
			_dataGridContext = new DataGridContext(true);
			_settings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(config);
			_sourceWorkspaceManager = sourceWorkspaceManager;
			_mappedFields = mappedFields;
			_fieldArtifactIds = mappedFields.Select(field => Int32.Parse(field.SourceField.FieldIdentifier)).ToArray();

			_baseContext = ClaimsPrincipal.Current.GetServiceContextUnversionShortTerm(_settings.SourceWorkspaceArtifactId);

			IQueryFieldLookup fieldLookupHelper = new QueryFieldLookup(_baseContext, (int)ArtifactType.Document);
			Dictionary<int, int> fieldsReferences = new Dictionary<int, int>();
			foreach (FieldEntry source in mappedFields.Select(f => f.SourceField))
			{
				int artifactId = Convert.ToInt32(source.FieldIdentifier);
				ViewFieldInfo fieldInfo = fieldLookupHelper.GetFieldByArtifactID(artifactId);

				fieldsReferences[artifactId] = fieldInfo.AvfId;
				if (fieldInfo.FieldType == FieldTypeHelper.FieldType.Objects)
				{
					_multipleObjectFieldArtifactIds.Add(artifactId);
				}
				else if (fieldInfo.FieldType == FieldTypeHelper.FieldType.Code)
				{
					_singleChoiceFieldsArtifactIds.Add(artifactId);
				}
				else if (fieldInfo.FieldType == FieldTypeHelper.FieldType.Text)
				{
					_longTextFieldArtifactIds.Add(artifactId);
				}
			}

			_avfIds = _fieldArtifactIds.Select(artifactId => fieldsReferences[artifactId]).ToArray(); // need to make sure that this is in order

			_exporter = new global::Relativity.Core.Api.Shared.Manager.Export.SavedSearchExporter
			(
					_baseContext,
					new UserPermissionsMatrix(_baseContext),
					global::Relativity.ArtifactType.Document,
					IntegrationPoints.Contracts.Constants.MULTI_VALUE_DELIMITER,
					IntegrationPoints.Contracts.Constants.NESTED_VALUE_DELIMITER,
					global::Relativity.Core.Api.Settings.RSAPI.Config.DynamicallyLoadedDllPaths
			);
			_exportJobInfo = _exporter.InitializeExport(_settings.SavedSearchArtifactId, _avfIds, startAt);
			_retrievedDataCount = 0;
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

		public IDataReader GetDataReader(ITempDocTableHelper docTableHelper)
		{
			if (_reader == null)
			{
				_reader = new DocumentTransferDataReader(_settings.SourceWorkspaceArtifactId, _settings.TargetWorkspaceArtifactId, this, _sourceWorkspaceManager, _mappedFields, _baseContext, docTableHelper);
			}
			return _reader;
		}

		public ArtifactDTO[] RetrieveData(int size)
		{
			List<ArtifactDTO> result = new List<ArtifactDTO>(size);
			object[] retrievedData = _exporter.RetrieveResults(_exportJobInfo.RunId, _avfIds, size);
			if (retrievedData != null)
			{
				int artifactType = (int)ArtifactType.Document;
				foreach (object data in retrievedData)
				{
					ArtifactFieldDTO[] fields = new ArtifactFieldDTO[_avfIds.Length];

					object[] fieldsValue = (object[])data;
					int documentArtifactId = Convert.ToInt32(fieldsValue[_avfIds.Length]);

					for (int index = 0; index < _avfIds.Length; index++)
					{
						int artifactId = _fieldArtifactIds[index];
						object value = fieldsValue[index];

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
							ExportApiDataHelper.RelativityLongTextStreamFactory factory = new ExportApiDataHelper.RelativityLongTextStreamFactory(_baseContext,
								_dataGridContext,
								documentArtifactId,
								_settings.SourceWorkspaceArtifactId,
								artifactId);
							value = ExportApiDataHelper.RetrieveLongTextFieldAsync(factory).ConfigureAwait(false).GetAwaiter().GetResult();
						}

						fields[index] = new ArtifactFieldDTO()
						{
							Name = _exportJobInfo.ColumnNames[index],
							ArtifactId = artifactId,
							Value = value
						};
					}
					result.Add(new ArtifactDTO(documentArtifactId, artifactType, fields));
				}
			}
			return result.ToArray();
		}
	}
}