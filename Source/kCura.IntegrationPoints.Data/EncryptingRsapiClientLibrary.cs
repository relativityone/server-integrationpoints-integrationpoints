using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Security;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public class EncryptingRsapiClientLibrary : RsapiClientLibrary<IntegrationPoint>
	{
		private readonly IEncryptionManager _encryptionManager;

		public EncryptingRsapiClientLibrary(IHelper helper, int workspaceArtifactId, IEncryptionManager encryptionManager) : base(helper, workspaceArtifactId)
		{
			_encryptionManager = encryptionManager;
		}

		public override List<int> Create(IEnumerable<IntegrationPoint> objs)
		{
			foreach (var integrationPoint in objs)
			{
				IgnoreMissingSecuredConfiguration(() => EncryptSecuredConfiguration(integrationPoint));
			}
			return base.Create(objs);
		}

		public override List<IntegrationPoint> Read(IEnumerable<int> artifactIds)
		{
			var result = base.Read(artifactIds);
			result.ForEach(DecryptSecuredConfiguration);
			return result;
		}

		public override bool Update(IEnumerable<IntegrationPoint> objs)
		{
			foreach (var integrationPoint in objs)
			{
				IgnoreMissingSecuredConfiguration(() => EncryptSecuredConfiguration(integrationPoint));
			}
			return base.Update(objs);
		}

		public override List<IntegrationPoint> Query(Query<RDO> q, int pageSize = 0)
		{
			var result = base.Query(q, pageSize);
			result.ForEach(x => IgnoreMissingSecuredConfiguration(() => DecryptSecuredConfiguration(x)));
			return result;
		}

		private void IgnoreMissingSecuredConfiguration(Action action)
		{
			try
			{
				action();
			}
			catch (FieldNotFoundException)
			{
				//Ignore as Integration Point RDO doesn't always include SecuredConfiguration
				//Any access to missing field will throw FieldNotFoundException
			}
		}

		private void EncryptSecuredConfiguration(IntegrationPoint rdo)
		{
			rdo.SecuredConfiguration = _encryptionManager.Encrypt(rdo.SecuredConfiguration);
		}

		private void DecryptSecuredConfiguration(IntegrationPoint rdo)
		{
			rdo.SecuredConfiguration = _encryptionManager.Decrypt(rdo.SecuredConfiguration);
		}
	}
}