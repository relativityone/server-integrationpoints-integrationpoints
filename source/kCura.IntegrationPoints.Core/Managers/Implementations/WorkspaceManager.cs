using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class WorkspaceManager:IWorkspaceManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public WorkspaceManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public IEnumerable<WorkspaceDTO> GetUserWorkspaces()
		{
			IWorkspaceRepository repository = _repositoryFactory.GetWorkspaceRepository();
			return repository.RetrieveAll();
		}

		public IEnumerable<WorkspaceDTO> GetUserActiveWorkspaces()
		{
			IEnumerable<WorkspaceDTO> userWorkspaces = GetUserWorkspaces();
			IEnumerable<WorkspaceDTO> activeWorkspaces = RetrieveAllActiveWorkspaces();
			return activeWorkspaces.Intersect(userWorkspaces);
		}

		public WorkspaceDTO RetrieveWorkspace(int workspaceArtifactId)
		{
			IWorkspaceRepository workspaceRepository = _repositoryFactory.GetWorkspaceRepository();
			WorkspaceDTO workspace = workspaceRepository.Retrieve(workspaceArtifactId);

			return workspace;
		}
		
		private IEnumerable<WorkspaceDTO> RetrieveAllActiveWorkspaces()
		{
			IWorkspacesRepository repository = _repositoryFactory.GetWorkspacesRepository();
			return repository.RetrieveAllActive();
		}
	}
}