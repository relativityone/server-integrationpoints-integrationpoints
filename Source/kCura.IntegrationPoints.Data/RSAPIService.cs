using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public partial class RSAPIService : IRSAPIService
	{
		private readonly IDictionary<Type, object> _genericLibraries;
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;

		public IGenericLibrary<Document> DocumentLibrary => GetGenericLibrary<Document>();
		public IGenericLibrary<IntegrationPoint> IntegrationPointLibrary => GetGenericLibrary<IntegrationPoint>();
		public IGenericLibrary<SourceProvider> SourceProviderLibrary => GetGenericLibrary<SourceProvider>();
		public IGenericLibrary<DestinationProvider> DestinationProviderLibrary => GetGenericLibrary<DestinationProvider>();
		public IGenericLibrary<JobHistory> JobHistoryLibrary => GetGenericLibrary<JobHistory>();
		public IGenericLibrary<JobHistoryError> JobHistoryErrorLibrary => GetGenericLibrary<JobHistoryError>();
		public IGenericLibrary<DestinationWorkspace> DestinationWorkspaceLibrary => GetGenericLibrary<DestinationWorkspace>();
		public IGenericLibrary<IntegrationPointType> IntegrationPointTypeLibrary => GetGenericLibrary<IntegrationPointType>();
		public IGenericLibrary<IntegrationPointProfile> IntegrationPointProfileLibrary => GetGenericLibrary<IntegrationPointProfile>();
	
		public IGenericLibrary<T> GetGenericLibrary<T>() where T : BaseRdo, new()
		{
			if (!_genericLibraries.ContainsKey(typeof(T)))
			{
				_genericLibraries.Add(typeof(T), new RsapiClientLibrary<T>(_helper, _workspaceArtifactId));
			}
			return (IGenericLibrary<T>) _genericLibraries[typeof(T)];
		}

		public RSAPIService(IHelper helper, int workspaceArtifactId)
		{
			_genericLibraries = new ConcurrentDictionary<Type, object>();
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
		}
	}
}
