using System;
using System.Collections.Generic;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
	public class GeneralWithCustodianRdoSynchronizerFactory : ISynchronizerFactory
	{
		private readonly IWindsorContainer _container;
		private readonly RSAPIRdoQuery _query;
		public GeneralWithCustodianRdoSynchronizerFactory(IWindsorContainer container, RSAPIRdoQuery query)
		{
			_container = container;
			_query = query;
		}

		public ITaskJobSubmitter TaskJobSubmitter { get; set; }

		public SourceProvider SourceProvider { get; set; }

		public IDataSynchronizer CreateSynchronizer(Guid identifier, string options)
		{
			var json = JsonConvert.DeserializeObject<ImportSettings>(options);
			var rdoObjectType = _query.GetObjectType(json.ArtifactTypeId);

            if (json.DestinationProviderType != null && json.DestinationProviderType.ToLower() == "load file")
                return _container.Kernel.Resolve<IDataSynchronizer>(typeof(ExportSynchroznizer).AssemblyQualifiedName);

            if (json.Provider != null && json.Provider.ToLower() == "relativity")
			{ 
				IRSAPIClient client = _container.Resolve<IRSAPIClient>();
				IHelper helper = _container.Resolve<IHelper>();
				client.APIOptions.WorkspaceID = json.CaseArtifactId;
				Dictionary<string, RelativityFieldQuery> dict = new Dictionary<string, RelativityFieldQuery>
				{
					{"fieldQuery", new RelativityFieldQuery(client, helper)},
				};
				IDataSynchronizer synchronizer = _container.Kernel.Resolve<IDataSynchronizer>(typeof (RdoSynchronizerPush).AssemblyQualifiedName, dict);
				RdoSynchronizerPush syncBase = (RdoSynchronizerPush) synchronizer;
				syncBase.SourceProvider = SourceProvider;
				return syncBase;
			}

			//name is very bad, we should consider switching to guid
			switch (rdoObjectType.Name.ToLower())
			{
				case "custodian":
					var s = (RdoCustodianSynchronizer)_container.Kernel.Resolve<IDataSynchronizer>(typeof(RdoCustodianSynchronizer).AssemblyQualifiedName);
					s.TaskJobSubmitter = TaskJobSubmitter;
					return s;
				default:
					return _container.Kernel.Resolve<IDataSynchronizer>(typeof(RdoSynchronizerPull).AssemblyQualifiedName);
			}
		}
	}
}
