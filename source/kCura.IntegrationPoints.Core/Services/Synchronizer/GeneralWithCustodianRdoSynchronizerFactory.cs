using System;
using Castle.Windsor;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Services.Synchronizer
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

		public IDataSynchronizer CreateSynchronizer(Guid identifier, string options)
		{
			var json = JsonConvert.DeserializeObject<ImportSettings>(options);
			var rdoObjectType = _query.GetObjectType(json.ArtifactTypeId);

			if (json.Provider.ToLower() == "relativity")
			{
				return _container.Kernel.Resolve<IDataSynchronizer>(typeof(RdoSynchronizerPush).AssemblyQualifiedName);
			}

			//name is very bad, we should consider switching to guid
			switch (rdoObjectType.Name.ToLower())
			{
				case "custodian":
					var s = (RdoCustodianSynchronizer)_container.Kernel.Resolve<IDataSynchronizer>(typeof(RdoCustodianSynchronizer).AssemblyQualifiedName);
					s.TaskJobSubmitter = TaskJobSubmitter;
					return s;
				default:
					return _container.Kernel.Resolve<IDataSynchronizer>(typeof(RdoSynchronizerBase).AssemblyQualifiedName);
			}
		}
	}
}
