using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class FakeSecretStore : ISecretStore
	{
		private static Dictionary<string, Dictionary<string, string>> _secrets = new Dictionary<string, Dictionary<string, string>>();

		public void Clear()
		{
			_secrets.Clear();
		}

		public void Setup(WorkspaceTest workspace, IntegrationPointTest integrationPoint)
		{
			string key = $"/{workspace.ArtifactId}/{integrationPoint.ArtifactId}/{integrationPoint.SecuredConfiguration}";
			_secrets[key] = new Dictionary<string, string> { { "SecuredConfiguration", integrationPoint.SecuredConfigurationDecrypted } };
		}

		public void Delete(string path)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(string path)
		{
			throw new NotImplementedException();
		}

		public Secret Get(string path)
		{
			throw new NotImplementedException();
		}

		public Task<Secret> GetAsync(string path)
		{
			Secret secret = new Secret
			{
				Data = _secrets[path]
			};

			return Task.FromResult(secret);
		}

		#region Not Implemented

		public List<string> List(string path)
		{
			throw new NotImplementedException();
		}

		public Task<List<string>> ListAsync(string path)
		{
			throw new NotImplementedException();
		}

		public void Set(string path, Secret secret)
		{
			throw new NotImplementedException();
		}

		public Task SetAsync(string path, Secret secret)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
