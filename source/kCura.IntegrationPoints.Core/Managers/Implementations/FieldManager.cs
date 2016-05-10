using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class FieldManager : IFieldManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public FieldManager(IContextContainer contextContainer)
		:this(new RepositoryFactory(contextContainer.Helper))
		{ }
		
		/// <summary>
		/// Unit tests should be the only external consumers of this constructor
		/// </summary>
		internal FieldManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public Dictionary<Guid, int> RetrieveFieldArtifactIds(int workspaceArtifactId, IEnumerable<Guid> fieldGuids)
		{
			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(workspaceArtifactId);
			return fieldRepository.RetrieveFieldArtifactIds(fieldGuids);
		}
		public int? RetrieveArtifactViewFieldId(int workspaceArtifactId, int fieldArtifactId)
		{
			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(workspaceArtifactId);
			return fieldRepository.RetrieveArtifactViewFieldId(fieldArtifactId);
		}
	}
}
