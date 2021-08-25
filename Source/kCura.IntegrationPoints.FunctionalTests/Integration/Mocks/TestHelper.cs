using System;
using Moq;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.InstanceSetting;
using Relativity.Services.Interfaces.Group;
using Relativity.Services.Objects;
using Relativity.Services.Permission;
using Relativity.Services.Workspace;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class TestHelper : IServiceHelper, IAgentHelper, ICPHelper
	{
		private readonly Mock<IServicesMgr> _serviceManager;

		public FakeSecretStore SecretStore { get; }

		public TestHelper(ProxyMock proxy)
		{
			SecretStore = new FakeSecretStore();

			_serviceManager = new Mock<IServicesMgr>();

			RegisterProxyInServiceManagerMock<IObjectManager>(proxy.ObjectManager.Object);
			RegisterProxyInServiceManagerMock<IWorkspaceManager>(proxy.WorkspaceManager.Object);
			RegisterProxyInServiceManagerMock<IPermissionManager>(proxy.PermissionManager.Object);
			RegisterProxyInServiceManagerMock<IInstanceSettingManager>(proxy.InstanceSettingManager.Object);
			RegisterProxyInServiceManagerMock<IGroupManager>(proxy.GroupManager.Object);
			RegisterProxyInServiceManagerMock<IArtifactGuidManager>(proxy.ArtifactGuidManager.Object);
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
			loggerFactory.Setup(x => x.GetLogger()).Returns(new ConsoleLogger());

			return loggerFactory.Object;
		}

		public IServicesMgr GetServicesManager()
		{
			return _serviceManager.Object;
		}

		public ISecretStore GetSecretStore()
		{
			return SecretStore;
		}

		public void Dispose()
		{
			SecretStore.Clear();
		}
        #region Not Implemented

		public IDBContext GetDBContext(int caseID)
		{
			return new Mock<IDBContext>().Object;
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

		public ICSRFManager GetCSRFManager()
		{
			throw new NotImplementedException();
		}

		public int GetActiveCaseID()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
