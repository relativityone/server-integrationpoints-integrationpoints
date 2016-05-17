using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using NSubstitute;
using Relativity.API;
using Relativity.Services.ObjectQuery;
using Relativity.Services.ServiceProxy;
using Context = kCura.Data.RowDataGateway.Context;
using UsernamePasswordCredentials = Relativity.Services.ServiceProxy.UsernamePasswordCredentials;

namespace kCura.IntegrationPoint.Tests.Core
{
	public interface ITestHelper : IHelper
	{
		IPermissionRepository PermissionManager { get; }

		IObjectQueryManager CreateUserObjectQueryManager();

		T CreateUserProxy<T>() where T : IDisposable;
	}

	public class TestHelper : ITestHelper
	{
		private readonly IServicesMgr _serviceManager;
		public IPermissionRepository PermissionManager { get; }

		public TestHelper()
		{
			PermissionManager = Substitute.For<IPermissionRepository>();
			_serviceManager = Substitute.For<IServicesMgr>();
			_serviceManager.CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser).Returns(Rsapi.CreateRsapiClient(ExecutionIdentity.CurrentUser));
			_serviceManager.CreateProxy<IRSAPIClient>(ExecutionIdentity.System).Returns(Rsapi.CreateRsapiClient(ExecutionIdentity.System));

			//_serviceManager.CreateProxy<IObjectQueryManager>(ExecutionIdentity.System).Returns(CreateAdminObjectQueryManager(), CreateAdminObjectQueryManager());
			//_serviceManager.CreateProxy<IObjectQueryManager>(ExecutionIdentity.CurrentUser).Returns(CreateUserObjectQueryManager(), CreateUserObjectQueryManager());
		}

		public IObjectQueryManager CreateUserObjectQueryManager()
		{
			return CreateUserProxy<IObjectQueryManager>();
		}

		public T CreateUserProxy<T>() where T : IDisposable
		{
			var userCredential = new UsernamePasswordCredentials(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
			ServiceFactorySettings userSettings = new ServiceFactorySettings(SharedVariables.RsapiClientServiceUri, SharedVariables.RestClientServiceUri, userCredential);
			ServiceFactory userServiceFactory = new ServiceFactory(userSettings);
			return userServiceFactory.CreateProxy<T>();
		}

		public IObjectQueryManager CreateAdminObjectQueryManager()
		{
			var credential = new UsernamePasswordCredentials("relativity.admin@kcura.com", "P@ssw0rd@1");
			ServiceFactorySettings settings = new ServiceFactorySettings(SharedVariables.RsapiClientServiceUri, SharedVariables.RestClientServiceUri, credential);
			ServiceFactory adminServiceFactory = new ServiceFactory(settings);
			return adminServiceFactory.CreateProxy<IObjectQueryManager>();
		}

		public void Dispose()
		{
		}

		public IDBContext GetDBContext(int caseId)
		{
			Context baseContext = null;
			if (caseId == -1)
			{
				baseContext = new Context(SharedVariables.EddsConnectionString);
			}
			else
			{

				string connectionString = String.Format(SharedVariables.WorkspaceConnectionStringFormat, caseId);
				baseContext = new Context(connectionString);
			}
			DBContext context = new DBContext(baseContext);
			return context;
		}

		public ILogFactory GetLoggerFactory()
		{
			throw new NotImplementedException();
		}

		public IServicesMgr GetServicesManager()
		{
			return _serviceManager;
		}

		public IUrlHelper GetUrlHelper()
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
	}
}