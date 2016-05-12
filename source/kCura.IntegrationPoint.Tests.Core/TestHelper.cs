using System;
using kCura.Data.RowDataGateway;
using kCura.Relativity.Client;
using NSubstitute;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class TestHelper : IHelper
	{
		private readonly IServicesMgr _serviceManager;

		public TestHelper(Helper helper)
		{
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
	}
}