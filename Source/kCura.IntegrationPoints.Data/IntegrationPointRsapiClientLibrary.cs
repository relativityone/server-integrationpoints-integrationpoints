using System.Collections.Generic;
using kCura.IntegrationPoints.Security;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public class IntegrationPointRsapiClientLibrary : RsapiClientLibrary<IntegrationPoint>
	{
		private readonly IEncryptionManager _encryptionManager;

		public IntegrationPointRsapiClientLibrary(IHelper helper, int workspaceArtifactId, IEncryptionManager encryptionManager) : base(helper, workspaceArtifactId)
		{
			_encryptionManager = encryptionManager;
		}

		public override List<int> Create(IEnumerable<IntegrationPoint> objs)
		{
			foreach (var integrationPoint in objs)
			{
				integrationPoint.SecuredConfiguration = _encryptionManager.Encrypt(integrationPoint.SecuredConfiguration);
			}
			return base.Create(objs);
		}

		public override List<IntegrationPoint> Read(IEnumerable<int> artifactIds)
		{
			var result = base.Read(artifactIds);
			result.ForEach(x => x.SecuredConfiguration = _encryptionManager.Decrypt(x.SecuredConfiguration));
			return result;
		}

		public override bool Update(IEnumerable<IntegrationPoint> objs)
		{
			foreach (var integrationPoint in objs)
			{
				integrationPoint.SecuredConfiguration = _encryptionManager.Encrypt(integrationPoint.SecuredConfiguration);
			}
			return base.Update(objs);
		}

		public override List<IntegrationPoint> Query(Query<RDO> q, int pageSize = 0)
		{
			var result = base.Query(q, pageSize);
			result.ForEach(x => x.SecuredConfiguration = _encryptionManager.Decrypt(x.SecuredConfiguration));
			return result;
		}
	}
}