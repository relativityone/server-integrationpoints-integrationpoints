﻿using System;
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
			Workspace wksp = GetWorkspace(workspaceId);
			if (wksp == null)
			{
				throw new ArgumentException($"Cannot find workspace with artifact id: {workspaceId}");
			}

			List<ProcessingSourceLocationDTO> processingSourceLocations =
				_resourcePoolRepository.GetProcessingSourceLocationsBy(wksp.ResourcePoolID.Value);
			return processingSourceLocations;
		}

		/// <summary>
		/// This method is extracted to remove rsapi unit testing limitatiations
		/// </summary>
		protected virtual Workspace GetWorkspace(int workspaceId)
		{
			_rsapiClient.APIOptions.WorkspaceID = -1;
			return _rsapiClient.Repositories.Workspace.Read(workspaceId).Results.FirstOrDefault()?.Artifact;
		}

		#endregion //Methods
	}
}
