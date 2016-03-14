using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Queries;
using Newtonsoft.Json;
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

		private readonly int[] _fieldArtifactIds;
		private readonly int[] _avfIds;

		public RelativityExporterService(
			DirectSqlCallHelper dataHelper,
			FieldMap[] mappedFields,
			int startAt,
			string config)
		{
			//TODO : wait for chris hogan to response on how to handle this properly. - Amornborvornwong - 3/14/2016
			ClaimsPrincipal claimPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity>() { new ClaimsIdentity(new List<Claim>() { new Claim("rel_uai", "9") }) });
			ExportUsingSavedSearchSettings settings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(config);
			_exporter = new global::Relativity.Core.Api.Shared.Manager.Export.Exporter
			{
				CurrentServiceContext = claimPrincipal.GetNewServiceContext(settings.SourceWorkspaceArtifactId),
				DynamicallyLoadedDllPaths = global::Relativity.Core.Config.DynamicallyLoadedDllPaths
			};

			_fieldArtifactIds = mappedFields.Select(field => Int32.Parse(field.SourceField.FieldIdentifier)).ToArray();
			Dictionary<int, int> fieldsReferences = dataHelper.GetArtifactViewFieldId(_fieldArtifactIds);
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