using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data
{
	public class RSAPIService : IRSAPIService
	{
		protected ExecutionIdentity ExecutionIdentity;
		private readonly IGenericLibraryFactory _genericLibraryFactory;
		private readonly IDictionary<Type, object> _genericLibraries;
		private readonly int _workspaceArtifactId;
		private readonly IHelper _helper;

		public virtual IGenericLibrary<SourceProvider> SourceProviderLibrary => GetGenericLibrary<SourceProvider>(ExecutionIdentity);
		public virtual IGenericLibrary<JobHistoryError> JobHistoryErrorLibrary => GetGenericLibrary<JobHistoryError>(ExecutionIdentity);
		public virtual IRelativityObjectManager RelativityObjectManager => GetRelativityObjectManager();

		protected IGenericLibrary<T> GetGenericLibrary<T>(ExecutionIdentity executionIdentity) where T : BaseRdo, new()
		{
			if (!_genericLibraries.ContainsKey(typeof(T)))
			{
				_genericLibraries.Add(typeof(T), _genericLibraryFactory.Create<T>(executionIdentity));
			}
			return (IGenericLibrary<T>)_genericLibraries[typeof(T)];
		}

		public RSAPIService(IHelper helper, int workspaceArtifactId)
			: this(new GenericLibraryFactory(helper, workspaceArtifactId))
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
			var factory = new RelativityObjectManagerFactory(_helper);
			return factory.CreateRelativityObjectManager(_workspaceArtifactId);
		}
	}
}