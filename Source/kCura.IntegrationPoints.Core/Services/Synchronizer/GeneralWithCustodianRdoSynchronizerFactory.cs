using System;
using System.Collections.Generic;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
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

			if (json.Provider != null && json.Provider.ToLower() == "relativity")
			{
				IRSAPIClient client;
				IHelper sourceInstanceHelper = _container.Resolve<IHelper>();
				if (json.FederatedInstanceArtifactId != null)
				{
					IHelperFactory helperFactory = _container.Resolve<IHelperFactory>();
					IHelper targetHelper = helperFactory.CreateOAuthClientHelper(sourceInstanceHelper, json.FederatedInstanceArtifactId.Value);
					client = targetHelper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
					client.APIOptions.WorkspaceID = json.CaseArtifactId;
				}
				else
				{
					client = _container.Resolve<IRSAPIClient>();
					client.APIOptions.WorkspaceID = json.CaseArtifactId;
				}
				Dictionary<string, RelativityFieldQuery> dict = new Dictionary<string, RelativityFieldQuery>
				{
					{"fieldQuery", new RelativityFieldQuery(client, sourceInstanceHelper)},
				};
				IDataSynchronizer synchronizer = _container.Kernel.Resolve<IDataSynchronizer>(typeof(RdoSynchronizer).AssemblyQualifiedName, dict);
				RdoSynchronizer syncBase = (RdoSynchronizer)synchronizer;
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
					return _container.Kernel.Resolve<IDataSynchronizer>(typeof(RdoSynchronizer).AssemblyQualifiedName);
			}
		}
	}
}
