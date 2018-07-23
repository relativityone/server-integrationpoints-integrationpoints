using System.Collections.Generic;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
	internal class RelativityRdoSynchronizerFactory
	{
		private readonly IWindsorContainer _container;
		private readonly IRsapiClientFactory _rsapiClientFactory;

		public RelativityRdoSynchronizerFactory(IWindsorContainer container, IRsapiClientFactory rsapiClientFactory)
		{
			_container = container;
			_rsapiClientFactory = rsapiClientFactory;
		}

		public IDataSynchronizer CreateSynchronizer(string credentials, ImportSettings importSettings, SourceProvider sourceProvider)
		{
			Dictionary<string, RelativityFieldQuery> rdoSynchronizerParametersDictionary = CreateRdoSynchronizerParametersDictionary(credentials, importSettings);

			IDataSynchronizer synchronizer = _container.Kernel.Resolve<IDataSynchronizer>(typeof(RdoSynchronizer).AssemblyQualifiedName, rdoSynchronizerParametersDictionary);
			RdoSynchronizer syncBase = (RdoSynchronizer)synchronizer;
			syncBase.SourceProvider = sourceProvider;
			return syncBase;
		}

		private Dictionary<string, RelativityFieldQuery> CreateRdoSynchronizerParametersDictionary(string credentials, ImportSettings importSettings)
		{
			IHelper sourceInstanceHelper = _container.Resolve<IHelper>();
			IRSAPIClient client = CreateRsapiClient(credentials, importSettings, sourceInstanceHelper);

			return new Dictionary<string, RelativityFieldQuery>
			{
				{"fieldQuery", new RelativityFieldQuery(client, sourceInstanceHelper)}
			};
		}

		private IRSAPIClient CreateRsapiClient(string credentials, ImportSettings importSettings, IHelper sourceInstanceHelper)
		{
			IRSAPIClient client;

			if (importSettings.IsFederatedInstance())
			{
				IHelperFactory helperFactory = _container.Resolve<IHelperFactory>();
				IHelper targetHelper = helperFactory.CreateTargetHelper(sourceInstanceHelper, importSettings.FederatedInstanceArtifactId, credentials);
				client = _rsapiClientFactory.CreateUserClient(targetHelper);
			}
			else
			{
				client = _container.Resolve<IRSAPIClient>();
			}

			client.APIOptions.WorkspaceID = importSettings.CaseArtifactId;

			return client;
		}
	}
}
