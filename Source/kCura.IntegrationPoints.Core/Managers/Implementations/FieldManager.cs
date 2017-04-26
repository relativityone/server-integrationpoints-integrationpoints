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
			IFieldQueryRepository fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(workspaceArtifactId);
			return fieldQueryRepository.RetrieveArtifactViewFieldId(fieldArtifactId);
		}

		public ArtifactFieldDTO[] RetrieveBeginBatesFields(int workspaceArtifactId)
		{
			IFieldQueryRepository fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(workspaceArtifactId);
			return fieldQueryRepository.RetrieveBeginBatesFields();
		}

		public ArtifactDTO[] RetrieveFields(int workspaceId, int rdoTypeId, HashSet<string> fieldNames)
		{
			IFieldQueryRepository fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(workspaceId);
			return fieldQueryRepository.RetrieveFields(rdoTypeId, fieldNames);
		}

		public ArtifactDTO[] RetrieveFields(int workspaceId, HashSet<string> fieldNames)
		{
			IFieldQueryRepository fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(workspaceId);
			return fieldQueryRepository.RetrieveFields(Convert.ToInt32(ArtifactType.Document), fieldNames);
		}
	}
}
