using System;
using Castle.Windsor;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Services.Syncronizer
{
	public class GeneralWithCustodianRdoSynchronizerFactory : kCura.IntegrationPoints.Contracts.ISynchronizerFactory
	{
		private readonly IWindsorContainer _container;
		private readonly RSAPIRdoQuery _query;
		public GeneralWithCustodianRdoSynchronizerFactory(IWindsorContainer container, RSAPIRdoQuery query)
		{
			_container = container;
			_query = query;
		}

		public ITaskJobSubmitter TaskJobSubmitter { get; set; }

		public IDataSynchronizer CreateSyncronizer(Guid identifier, string options)
		{
			var json = JsonConvert.DeserializeObject<ImportSettings>(options);
			var rdoObjectType = _query.GetObjectType(json.ArtifactTypeId);
			//name is very bad, we should consider switching to guid
			switch (rdoObjectType.Name.ToLower())
			{
				case "custodian":
					var s = _container.Kernel.Resolve<kCura.IntegrationPoints.Synchronizers.RDO.RDOCustodianSynchronizer>();
					s.TaskJobSubmitter = TaskJobSubmitter;
					return s;
				default:
					return _container.Kernel.Resolve<kCura.IntegrationPoints.Synchronizers.RDO.RdoSynchronizer>();
			}

		}
	}
}
