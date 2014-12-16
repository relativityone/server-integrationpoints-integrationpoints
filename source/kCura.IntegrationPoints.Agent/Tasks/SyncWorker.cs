using System;
using kCura.IntegrationPoints.Core;
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
			_serviceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointID);

			//var client = new RSAPIClient(new Uri("net.pipe://localhost/Relativity.Services"), new IntegratedAuthCredentials())
			//{
			//	APIOptions = { WorkspaceID = CaseArtifactId }
			//};

			//var rdo = new RdoSynchronizer(new RelativityFieldQuery(client));
			//ImportSettings settings = new ImportSettings();

		}
	}
}
