using System;
using Moq;
using Relativity.API;
using Relativity.Services.Objects;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class TestHelper : IHelper, IAgentHelper
	{
		private readonly Mock<IServicesMgr> _serviceManager;

		public TestHelper(ProxyMock proxy)
		{
			_serviceManager = new Mock<IServicesMgr>();

			RegisterProxyInServiceManagerMock<IObjectManager>(proxy.ObjectManager.Object);
		}

		private void RegisterProxyInServiceManagerMock<T>(T proxy) 
			where T : IDisposable
		{
			_serviceManager.Setup(x => x.CreateProxy<T>(It.IsAny<ExecutionIdentity>()))
				.Returns(proxy);
		}

		public ILogFactory GetLoggerFactory()
		{
			var loggerFactory = new Mock<ILogFactory>();
			loggerFactory.Setup(x => x.GetLogger()).Returns(new EmptyLogger());

			return loggerFactory.Object;
		}

		public IServicesMgr GetServicesManager()
		{
			return _serviceManager.Object;
		}

		#region Not Implemented

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public IDBContext GetDBContext(int caseID)
		{
			throw new NotImplementedException();
		}

		public IUrlHelper GetUrlHelper()
		{
			throw new NotImplementedException();
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

		public Guid GetGuid(int workspaceID, int artifactID)
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

		public IStringSanitizer GetStringSanitizer(int workspaceID)
		{
			throw new NotImplementedException();
		}

		public IAuthenticationMgr GetAuthenticationManager()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
