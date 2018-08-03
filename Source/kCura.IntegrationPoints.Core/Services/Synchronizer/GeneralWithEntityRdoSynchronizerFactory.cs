using System;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
	public class GeneralWithEntityRdoSynchronizerFactory : ISynchronizerFactory
	{
		private readonly RelativityRdoSynchronizerFactory _relativityRdoSynchronizerFactory;
		private readonly ImportProviderRdoSynchronizerFactory _importProviderRdoSynchronizerFactory;

		public GeneralWithEntityRdoSynchronizerFactory(IWindsorContainer container, IObjectTypeRepository objectTypeRepository, IRsapiClientFactory rsapiClientFactory)
		{
			_relativityRdoSynchronizerFactory = new RelativityRdoSynchronizerFactory(container, rsapiClientFactory);
			_importProviderRdoSynchronizerFactory = new ImportProviderRdoSynchronizerFactory(container, objectTypeRepository);
		}

		public ITaskJobSubmitter TaskJobSubmitter { get; set; }

		public SourceProvider SourceProvider { get; set; }

		public IDataSynchronizer CreateSynchronizer(Guid identifier, string options)
		{
			return CreateSynchronizer(identifier, options, null);
		}

		public IDataSynchronizer CreateSynchronizer(Guid identifier, string options, string credentials)
		{
			ImportSettings importSettings = JsonConvert.DeserializeObject<ImportSettings>(options);

			return importSettings.IsRelativityProvider()
				? _relativityRdoSynchronizerFactory.CreateSynchronizer(credentials, importSettings, SourceProvider)
				: _importProviderRdoSynchronizerFactory.CreateSynchronizer(importSettings, TaskJobSubmitter);
		}
	}
}
