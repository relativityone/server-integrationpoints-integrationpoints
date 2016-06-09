using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class FieldManager : IFieldManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		internal FieldManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public int? RetrieveArtifactViewFieldId(int workspaceArtifactId, int fieldArtifactId)
		{
			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(workspaceArtifactId);
			return fieldRepository.RetrieveArtifactViewFieldId(fieldArtifactId);
		}
	}
}
