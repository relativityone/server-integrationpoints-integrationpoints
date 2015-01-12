﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Contracts.Syncronizer;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Conversion;
using kCura.IntegrationPoints.Core.Services.Conversion;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Syncronizer;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncWorker : ITask
	{
		private ICaseServiceContext _caseServiceContext;
		private IDataSyncronizerFactory _dataSyncronizerFactory;
		private IDataProviderFactory _dataProviderFactory;
		private kCura.Apps.Common.Utils.Serializers.ISerializer _serializer;

		public SyncWorker(ICaseServiceContext caseServiceContext, IDataSyncronizerFactory dataSyncronizerFactory, IDataProviderFactory dataProviderFactory, kCura.Apps.Common.Utils.Serializers.ISerializer serializer)
		{
			_caseServiceContext = caseServiceContext;
			_dataSyncronizerFactory = dataSyncronizerFactory;
			_dataProviderFactory = dataProviderFactory;
			_serializer = serializer;
		}

		public void Execute(Job job)
		{
			IntegrationPoint rdoIntegrationPoint = null;
			try
			{
				int integrationPointID = job.RelatedObjectArtifactID;
				rdoIntegrationPoint = _caseServiceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointID);
				if (rdoIntegrationPoint == null) throw new Exception("Failed to retrieved corresponding Integration Point.");

				IEnumerable<FieldMap> fieldMap = _serializer.Deserialize<List<FieldMap>>(rdoIntegrationPoint.FieldMappings);

				List<string> entryIDs = _serializer.Deserialize<List<string>>(job.JobDetails);
				IDataSourceProvider sourceProvider = _dataProviderFactory.GetDataProvider(Guid.NewGuid());
				fieldMap.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);
				List<FieldEntry> sourceFields = fieldMap.Select(f => f.SourceField).ToList();
				IDataReader sourceDataReader = sourceProvider.GetData(sourceFields, entryIDs, rdoIntegrationPoint.SourceConfiguration);

				IDataSyncronizer dataSyncronizer = _dataSyncronizerFactory.GetSyncronizer();
				IEnumerable<IDictionary<FieldEntry, object>> data =
					new DataReaderToEnumerableService(
						new SynchronizerObjectBuilder(sourceFields))
						.GetData<IDictionary<FieldEntry, object>>(sourceDataReader);

				dataSyncronizer.SyncData(data, fieldMap, rdoIntegrationPoint.DestinationConfiguration);
			}
			catch
			{
				throw;
			}
			finally
			{
				if (rdoIntegrationPoint != null)
				{
					rdoIntegrationPoint.LastRuntime = DateTime.UtcNow;
					//TODO:
					//rdoIntegrationPoint.NextScheduledRuntime = ???;
					_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(rdoIntegrationPoint);
				}
			}
		}
	}
}
