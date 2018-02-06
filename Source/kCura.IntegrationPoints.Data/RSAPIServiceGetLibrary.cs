using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.SecretStore;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public partial class RSAPIService
	{
		protected ExecutionIdentity ExecutionIdentity;
		private readonly IGenericLibraryFactory _genericLibraryFactory;
		private readonly IDictionary<Type, object> _genericLibraries;
		private readonly int _workspaceArtifactId;
		private readonly IHelper _helper;

		public IGenericLibrary<T> GetGenericLibrary<T>() where T : BaseRdo, new()
		{
			return GetGenericLibrary<T>(ExecutionIdentity);
		}

		protected IGenericLibrary<T> GetGenericLibrary<T>(ExecutionIdentity executionIdentity) where T : BaseRdo, new()
		{
			if (!_genericLibraries.ContainsKey(typeof(T)))
			{
				_genericLibraries.Add(typeof(T), _genericLibraryFactory.Create<T>(executionIdentity));
			}
			return (IGenericLibrary<T>)_genericLibraries[typeof(T)];
		}

		public RSAPIService(IHelper helper, int workspaceArtifactId)
			: this(new GenericLibraryFactory(helper, workspaceArtifactId, new DefaultSecretCatalogFactory(), new SecretManager(workspaceArtifactId)))
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
		}

		internal RSAPIService(IGenericLibraryFactory genericLibraryFactory)
		{
			_genericLibraryFactory = genericLibraryFactory;
			_genericLibraries = new ConcurrentDictionary<Type, object>();
			ExecutionIdentity = ExecutionIdentity.CurrentUser;
		}

		public IRelativityObjectManager GetRelativityObjectManager()
		{
			return new RelativityObjectManager(_workspaceArtifactId, _helper, new DefaultSecretCatalogFactory(), new SecretManager(_workspaceArtifactId));
		}
	}
}