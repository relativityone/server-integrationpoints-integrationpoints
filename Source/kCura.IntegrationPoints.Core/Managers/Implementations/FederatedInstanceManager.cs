﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class FederatedInstanceManager : IFederatedInstanceManager
	{
		private const string _ARTIFACT_TYPE_NAME = "Federated Instance";

		private readonly IRepositoryFactory _repositoryFactory;

		public FederatedInstanceManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public static FederatedInstanceDto LocalInstance { get; } = new FederatedInstanceDto
		{
			Name = "This Instance",
			ArtifactId = null
		};
		
		public FederatedInstanceDto RetrieveFederatedInstanceByArtifactId(int? artifactId)
		{
			if (artifactId.HasValue)
			{
				IFederatedInstanceRepository federatedInstanceRepository = GetFederatedInstanceRepository();

				FederatedInstanceDto federatedInstance =
					federatedInstanceRepository.RetrieveFederatedInstance(artifactId.Value);

				if (federatedInstance != null)
				{
					LoadUrl(federatedInstance);
				}

				return federatedInstance;
			}

			return LocalInstance;
		}

		public IEnumerable<FederatedInstanceDto> RetrieveAll()
		{
			IFederatedInstanceRepository federatedInstanceRepository = GetFederatedInstanceRepository();

			var instances = federatedInstanceRepository.RetrieveAll().ToList();

			LoadUrls(instances);

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
			int artifactTypeID = artifactTypeRepository.GetArtifactTypeIDFromArtifactTypeName(_ARTIFACT_TYPE_NAME);
			return _repositoryFactory.GetFederatedInstanceRepository(artifactTypeID);
		}
	}
}