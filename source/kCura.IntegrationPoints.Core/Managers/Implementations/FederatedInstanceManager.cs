using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Toggles;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class FederatedInstanceManager : IFederatedInstanceManager
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IToggleProvider _toggleProvider;
		const string ARTIFACT_TYPE_NAME = "FederatedInstance";

		private static readonly FederatedInstanceDto _localInstanceDto = new FederatedInstanceDto()
		{
			Name = "This Instance",
			ArtifactId = null
		};

		public FederatedInstanceManager(IRepositoryFactory repositoryFactory, IToggleProvider toggleProvider)
		{
			_repositoryFactory = repositoryFactory;
			_toggleProvider = toggleProvider;
		}

		public static FederatedInstanceDto LocalInstance 
		{
			get { return _localInstanceDto; }
		}

		public FederatedInstanceDto RetrieveFederatedInstanceByName(string instanceName)
		{
			if (instanceName.Equals(LocalInstance.Name, StringComparison.Ordinal))
				return LocalInstance;

			IFederatedInstanceRepository federatedInstanceRepository = GetFederatedInstanceRepository();

			FederatedInstanceDto federatedInstance =
				federatedInstanceRepository.RetrieveFederatedInstance(instanceName);

			LoadUrl(federatedInstance);

			return federatedInstance;
		}

		public FederatedInstanceDto RetrieveFederatedInstanceByArtifactId(int? artifactId)
		{
			if (artifactId.HasValue)
			{
				IFederatedInstanceRepository federatedInstanceRepository = GetFederatedInstanceRepository();

				FederatedInstanceDto federatedInstance =
					federatedInstanceRepository.RetrieveFederatedInstance(artifactId.Value);

				LoadUrl(federatedInstance);

				return federatedInstance;
			}

			return LocalInstance;
		}

		public IEnumerable<FederatedInstanceDto> RetrieveAll()
		{
			var instances = new List<FederatedInstanceDto>();

			if (_toggleProvider.IsEnabled<RipToR1Toggle>())
			{
				IFederatedInstanceRepository federatedInstanceRepository = GetFederatedInstanceRepository();

				instances = federatedInstanceRepository.RetrieveAll().ToList();

				LoadUrls(instances);
			}

			// add local instance
			instances.Insert(0, LocalInstance);

			return instances;
		}

		private void LoadUrls(IEnumerable<FederatedInstanceDto> federatedInstances)
		{
			IServiceUrlRepository serviceUrlRepository = _repositoryFactory.GetServiceUrlRepository();

			foreach (var federatedInstance in federatedInstances)
			{
				LoadUrl(federatedInstance, serviceUrlRepository);
			}
		}

		private void LoadUrl(FederatedInstanceDto federatedInstance)
		{
			IServiceUrlRepository serviceUrlRepository = _repositoryFactory.GetServiceUrlRepository();
			LoadUrl(federatedInstance, serviceUrlRepository);
		}

		private void LoadUrl(FederatedInstanceDto federatedInstance, IServiceUrlRepository serviceUrlRepository)
		{
			InstanceUrlCollectionDTO instanceUrlCollection = serviceUrlRepository.RetrieveInstanceUrlCollection(new Uri(federatedInstance.InstanceUrl));

			federatedInstance.RsapiUrl = instanceUrlCollection.RsapiUrl;
			federatedInstance.KeplerUrl = instanceUrlCollection.KeplerUrl;
			federatedInstance.WebApiUrl = instanceUrlCollection.WebApiUrl;
		}

		private IFederatedInstanceRepository GetFederatedInstanceRepository()
		{
			IArtifactTypeRepository artifactTypeRepository = _repositoryFactory.GetArtifactTypeRepository();
			int artifactTypeId = artifactTypeRepository.GetArtifactTypeIdFromArtifactTypeName(ARTIFACT_TYPE_NAME);
			return _repositoryFactory.GetFederatedInstanceRepository(artifactTypeId);
		}
	}
}