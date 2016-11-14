using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client.DTOs;

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
			IEnumerable<int> accessibleWorkspacesForCurrentUser = GetWorkspacesAccessibleForCurrentUser();
			IEnumerable<WorkspaceDTO> activeWorkspaces = RetrieveAllActiveWorkspaces();
			return  activeWorkspaces.Where(_ => accessibleWorkspacesForCurrentUser.Contains(_.ArtifactId));
		}

		private IEnumerable<WorkspaceDTO> RetrieveAllActiveWorkspaces()
		{
			IWorkspacesRepository repository = _repositoryFactory.GetWorkspacesRepository();
			return repository.RetrieveAllActive();
		}

		private IEnumerable<int> GetWorkspacesAccessibleForCurrentUser()
		{
			IRdoRepository rdoRepository = _repositoryFactory.GetRdoRepository(workspaceArtifactId:-1);
			var query = new Query<RDO>()
			{
				ArtifactTypeName = ArtifactTypeNames.Workspace,
				Fields = FieldValue.NoFields
			};
			return rdoRepository.Query(query).Results.Select(_ => _.Artifact.ArtifactID);
		}
	}
}