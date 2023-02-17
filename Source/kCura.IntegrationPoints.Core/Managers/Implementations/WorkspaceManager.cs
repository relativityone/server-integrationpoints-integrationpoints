using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class WorkspaceManager : IWorkspaceManager
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

        public WorkspaceDTO RetrieveWorkspace(int workspaceArtifactId, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
        {
            IWorkspaceRepository workspaceRepository = _repositoryFactory.GetWorkspaceRepository();
            WorkspaceDTO workspace = workspaceRepository.Retrieve(workspaceArtifactId, executionIdentity);

            return workspace;
        }

        public bool WorkspaceExists(int workspaceArtifactId)
        {
            IWorkspaceRepository workspaceRepository = _repositoryFactory.GetWorkspaceRepository();
            WorkspaceDTO workspace = workspaceRepository.Retrieve(workspaceArtifactId, ExecutionIdentity.System);
            return workspace != null;
        }

        public IEnumerable<WorkspaceDTO> GetUserAvailableDestinationWorkspaces(int sourceWorkspaceId)
        {
            return GetUserActiveWorkspaces().Where(w => w.ArtifactId != sourceWorkspaceId);
        }

        private IEnumerable<WorkspaceDTO> RetrieveAllActiveWorkspaces()
        {
            IWorkspaceRepository repository = _repositoryFactory.GetWorkspaceRepository();
            return repository.RetrieveAllActive();
        }
    }
}
