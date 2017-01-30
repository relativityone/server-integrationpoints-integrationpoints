using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.Data
{
	public class EncryptingRsapiClientLibrary : IGenericLibrary<IntegrationPoint>
	{
		private readonly IGenericLibrary<IntegrationPoint> _integrationPointLibrary;
		private readonly ISecretCatalog _secretCatalog;
		private readonly ISecretManager _secretManager;

		public EncryptingRsapiClientLibrary(IGenericLibrary<IntegrationPoint> integrationPointLibrary, ISecretCatalog secretCatalog, ISecretManager secretManager)
		{
			_integrationPointLibrary = integrationPointLibrary;
			_secretCatalog = secretCatalog;
			_secretManager = secretManager;
		}

		public int Create(IntegrationPoint obj)
		{
			EncryptSecuredConfigurationForNewRdo(obj);
			return _integrationPointLibrary.Create(obj);
		}

		public List<int> Create(IEnumerable<IntegrationPoint> objs)
		{
			EncryptSecuredConfigurationForNewRdos(objs);
			return _integrationPointLibrary.Create(objs);
		}

		public IntegrationPoint Read(int artifactId)
		{
			var result = _integrationPointLibrary.Read(artifactId);
			DecryptSecuredConfiguration(result);
			return result;
		}

		public List<IntegrationPoint> Read(IEnumerable<int> artifactIds)
		{
			var result = _integrationPointLibrary.Read(artifactIds);
			DecryptSecuredConfiguration(result);
			return result;
		}

		public bool Update(IntegrationPoint obj)
		{
			EncryptSecuredConfigurationForExistingRdo(obj);
			return _integrationPointLibrary.Update(obj);
		}

		public bool Update(IEnumerable<IntegrationPoint> objs)
		{
			EncryptSecuredConfigurationForExistingRdos(objs);
			return _integrationPointLibrary.Update(objs);
		}

		public bool Delete(int artifactId)
		{
			return _integrationPointLibrary.Delete(artifactId);
		}

		public bool Delete(IEnumerable<int> artifactIds)
		{
			return _integrationPointLibrary.Delete(artifactIds);
		}

		public bool Delete(IntegrationPoint obj)
		{
			return _integrationPointLibrary.Delete(obj);
		}

		public bool Delete(IEnumerable<IntegrationPoint> objs)
		{
			return _integrationPointLibrary.Delete(objs);
		}

		public void MassDelete(IEnumerable<IntegrationPoint> objs)
		{
			throw new NotImplementedException();
		}

		public MassCreateResult MassCreate(IEnumerable<IntegrationPoint> objs)
		{
			throw new NotImplementedException();
		}

		public MassEditResult MassEdit(IEnumerable<IntegrationPoint> objs)
		{
			throw new NotImplementedException();
		}

		public List<IntegrationPoint> Query(Query<RDO> q, int pageSize = 0)
		{
			var result = _integrationPointLibrary.Query(q, pageSize);
			DecryptSecuredConfiguration(result);
			return result;
		}

		private void EncryptSecuredConfigurationForNewRdos(IEnumerable<IntegrationPoint> rdos)
		{
			foreach (var rdo in rdos)
			{
				EncryptSecuredConfigurationForNewRdo(rdo);
			}
		}

		private void EncryptSecuredConfigurationForExistingRdos(IEnumerable<IntegrationPoint> rdos)
		{
			foreach (var rdo in rdos)
			{
				EncryptSecuredConfigurationForExistingRdo(rdo);
			}
		}

		private void DecryptSecuredConfiguration(IEnumerable<IntegrationPoint> rdos)
		{
			foreach (var rdo in rdos)
			{
				DecryptSecuredConfiguration(rdo);
			}
		}

		private void EncryptSecuredConfigurationForNewRdo(IntegrationPoint rdo)
		{
			IgnoreMissingSecuredConfiguration(() =>
			{
				var secretData = _secretManager.CreateSecretData(rdo);
				var secretIdentifier = _secretManager.GenerateIdentifier();
				_secretCatalog.WriteSecret(secretIdentifier, secretData);
				rdo.SecuredConfiguration = secretIdentifier.SecretID;
			});
		}

		private void EncryptSecuredConfigurationForExistingRdo(IntegrationPoint rdo)
		{
			IgnoreMissingSecuredConfiguration(() =>
			{
				var secretData = _secretManager.CreateSecretData(rdo);
				var existingRdo = _integrationPointLibrary.Read(rdo.ArtifactId);
				var secretIdentifier = _secretManager.RetrieveIdentifier(existingRdo);
				_secretCatalog.WriteSecret(secretIdentifier, secretData);
				rdo.SecuredConfiguration = secretIdentifier.SecretID;
			});
		}

		private void DecryptSecuredConfiguration(IntegrationPoint rdo)
		{
			IgnoreMissingSecuredConfiguration(() =>
			{
				if (string.IsNullOrWhiteSpace(rdo.SecuredConfiguration))
				{
					return;
				}
				var secretIdentifier = _secretManager.RetrieveIdentifier(rdo);
				var secretData = _secretCatalog.GetSecret(secretIdentifier);
				rdo.SecuredConfiguration = _secretManager.RetrieveValue(secretData);
			});
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
	}
}