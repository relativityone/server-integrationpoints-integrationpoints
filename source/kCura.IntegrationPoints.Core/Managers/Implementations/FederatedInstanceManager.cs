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
		private readonly IRepositoryFactory _repositoryFactory;
		const string ARTIFACT_TYPE_NAME = "FederatedInstance";

		private static readonly FederatedInstanceDto _localInstanceDto = new FederatedInstanceDto()
		{
			Name = "This Instance",
			ArtifactId = null
		};

		public FederatedInstanceManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public static FederatedInstanceDto LocalInstance 
		{
			get { return _localInstanceDto; }
		}

		public FederatedInstanceDto RetrieveFederatedInstance(int? artifactId)
		{
			if (artifactId.HasValue)
			{
				IArtifactTypeRepository artifactTypeRepository = _repositoryFactory.GetArtifactTypeRepository();
				int artifactTypeId = artifactTypeRepository.GetArtifactTypeIdFromArtifactTypeName(ARTIFACT_TYPE_NAME);
				IFederatedInstanceRepository federatedInstanceRepository =
					_repositoryFactory.GetFederatedInstanceRepository(artifactTypeId);

				FederatedInstanceDto federatedInstance =
					federatedInstanceRepository.RetrieveFederatedInstance(artifactId.Value);

				IServiceUrlRepository serviceUrlRepository = _repositoryFactory.GetServiceUrlRepository();

				LoadUrl(federatedInstance, serviceUrlRepository);

				return federatedInstance;
			}

			return LocalInstance;
		}

		public IEnumerable<FederatedInstanceDto> RetrieveAll()
		{
			IArtifactTypeRepository artifactTypeRepository = _repositoryFactory.GetArtifactTypeRepository();
			int artifactTypeId = artifactTypeRepository.GetArtifactTypeIdFromArtifactTypeName(ARTIFACT_TYPE_NAME);
			IFederatedInstanceRepository federatedInstanceRepository = _repositoryFactory.GetFederatedInstanceRepository(artifactTypeId);

			var federatedInstances =
				federatedInstanceRepository.RetrieveAll().ToList();

			LoadUrls(federatedInstances);

			// add local instance
			federatedInstances.Insert(0, LocalInstance);

			return federatedInstances;
		}

		private void LoadUrls(IEnumerable<FederatedInstanceDto> federatedInstances)
		{
			IServiceUrlRepository serviceUrlRepository = _repositoryFactory.GetServiceUrlRepository();

			foreach (var federatedInstance in federatedInstances)
			{
				LoadUrl(federatedInstance, serviceUrlRepository);
			}
		}

		private void LoadUrl(FederatedInstanceDto federatedInstance, IServiceUrlRepository serviceUrlRepository)
		{
			InstanceUrlCollectionDTO instanceUrlCollection = serviceUrlRepository.RetrieveInstanceUrlCollection(new Uri(federatedInstance.InstanceUrl));

			federatedInstance.RsapiUrl = instanceUrlCollection.RsapiUrl;
			federatedInstance.KeplerUrl = instanceUrlCollection.KeplerUrl;
			federatedInstance.WebApiUrl = instanceUrlCollection.WebApiUrl;
		}
	}
}