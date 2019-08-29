using System;
using System.Collections.Generic;
using Castle.Windsor;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
	internal class RelativityRdoSynchronizerFactory
	{
		private readonly IWindsorContainer _container;

		public RelativityRdoSynchronizerFactory(IWindsorContainer container)
		{
			_container = container;
		}

		public IDataSynchronizer CreateSynchronizer(ImportSettings importSettings, SourceProvider sourceProvider)
		{
			Dictionary<string, RelativityFieldQuery> rdoSynchronizerParametersDictionary = CreateRdoSynchronizerParametersDictionary(importSettings);

			IDataSynchronizer synchronizer = _container.Kernel.Resolve<IDataSynchronizer>(typeof(RdoSynchronizer).AssemblyQualifiedName, rdoSynchronizerParametersDictionary);
			RdoSynchronizer syncBase = (RdoSynchronizer)synchronizer;
			syncBase.SourceProvider = sourceProvider;
			return syncBase;
		}

		private Dictionary<string, RelativityFieldQuery> CreateRdoSynchronizerParametersDictionary(ImportSettings importSettings)
		{
			IHelper sourceInstanceHelper = _container.Resolve<IHelper>();
			IRSAPIClient client = CreateRsapiClient(importSettings);

			return new Dictionary<string, RelativityFieldQuery>
			{
				{"fieldQuery", new RelativityFieldQuery(client, sourceInstanceHelper)}
			};
		}

		private IRSAPIClient CreateRsapiClient(ImportSettings importSettings)
		{
			IRSAPIClient client;

			if (importSettings.IsFederatedInstance())
			{
				throw new InvalidOperationException("i2i is not supported");
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
