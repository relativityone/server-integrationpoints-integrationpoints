using System;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class ExtendedIAgentHelper : IAgentHelper
	{
		private readonly ITestHelper _helper;

		public ExtendedIAgentHelper(ITestHelper helper)
		{
			_helper = helper;
		}

		public void Dispose()
		{
			throw new NotImplementedException();
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

		public Guid GetGuid(int workspaceId, int artifactId)
		{
			throw new NotImplementedException();
		}

		public IAuthenticationMgr GetAuthenticationManager()
		{
			throw new NotImplementedException();
		}
	}
}
