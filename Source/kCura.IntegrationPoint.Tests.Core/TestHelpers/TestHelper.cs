using System;
using kCura.WinEDDS.Service.Export;
using NSubstitute;
using Relativity.API;
using Relativity.Data;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Folder;
using Relativity.Services.InstanceSetting;
using Relativity.Services.Objects;
using Relativity.Services.Permission;
using Relativity.Services.ResourceServer;
using Relativity.Services.Search;
using Relativity.Services.Security;
using Relativity.Services.ServiceProxy;
using Relativity.Services.Workspace;
using Relativity.Services.Interfaces.Group;
using ARMTestServices.Services.Interfaces;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.Tab;
using Relativity.Services.InternalMetricsCollection;
using Relativity.Services.View;
using Relativity.Services.ChoiceQuery;
using Relativity.Services.Interfaces.UserInfo;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	using System.Net;
	using IFieldManager = global::Relativity.Services.FieldManager.IFieldManager;

	public class TestHelper : ITestHelper
	{
		private readonly IServicesMgr _serviceManager;
		private readonly ILogFactory _logFactory;
		private readonly IInstanceSettingsBundle _instanceSettingsBundleMock;

		public string RelativityUserName { get; }

		public string RelativityPassword { get; }
		
		public TestHelper()
			: this(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword)
		{ }

		public TestHelper(string userName, string password)
		{
			RelativityUserName = userName;
			RelativityPassword = password;

			_logFactory = Substitute.For<ILogFactory>();
			_instanceSettingsBundleMock = Substitute.For<IInstanceSettingsBundle>();
			_serviceManager = Substitute.For<IServicesMgr>();
			RegisterProxyInServiceManagerMock<IPermissionManager>(ExecutionIdentity.CurrentUser);
			RegisterProxyInServiceManagerMock<IPermissionManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<IKeywordSearchManager>(ExecutionIdentity.CurrentUser);
			RegisterProxyInServiceManagerMock<IKeywordSearchManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<IObjectManager>(ExecutionIdentity.CurrentUser);
			RegisterProxyInServiceManagerMock<IObjectManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<IObjectTypeManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<IResourceServerManager>(ExecutionIdentity.CurrentUser);
			RegisterProxyInServiceManagerMock<IResourceServerManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<IWorkspaceManager>(ExecutionIdentity.CurrentUser);
			RegisterProxyInServiceManagerMock<IArtifactGuidManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<IFieldManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<IFieldManager>(ExecutionIdentity.CurrentUser);
			RegisterProxyInServiceManagerMock<IInstanceSettingManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<ISearchContainerManager>(ExecutionIdentity.CurrentUser);
			RegisterProxyInServiceManagerMock<IOAuth2ClientManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<IFolderManager>(ExecutionIdentity.CurrentUser);
			RegisterProxyInServiceManagerMock<global:: Relativity.Productions.Services.IProductionManager>(ExecutionIdentity.CurrentUser);
			RegisterProxyInServiceManagerMock<IGroupManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<IGroupManager>(ExecutionIdentity.CurrentUser);
			RegisterProxyInServiceManagerMock<IUserInfoManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<IUserInfoManager>(ExecutionIdentity.CurrentUser);
			RegisterProxyInServiceManagerMock<ILoginProfileManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<ILoginProfileManager>(ExecutionIdentity.CurrentUser);
			RegisterProxyInServiceManagerMock<IFileManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<IFileshareManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<IInternalMetricsCollectionManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<global::Relativity.Services.Interfaces.Field.IFieldManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<global::Relativity.Services.Interfaces.Field.IFieldManager>(ExecutionIdentity.CurrentUser);
			RegisterProxyInServiceManagerMock<ITabManager>(ExecutionIdentity.CurrentUser);
			RegisterProxyInServiceManagerMock<ISearchService>(ExecutionIdentity.CurrentUser);
			RegisterProxyInServiceManagerMock<IViewManager>(ExecutionIdentity.System);
			RegisterProxyInServiceManagerMock<IChoiceQueryManager>(ExecutionIdentity.System);
			_serviceManager.GetServicesURL().Returns(SharedVariables.RelativityRestUri);
		}
		
		private void RegisterProxyInServiceManagerMock<T>(ExecutionIdentity executionIdentity) where T : IDisposable
		{
			_serviceManager.CreateProxy<T>(executionIdentity).Returns(_ => CreateProxy<T>());
		}

		public T CreateProxy<T>() where T : IDisposable
		{
			return CreateProxy<T>(RelativityUserName);
		}
		
		public T CreateProxy<T>(string username) where T : IDisposable
		{
			var userCredential = new UsernamePasswordCredentials(username, RelativityPassword);
			ServiceFactorySettings userSettings = new ServiceFactorySettings(SharedVariables.RelativityRestUri, userCredential);
			ServiceFactory userServiceFactory = new ServiceFactory(userSettings);
			return userServiceFactory.CreateProxy<T>();
		}

		public ISearchManager CreateSearchManager()
		{
			ICredentials credentials = new NetworkCredential(RelativityUserName, RelativityPassword);

			return new WinEDDS.Service.SearchManager(credentials, new CookieContainer());
		}

        public void Dispose()
		{
			// empty by design
		}

		public IDBContext GetDBContext(int caseID)
		{
			Data.RowDataGateway.Context baseContext;
			if (caseID == -1)
			{
				baseContext = new Data.RowDataGateway.Context(SharedVariables.EddsConnectionString);
			}
			else
			{
				string connectionString = string.Format(SharedVariables.WorkspaceConnectionStringFormat, caseID);
				baseContext = new Data.RowDataGateway.Context(connectionString);
			}
			return DBContextMockBuilder.Build(baseContext);
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
			return Helper.GetInstance().GetResourceDataBasePrepend();
		}

		public string ResourceDBPrepend(IDBContext context)
		{
			return ResourceDBPrepend();
		}

		public string GetSchemalessResourceDataBasePrepend(IDBContext context)
		{
			return context.Database;
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
			return _instanceSettingsBundleMock;
		}

		public IStringSanitizer GetStringSanitizer(int workspaceID)
		{
			throw new NotImplementedException();
		}

		public IServicesMgr GetServicesManager()
		{
			return _serviceManager;
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
	}
}
