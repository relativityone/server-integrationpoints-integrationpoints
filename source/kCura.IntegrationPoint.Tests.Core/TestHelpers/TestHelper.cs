using System;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using NSubstitute;
using Relativity.API;
using Relativity.Services.ObjectQuery;
using Relativity.Services.Permission;
using Relativity.Services.Search;
using Relativity.Services.ServiceProxy;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class TestHelper : ITestHelper
	{
		private readonly IServicesMgr _serviceManager;
		private readonly ILogFactory _logFactory;

		public string RelativityUserName { get; set; } = SharedVariables.RelativityUserName;
		public string RelativityPassword { get; set; } = SharedVariables.RelativityPassword;

		public IPermissionRepository PermissionManager { get; }

		public TestHelper()
		{
			PermissionManager = Substitute.For<IPermissionRepository>();
			_serviceManager = Substitute.For<IServicesMgr>();
			_logFactory = Substitute.For<ILogFactory>();
			_serviceManager.CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser).Returns(new ExtendedIRSAPIClient(this, ExecutionIdentity.CurrentUser));
			_serviceManager.CreateProxy<IRSAPIClient>(ExecutionIdentity.System).Returns(new ExtendedIRSAPIClient(this, ExecutionIdentity.System));
			_serviceManager.CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser).Returns(new ExtendedIPermissionManager(this, ExecutionIdentity.CurrentUser));
			_serviceManager.CreateProxy<IPermissionManager>(ExecutionIdentity.System).Returns(new ExtendedIPermissionManager(this, ExecutionIdentity.System));
			_serviceManager.CreateProxy<IObjectQueryManager>(ExecutionIdentity.System).Returns(new ExtendedIObjectQueryManager(this, ExecutionIdentity.System));
			_serviceManager.CreateProxy<IObjectQueryManager>(ExecutionIdentity.CurrentUser).Returns(new ExtendedIObjectQueryManager(this, ExecutionIdentity.CurrentUser));
			_serviceManager.CreateProxy<IKeywordSearchManager>(ExecutionIdentity.System).Returns(new ExtendedIKeywordSearchManager(this, ExecutionIdentity.System));
			_serviceManager.CreateProxy<IKeywordSearchManager>(ExecutionIdentity.CurrentUser).Returns(new ExtendedIKeywordSearchManager(this, ExecutionIdentity.CurrentUser));
		}

		public T CreateUserProxy<T>() where T : IDisposable
		{
			var userCredential = new global::Relativity.Services.ServiceProxy.UsernamePasswordCredentials(RelativityUserName, RelativityPassword);
			ServiceFactorySettings userSettings = new ServiceFactorySettings(SharedVariables.RsapiClientServiceUri, SharedVariables.RestClientServiceUri, userCredential);
			ServiceFactory userServiceFactory = new ServiceFactory(userSettings);
			return userServiceFactory.CreateProxy<T>();
		}

		public T CreateAdminProxy<T>() where T : IDisposable
		{
			var credential = new global::Relativity.Services.ServiceProxy.UsernamePasswordCredentials("relativity.admin@kcura.com", "Test1234!");
			ServiceFactorySettings settings = new ServiceFactorySettings(SharedVariables.RsapiClientServiceUri, SharedVariables.RestClientServiceUri, credential);
			ServiceFactory adminServiceFactory = new ServiceFactory(settings);
			return adminServiceFactory.CreateProxy<T>();
		}

		public void Dispose()
		{
		}

		public IDBContext GetDBContext(int caseId)
		{
			Data.RowDataGateway.Context baseContext;
			if (caseId == -1)
			{
				baseContext = new Data.RowDataGateway.Context(SharedVariables.EddsConnectionString);
			}
			else
			{
				string connectionString = String.Format(SharedVariables.WorkspaceConnectionStringFormat, caseId);
				baseContext = new Data.RowDataGateway.Context(connectionString);
			}
			DBContext context = new DBContext(baseContext);
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

		public Guid GetGuid(int workspaceId, int artifactId)
		{
			throw new NotImplementedException();
		}

		public IServicesMgr GetServicesManager()
		{
			return _serviceManager;
		}
	}
}