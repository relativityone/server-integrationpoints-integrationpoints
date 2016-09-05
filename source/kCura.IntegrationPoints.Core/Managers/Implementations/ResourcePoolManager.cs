using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;


namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class ResourcePoolManager : IResourcePoolManager
	{
		#region Fields

		private readonly IResourcePoolRepository _resourcePoolRepository;
		private readonly IRSAPIClient _rsapiClient;

		#endregion //Fields

		#region Constructors

		public ResourcePoolManager(IRepositoryFactory repositoryFactory, IRSAPIClient rsapiClient)
		{
			_rsapiClient = rsapiClient;
			_resourcePoolRepository = repositoryFactory.GetResourcePoolRepository();
		}

		#endregion //Constructors

		#region Methods

		public List<ProcessingSourceLocationDTO> GetProcessingSourceLocation(int workspaceId)
		{
			var wkspId = _rsapiClient.APIOptions.WorkspaceID;
			try
			{
				_rsapiClient.APIOptions.WorkspaceID = -1;

				Workspace wksp = _rsapiClient.Repositories.Workspace.Read(workspaceId).Results.FirstOrDefault()?.Artifact;

				if (wksp == null)
				{
					throw new ArgumentException($"Cannot find workspace with artifact id: {workspaceId}");
				}

				List<ProcessingSourceLocationDTO> processingSourceLocations =
					_resourcePoolRepository.GetProcessingSourceLocationsBy(wksp.ResourcePoolID.Value);
				return processingSourceLocations;
			}
			finally
			{
				_rsapiClient.APIOptions.WorkspaceID = wkspId;
			}
		}

		#endregion //Methods
	}
}
