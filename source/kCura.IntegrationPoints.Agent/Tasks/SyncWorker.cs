using System;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueueAgent;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncWorker :  ITask
	{
		private IServiceContext _serviceContext;
		public SyncWorker(IServiceContext serviceContext)
		{
			_serviceContext = serviceContext;
		}
		
		public void Execute(Job job)
		{
			int integrationPointID = job.RelatedObjectArtifactID;
			IntegrationPoint rdoIntegrationPoint = _serviceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointID);

			if (rdoIntegrationPoint==null) throw new Exception("Failed to retrieved corresponding Integration Point.");



			//RdoSynchronizer rdoSync = new RdoSynchronizer();
			
			//rdoIntegrationPoint.DestinationConfiguration
			
			//var client = new RSAPIClient(new Uri("net.pipe://localhost/Relativity.Services"), new IntegratedAuthCredentials())
			//{
			//	APIOptions = { WorkspaceID = CaseArtifactId }
			//};

			//var rdo = new RdoSynchronizer(new RelativityFieldQuery(client));
			//ImportSettings settings = new ImportSettings();

		}
	}
}
