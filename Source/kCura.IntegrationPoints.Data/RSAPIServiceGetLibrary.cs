using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using kCura.IntegrationPoints.Security;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public partial class RSAPIService
	{
		private readonly IEncryptionManager _encryptionManager;
		private readonly IDictionary<Type, object> _genericLibraries;
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;

		public IGenericLibrary<T> GetGenericLibrary<T>() where T : BaseRdo, new()
		{
			if (!_genericLibraries.ContainsKey(typeof(T)))
			{
				if (typeof(T) == typeof(IntegrationPoint))
				{
					_genericLibraries.Add(typeof(T), new IntegrationPointRsapiClientLibrary(_helper, _workspaceArtifactId, _encryptionManager));
				}
				else
				{
					_genericLibraries.Add(typeof(T), new RsapiClientLibrary<T>(_helper, _workspaceArtifactId));
				}
			}
			return (IGenericLibrary<T>) _genericLibraries[typeof(T)];
		}

		public RSAPIService(IHelper helper, int workspaceArtifactId) : this(helper, workspaceArtifactId, new DefaultEncryptionManager())
		{
		}

		public RSAPIService(IHelper helper, int workspaceArtifactId, IEncryptionManager encryptionManager)
		{
			_genericLibraries = new ConcurrentDictionary<Type, object>();
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
			_encryptionManager = encryptionManager;
		}
	}
}