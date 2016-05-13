using System;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using NSubstitute;
using Relativity.API;
using Context = kCura.Data.RowDataGateway.Context;

namespace kCura.IntegrationPoint.Tests.Core
{
	public interface ITestHelper : IHelper
	{
		IPermissionRepository PermissionManager { get; }
	}

	public class TestHelper : ITestHelper
	{
		private readonly IServicesMgr _serviceManager;
		public IPermissionRepository PermissionManager { get; }

		public TestHelper(Helper helper)
		{
			PermissionManager = Substitute.For<IPermissionRepository>();
			_serviceManager = Substitute.For<IServicesMgr>();
			_serviceManager.CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser).Returns(helper.Rsapi.CreateRsapiClient(ExecutionIdentity.CurrentUser));
			_serviceManager.CreateProxy<IRSAPIClient>(ExecutionIdentity.System).Returns(helper.Rsapi.CreateRsapiClient(ExecutionIdentity.System));
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
			context.BeginTransaction();
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