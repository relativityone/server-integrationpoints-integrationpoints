using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;

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

		public ArtifactFieldDTO[] RetrieveBeginBatesFields(int workspaceArtifactId)
		{
			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(workspaceArtifactId);
			return fieldRepository.RetrieveBeginBatesFields();
		}

		public ArtifactDTO[] RetrieveFields(int workspaceId, int rdoTypeId, HashSet<string> fieldNames)
		{
			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(workspaceId);
			return fieldRepository.RetrieveFields(rdoTypeId, fieldNames);
		}

		public ArtifactDTO[] RetrieveFields(int workspaceId, HashSet<string> fieldNames)
		{
			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(workspaceId);
			return fieldRepository.RetrieveFields(Convert.ToInt32(ArtifactType.Document), fieldNames);
		}
	}
}
