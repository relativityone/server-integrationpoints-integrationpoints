using System;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services.Syncronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueueAgent;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncWorker : ITask
	{
		private IServiceContext _serviceContext;
		private IDataSyncronizerFactory _dataSyncronizerFactory;
		public SyncWorker(IServiceContext serviceContext, IDataSyncronizerFactory dataSyncronizerFactory)
		{
			_serviceContext = serviceContext;
		}

		public void Execute(Job job)
		{
			IntegrationPoint rdoIntegrationPoint = null;
			try
			{
				int integrationPointID = job.RelatedObjectArtifactID;
				rdoIntegrationPoint = _serviceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointID);
				if (rdoIntegrationPoint == null) throw new Exception("Failed to retrieved corresponding Integration Point.");

				SourceProvider sourceProvider =
					_serviceContext.RsapiService.SourceProviderLibrary.Read(rdoIntegrationPoint.SourceProvider.Value);
				SourceProvider destinationProvider =
					_serviceContext.RsapiService.SourceProviderLibrary.Read(rdoIntegrationPoint.SourceProvider.Value);
				//sourceProvider.Identifier 

				//IDataSyncronizer ds = _dataSyncronizerFactory.GetSyncronizer(rdoIntegrationPoint.SourceProvider);

				//rdoIntegrationPoint.DestinationConfiguration

				//var client = new RSAPIClient(new Uri("net.pipe://localhost/Relativity.Services"), new IntegratedAuthCredentials())
				//{
				//	APIOptions = { WorkspaceID = CaseArtifactId }
				//};

				//var rdo = new RdoSynchronizer(new RelativityFieldQuery(client));
				//ImportSettings settings = new ImportSettings();
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
