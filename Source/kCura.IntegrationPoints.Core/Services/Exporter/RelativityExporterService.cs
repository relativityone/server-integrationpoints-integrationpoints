﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter.TransferContext;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using Relativity.API;
using Relativity.Core.Api.Shared.Manager.Export;
using Relativity.Toggles;
using ArtifactType = kCura.Relativity.Client.ArtifactType;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class RelativityExporterService : ExporterServiceBase
	{
		public RelativityExporterService(IRepositoryFactory sourceRepositoryFactory, IRepositoryFactory targetRepositoryFactory, IJobStopManager jobStopManager, IHelper helper, ClaimsPrincipal claimsPrincipal, FieldMap[] mappedFields, int startAt, string config, int savedSearchArtifactId) : base(sourceRepositoryFactory, targetRepositoryFactory, jobStopManager, helper, claimsPrincipal, mappedFields, startAt, config, savedSearchArtifactId)
		{
		}

		public RelativityExporterService(FieldMap[] mappedFields, IJobStopManager jobStopManager, IHelper helper) : base(mappedFields, jobStopManager, helper)
		{
		}

		internal RelativityExporterService(IExporter exporter, IILongTextStreamFactory longTextStreamFactory, IJobStopManager jobStopManager, IHelper helper, FieldMap[] mappedFields, HashSet<int> longTextField, int[] avfIds) : base(exporter, longTextStreamFactory, jobStopManager, helper, mappedFields, longTextField, avfIds)
		{
		}

		public override IDataTransferContext GetDataTransferContext(IExporterTransferConfiguration transferConfiguration)
		{
			var documentTransferDataReader = new DocumentTransferDataReader(this, _mappedFields, _baseContext, transferConfiguration.ScratchRepositories);
			var exporterTransferContext = new ExporterTransferContext(documentTransferDataReader,transferConfiguration) {TotalItemsFound = TotalRecordsFound};
			return _context ?? (_context = exporterTransferContext);
		}

		public override ArtifactDTO[] RetrieveData(int size)
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

					SetupBaseFields(documentArtifactId, fieldsValue, fields);

					// TODO: replace String.empty
					result.Add(new ArtifactDTO(documentArtifactId, artifactType, string.Empty, fields));
				}
			}
			_retrievedDataCount += result.Count;
			return result.ToArray();
		}


	}
}