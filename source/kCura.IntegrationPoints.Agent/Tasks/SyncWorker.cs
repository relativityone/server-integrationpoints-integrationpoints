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

		public SyncWorker(IServiceContext serviceContext, IDataSyncronizerFactory dataSyncronizerFactory, IDataProviderFactory dataProviderFactory)
		{
			_serviceContext = serviceContext;
			_dataSyncronizerFactory = dataSyncronizerFactory;
			_dataProviderFactory = dataProviderFactory;
		}

		public void Execute(Job job)
		{
			IntegrationPoint rdoIntegrationPoint = null;
			try
			{
				int integrationPointID = job.RelatedObjectArtifactID;
				rdoIntegrationPoint = _serviceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointID);
				if (rdoIntegrationPoint == null) throw new Exception("Failed to retrieved corresponding Integration Point.");

				//SourceProvider sourceProvider =
				//	_serviceContext.RsapiService.SourceProviderLibrary.Read(rdoIntegrationPoint.SourceProvider.Value);
				//SourceProvider destinationProvider =
				//	_serviceContext.RsapiService.SourceProviderLibrary.Read(rdoIntegrationPoint.DestinationProvider.Value);

				IEnumerable<FieldMap> fieldMap = new JSONSerializer().Deserialize<List<FieldMap>>(rdoIntegrationPoint.FieldMappings);

				List<string> entryIDs = new JSONSerializer().Deserialize<List<string>>(job.JobDetails);
				IDataSourceProvider sourceProvider = _dataProviderFactory.GetDataProvider();
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
