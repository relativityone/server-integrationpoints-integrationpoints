using System;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using NSubstitute;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.InstanceSetting;
using Relativity.Services.ObjectQuery;
using Relativity.Services.Objects;
using Relativity.Services.Permission;
using Relativity.Services.ResourceServer;
using Relativity.Services.Search;
using Relativity.Services.Security;
using Relativity.Services.ServiceProxy;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	using System.Net;
	using WinEDDS.Service.Export;
	using IFieldManager = global::Relativity.Services.FieldManager.IFieldManager;

	public class TestHelper : ITestHelper
	{
		private string _relativityUserName;
		private string _relativityPassword;

		private readonly IServicesMgr _serviceManager;
		private readonly ILogFactory _logFactory;
		private readonly IInstanceSettingsBundle _instanceSettingsBundleMock;
		
		public string RelativityUserName {
			get { return _relativityUserName ?? SharedVariables.RelativityUserName; }
			set { _relativityUserName = value; }
		}

		public string RelativityPassword
		{
			get { return _relativityPassword ?? SharedVariables.RelativityPassword; }
			set { _relativityPassword = value; }
		}

		public IPermissionRepository PermissionManager { get; }

		public TestHelper()
		{
			PermissionManager = Substitute.For<IPermissionRepository>();
			_serviceManager = Substitute.For<IServicesMgr>();
			_logFactory = Substitute.For<ILogFactory>();
			_instanceSettingsBundleMock = Substitute.For<IInstanceSettingsBundle>();
			_serviceManager.CreateProxy<IRSAPIClient>(Arg.Any<ExecutionIdentity>()).Returns(new ExtendedIRSAPIClient());
			_serviceManager.CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser).Returns(new ExtendedIPermissionManager(this, ExecutionIdentity.CurrentUser));
			_serviceManager.CreateProxy<IPermissionManager>(ExecutionIdentity.System).Returns(new ExtendedIPermissionManager(this, ExecutionIdentity.System));
			_serviceManager.CreateProxy<IObjectQueryManager>(ExecutionIdentity.System).Returns(new ExtendedIObjectQueryManager(this, ExecutionIdentity.System));
			_serviceManager.CreateProxy<IObjectQueryManager>(ExecutionIdentity.CurrentUser).Returns(new ExtendedIObjectQueryManager(this, ExecutionIdentity.CurrentUser));
			_serviceManager.CreateProxy<IKeywordSearchManager>(ExecutionIdentity.System).Returns(new ExtendedIKeywordSearchManager(this, ExecutionIdentity.System));
			_serviceManager.CreateProxy<IKeywordSearchManager>(ExecutionIdentity.CurrentUser).Returns(new ExtendedIKeywordSearchManager(this, ExecutionIdentity.CurrentUser));
			_serviceManager.CreateProxy<IWorkspaceManager>(ExecutionIdentity.CurrentUser).Returns(new ExtendedIWorkspaceManager(this, ExecutionIdentity.CurrentUser));
			_serviceManager.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System).Returns(new ExtendedIArtifactGuidManager(this, ExecutionIdentity.System));
			_serviceManager.CreateProxy<IFieldManager>(ExecutionIdentity.System).Returns(new ExtendedIFieldManager(this, ExecutionIdentity.System));
			_serviceManager.CreateProxy<IInstanceSettingManager>(ExecutionIdentity.CurrentUser).Returns(new ExtendedInstanceSettingManager(this, ExecutionIdentity.CurrentUser));
		    _serviceManager.CreateProxy<IOAuth2ClientManager>(ExecutionIdentity.System).Returns(_ => CreateAdminProxy<IOAuth2ClientManager>());
			_serviceManager.CreateProxy<IObjectManager>(ExecutionIdentity.System).Returns(_ => CreateAdminProxy<IObjectManager>());
			_serviceManager.CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser).Returns(_ => CreateUserProxy<IObjectManager>());
			_serviceManager.CreateProxy<IResourceServerManager>(ExecutionIdentity.CurrentUser).Returns(_ => CreateUserProxy<IResourceServerManager>());
			_serviceManager.CreateProxy<IResourceServerManager>(ExecutionIdentity.System).Returns(_ => CreateAdminProxy<IResourceServerManager>());
			_serviceManager.GetServicesURL().Returns(SharedVariables.RelativityRestUri);
		}

		public T CreateUserProxy<T>() where T : IDisposable
		{
			return CreateUserProxy<T>(RelativityUserName);
		}

		public T CreateAdminProxy<T>() where T : IDisposable
		{
			var credential = new global::Relativity.Services.ServiceProxy.UsernamePasswordCredentials(RelativityUserName, RelativityPassword);
			ServiceFactorySettings settings = new ServiceFactorySettings(SharedVariables.RsapiUri, SharedVariables.RelativityRestUri, credential);
			ServiceFactory adminServiceFactory = new ServiceFactory(settings);
			return adminServiceFactory.CreateProxy<T>();
		}

		public T CreateUserProxy<T>(string username) where T : IDisposable
		{
			var userCredential = new global::Relativity.Services.ServiceProxy.UsernamePasswordCredentials(username, RelativityPassword);
			ServiceFactorySettings userSettings = new ServiceFactorySettings(SharedVariables.RsapiUri, SharedVariables.RelativityRestUri, userCredential);
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
			TestDbContext context = new TestDbContext(baseContext);
			return context;
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