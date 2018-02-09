using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class ResourcePoolManager : IResourcePoolManager
	{
		#region Fields

		private readonly IResourcePoolRepository _resourcePoolRepository;
		private readonly IRsapiClientFactory _rsapiClientFactory;
		private readonly IHelper _helper;
		private readonly IAPILog _logger;

		#endregion //Fields

		#region Constructors

		public ResourcePoolManager(IRepositoryFactory repositoryFactory, IHelper helper, IRsapiClientFactory rsapiClientFactory)
		{
			_helper = helper;
			_resourcePoolRepository = repositoryFactory.GetResourcePoolRepository();
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ResourcePoolManager>();
			_rsapiClientFactory = rsapiClientFactory;
		}

		#endregion //Constructors

		#region Methods

		public List<ProcessingSourceLocationDTO> GetProcessingSourceLocation(int workspaceId)
		{
			Workspace wksp = GetWorkspace(workspaceId);
			if (wksp == null)
			{
				LogMissingWorkspaceError(workspaceId);
				throw new ArgumentException($"Cannot find workspace with artifact id: {workspaceId}");
			}

			List<ProcessingSourceLocationDTO> processingSourceLocations =
				_resourcePoolRepository.GetProcessingSourceLocationsByResourcePool(wksp.ResourcePoolID.Value);
			return processingSourceLocations;
		}

		/// <summary>
		///     This method is extracted to remove rsapi unit testing limitatiations
		/// </summary>
		protected virtual Workspace GetWorkspace(int workspaceId)
		{
			using (IRSAPIClient rsApiClient = _rsapiClientFactory.CreateAdminClient(_helper))
			{
				rsApiClient.APIOptions.WorkspaceID = -1;
				return rsApiClient.Repositories.Workspace.Read(workspaceId).Results.FirstOrDefault()?.Artifact;
			}
		}

		#endregion //Methods

		#region Logging

		private void LogMissingWorkspaceError(int workspaceId)
		{
			_logger.LogError("Cannot find workspace with artifact id: {WorkspaceId}.", workspaceId);
		}

		#endregion
	}
}