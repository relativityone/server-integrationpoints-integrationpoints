using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.SecretStore;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public partial class RSAPIService
	{
		private readonly IGenericLibraryFactory _genericLibraryFactory;
		private readonly IDictionary<Type, object> _genericLibraries;

		public IGenericLibrary<T> GetGenericLibrary<T>() where T : BaseRdo, new()
		{
			if (!_genericLibraries.ContainsKey(typeof(T)))
			{
				_genericLibraries.Add(typeof(T), _genericLibraryFactory.Create<T>());
			}
			return (IGenericLibrary<T>) _genericLibraries[typeof(T)];
		}

		public RSAPIService(IHelper helper, int workspaceArtifactId)
			: this(new GenericLibraryFactory(helper, workspaceArtifactId, new DefaultSecretCatalogFactory(), new SecretManager(workspaceArtifactId)))
		{
		}

		internal RSAPIService(IGenericLibraryFactory genericLibraryFactory)
		{
			_genericLibraryFactory = genericLibraryFactory;
			_genericLibraries = new ConcurrentDictionary<Type, object>();
		}
	}
}