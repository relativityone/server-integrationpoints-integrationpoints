using System;
using kCura.Data.RowDataGateway;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class TestHelper : IHelper
	{
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
			throw new NotImplementedException();
		}

		public IUrlHelper GetUrlHelper()
		{
			throw new NotImplementedException();
		}
	}
}