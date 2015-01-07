using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Contracts.Syncronizer;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Conversion;
using kCura.IntegrationPoints.Core.Services.Conversion;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.Syncronizer;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncWorker : ITask
	{
		private IServiceContext _serviceContext;
		private IDataSyncronizerFactory _dataSyncronizerFactory;
		private IDataProviderFactory _dataProviderFactory;
		private kCura.Apps.Common.Utils.Serializers.ISerializer _serializer;

		public SyncWorker(IServiceContext serviceContext, IDataSyncronizerFactory dataSyncronizerFactory, IDataProviderFactory dataProviderFactory,kCura.Apps.Common.Utils.Serializers.ISerializer serializer)
		{
			_serviceContext = serviceContext;
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
				rdoIntegrationPoint = _serviceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointID);
				if (rdoIntegrationPoint == null) throw new Exception("Failed to retrieved corresponding Integration Point.");

				IEnumerable<FieldMap> fieldMap = _serializer.Deserialize<List<FieldMap>>(rdoIntegrationPoint.FieldMappings);

				List<string> entryIDs = _serializer.Deserialize<List<string>>(job.JobDetails);
				IDataSourceProvider sourceProvider = _dataProviderFactory.GetDataProvider(Guid.NewGuid());
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
					_serviceContext.RsapiService.IntegrationPointLibrary.Update(rdoIntegrationPoint);
				}
			}
		}
	}
}
