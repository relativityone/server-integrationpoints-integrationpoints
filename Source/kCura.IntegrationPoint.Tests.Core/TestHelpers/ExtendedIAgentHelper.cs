using System;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class ExtendedIAgentHelper : IAgentHelper
	{
		private readonly ITestHelper _helper;
		private readonly ILogFactory _logFactory;

		public ExtendedIAgentHelper(ITestHelper helper)
		{
			_helper = helper;
			_logFactory = NSubstitute.Substitute.For<ILogFactory>();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public IDBContext GetDBContext(int caseId)
		{
			return _helper.GetDBContext(caseId);
		}

		public IServicesMgr GetServicesManager()
		{
			return _helper.GetServicesManager();
		}

		public IUrlHelper GetUrlHelper()
		{
			throw new NotImplementedException();
		}

		public ILogFactory GetLoggerFactory()
		{
			return _logFactory;
		}

		public string ResourceDBPrepend()
		{
			throw new NotImplementedException();
		}

		public string ResourceDBPrepend(IDBContext context)
		{
			throw new NotImplementedException();
		}

		public string GetSchemalessResourceDataBasePrepend(IDBContext context)
		{
			throw new NotImplementedException();
		}

		public Guid GetGuid(int workspaceId, int artifactId)
		{
			throw new NotImplementedException();
		}

		public ISecretStore GetSecretStore()
		{
			throw new NotImplementedException();
		}

		public IInstanceSettingsBundle GetInstanceSettingBundle()
		{
			throw new NotImplementedException();
		}

		public IAuthenticationMgr GetAuthenticationManager()
		{
			throw new NotImplementedException();
		}
	}
}
