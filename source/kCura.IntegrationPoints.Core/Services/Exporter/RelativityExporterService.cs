using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Queries;
using Newtonsoft.Json;
using Relativity.Core;
using Relativity.Core.Authentication;
using Relativity.Data.QueryBuilders.Dynamic;
using ArtifactType = kCura.Relativity.Client.ArtifactType;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class RelativityExporterService : IExporterService
	{
		private readonly global::Relativity.Core.Api.Shared.Manager.Export.Exporter _exporter;
		private readonly Export.InitializationResults _exportJobInfo;
		private readonly int _retrievedDataCount;

		private readonly int[] _fieldArtifactIds;
		private readonly int[] _avfIds;

		public RelativityExporterService(
			FieldMap[] mappedFields,
			int startAt,
			string config)
		{
			ExportUsingSavedSearchSettings settings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(config);

			BaseServiceContext context = ClaimsPrincipal.Current.GetNewServiceContext(settings.SourceWorkspaceArtifactId);
			_exporter = new global::Relativity.Core.Api.Shared.Manager.Export.Exporter
			{
				CurrentServiceContext = context,
				DynamicallyLoadedDllPaths = global::Relativity.Core.Api.Settings.RSAPI.Config.DynamicallyLoadedDllPaths
			};

			_fieldArtifactIds = mappedFields.Select(field => Int32.Parse(field.SourceField.FieldIdentifier)).ToArray();

			QueryFieldLookup fieldLookupHelper = new QueryFieldLookup(context, (int) ArtifactType.Document);
			Dictionary<int,int> fieldsReferences = new Dictionary<int, int>();
			foreach (FieldEntry source in mappedFields.Select(f => f.SourceField))
			{
				int artifactId = Convert.ToInt32(source.FieldIdentifier);
				fieldsReferences[artifactId] = fieldLookupHelper.GetFieldByArtifactID(artifactId).AvfId;
			}
			_avfIds = _fieldArtifactIds.Select(artifactId => fieldsReferences[artifactId]).ToArray(); // need to make sure that this is in order


			_exportJobInfo = _exporter.InitializeSearchExport(settings.SavedSearchArtifactId, _avfIds, startAt);
			_retrievedDataCount = 0;
		}

		public ArtifactDTO[] RetrieveData(int size)
		{
			List<ArtifactDTO> result = new List<ArtifactDTO>(size);
			object[] retrievedData = _exporter.RetrieveResults(_exportJobInfo.RunId, _avfIds, size);
			int artifactType = (int)ArtifactType.Document;
			foreach (var data in retrievedData)
			{
				ArtifactFieldDTO[] fields = new ArtifactFieldDTO[_avfIds.Length];
				object[] fieldsValue = (object[])data;
				for (int index = 0; index < _avfIds.Length; index++)
				{
					fields[index] = new ArtifactFieldDTO()
					{
						Name = _exportJobInfo.ColumnNames[index],
						ArtifactId = _fieldArtifactIds[index],
						Value = fieldsValue[index]
					};
				}
				result.Add(new ArtifactDTO(Convert.ToInt32(fieldsValue[_avfIds.Length]), artifactType, fields));
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