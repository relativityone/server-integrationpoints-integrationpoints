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
using kCura.IntegrationPoints.Core.Services.Exporter.TransferContext;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using Newtonsoft.Json;
using Relativity;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.Core.Api.Shared.Manager.Export;
using Relativity.Data;
using Relativity.Toggles;
using ArtifactType = kCura.Relativity.Client.ArtifactType;
using FileQuery = Relativity.Core.Service.FileQuery;
using QueryFieldLookup = Relativity.Core.QueryFieldLookup;
using UserPermissionsMatrix = Relativity.Core.UserPermissionsMatrix;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class ImageExporterService : ExporterServiceBase
	{
		private const string ImageLocationColumn = "Location";

		public ImageExporterService(IRepositoryFactory sourceRepositoryFactory, IRepositoryFactory targetRepositoryFactory, IJobStopManager jobStopManager, IHelper helper, ClaimsPrincipal claimsPrincipal, FieldMap[] mappedFields, int startAt, string config, int savedSearchArtifactId) : base(sourceRepositoryFactory, targetRepositoryFactory, jobStopManager, helper, claimsPrincipal, mappedFields, startAt, config, savedSearchArtifactId)
		{
		}

		public ImageExporterService(FieldMap[] mappedFields, IJobStopManager jobStopManager, IHelper helper) : base(mappedFields, jobStopManager, helper)
		{
		}

		public override IDataTransferContext GetDataTransferContext(IExporterTransferConfiguration transferConfiguration)
		{
			var imageTransferDataReader = new ImageTransferDataReader(this, _mappedFields, _baseContext, transferConfiguration.ScratchRepositories);
			return _context ?? (_context = new ExporterTransferContext(imageTransferDataReader,transferConfiguration));
		}

		public override ArtifactDTO[] RetrieveData(int size)
		{
			List<ArtifactDTO> result = new List<ArtifactDTO>();
			object[] retrievedData = _exporter.RetrieveResults(_exportJobInfo.RunId, _avfIds, size);

			if (retrievedData != null)
			{
				int artifactType = (int)ArtifactType.Document;
				foreach (object data in retrievedData)
				{
					ArtifactFieldDTO[] fields = new ArtifactFieldDTO[_avfIds.Length];

					object[] fieldsValue = (object[])data;
					int documentArtifactId = Convert.ToInt32(fieldsValue[_avfIds.Length]);

					kCura.Data.DataView imagesDataView = FileQuery.RetrieveAllImagesForDocuments(_baseContext, new []{documentArtifactId});
					if (imagesDataView.Count > 0)
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
						for (int index = 0; index < imagesDataView.Table.Rows.Count; index++)
						{
							DataRow row = imagesDataView.Table.Rows[index];
							string fileLocation = (string)row[ImageLocationColumn];
							var fileLocationField = new ArtifactFieldDTO()
							{
								Name = IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME ,
								Value = fileLocation
							};
							var nativeFileNameField = new ArtifactFieldDTO()
							{
								Name = IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME,
								Value = fieldsValue[0]
							};

							var artifactFieldDtos = fields.ToList();
							artifactFieldDtos.Add(fileLocationField);
							artifactFieldDtos.Add(nativeFileNameField);

							result.Add(new ArtifactDTO(documentArtifactId, artifactType, string.Empty, artifactFieldDtos));
						}

						
					}
				}
			}

		
			_retrievedDataCount += result.Count;
			_context.TotalItemsFound = _retrievedDataCount;
			return result.ToArray();
		}

		//public override int TotalRecordsFound => _retrievedDataCount == 0 ? (int)_exportJobInfo.RowCount : _retrievedDataCount;//TODO: Item Count
	}
}