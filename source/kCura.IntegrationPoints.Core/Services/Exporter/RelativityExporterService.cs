using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Xml.Serialization;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Queries;
using Newtonsoft.Json;
using Relativity;
using Relativity.Core;
using Relativity.Core.Authentication;
using ArtifactType = kCura.Relativity.Client.ArtifactType;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class RelativityExporterService : IExporterService
	{
		private readonly global::Relativity.Core.Api.Shared.Manager.Export.Exporter _exporter;
		private readonly Export.InitializationResults _exportJobInfo;
		private readonly int _retrievedDataCount;

		private readonly HashSet<int> _singleChioceFieldsArtifactIds;
		private readonly HashSet<int> _multipleObjectFieldArtifactIds;
		 
		private readonly FieldMap[] _mappedFields;
		private readonly int[] _fieldArtifactIds;
		private readonly int[] _avfIds;
		private readonly DirectSqlCallHelper _helper;
		private IDataReader _reader;

		public RelativityExporterService(
			FieldMap[] mappedFields,
			int startAt,
			string config,
			DirectSqlCallHelper helper)
		{
			_singleChioceFieldsArtifactIds = new HashSet<int>();
			_multipleObjectFieldArtifactIds = new HashSet<int>();

			ExportUsingSavedSearchSettings settings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(config);
			BaseServiceContext context = ClaimsPrincipal.Current.GetNewServiceContext(settings.SourceWorkspaceArtifactId);
			_exporter = new global::Relativity.Core.Api.Shared.Manager.Export.Exporter
			{
				CurrentServiceContext = context,
				DynamicallyLoadedDllPaths = global::Relativity.Core.Api.Settings.RSAPI.Config.DynamicallyLoadedDllPaths,
				UserAclMatrix = new UserPermissionsMatrix(context),
				MultiValueDeimiter = IntegrationPoints.Contracts.Constants.MULTI_VALUE_DEIMITER,
				NestedValueDelimiter = IntegrationPoints.Contracts.Constants.NESTED_VALUE_DELIMITER
			};

			_mappedFields = mappedFields;
			_fieldArtifactIds = mappedFields.Select(field => Int32.Parse(field.SourceField.FieldIdentifier)).ToArray();

			QueryFieldLookup fieldLookupHelper = new QueryFieldLookup(context, (int)ArtifactType.Document);
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
					_singleChioceFieldsArtifactIds.Add(artifactId);
				}
			}

			_avfIds = _fieldArtifactIds.Select(artifactId => fieldsReferences[artifactId]).ToArray(); // need to make sure that this is in order

			_exportJobInfo = _exporter.InitializeSearchExport(settings.SavedSearchArtifactId, _avfIds, startAt);
			_retrievedDataCount = 0;
			_helper = helper;
		}

		public IDataReader GetDataReader()
		{
			if (_reader == null)
			{
				IEnumerable<FieldEntry> sources = _mappedFields.Select(map => map.SourceField);
				_reader = new DocumentTransferDataReader(this, sources, _helper);
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
				foreach (var data in retrievedData)
				{
					ArtifactFieldDTO[] fields = new ArtifactFieldDTO[_avfIds.Length];
					object[] fieldsValue = (object[])data;
					for (int index = 0; index < _avfIds.Length; index++)
					{
						int artifactId = _fieldArtifactIds[index];
						object value = fieldsValue[index];

						if (_multipleObjectFieldArtifactIds.Contains(artifactId))
						{
							value = ExportApiDataHelper.SanitizeMultiObjectField(value);
						}
						else if (_singleChioceFieldsArtifactIds.Contains(artifactId))
						{
							value = ExportApiDataHelper.SanitizeSingleChoiceField(value);
						}

						fields[index] = new ArtifactFieldDTO()
						{
							Name = _exportJobInfo.ColumnNames[index],
							ArtifactId = artifactId,
							Value = value
						};
					}
					result.Add(new ArtifactDTO(Convert.ToInt32(fieldsValue[_avfIds.Length]), artifactType, fields));
				}
			}
			return result.ToArray();
		}

		public bool HasDataToRetrieve
		{
			get
			{
				return TotalRecordsToImport > _retrievedDataCount;
			}
		}

		public int TotalRecordsToImport
		{
			get
			{
				return TotalRecordsFound;
			}
		}

		public int TotalRecordsFound
		{
			get
			{
				return (int)_exportJobInfo.RowCount;
			}
		}
	}
}