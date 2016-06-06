using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class ErrorManager : IErrorManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		internal ErrorManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public void Create(int workspaceArtifactId, IEnumerable<ErrorDTO> errors)
		{
			IErrorRepository repository = _repositoryFactory.GetErrorRepository(workspaceArtifactId);

			repository.Create(errors);
		}
	}
}