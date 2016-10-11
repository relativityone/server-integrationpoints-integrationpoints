using System;
using System.Data.SqlClient;
using kCura.Data.RowDataGateway;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Tests.Integration
{
	public class TestDBContextHelper
	{
		public IDBContext GetEDDSDBContext()
		{
			return GetDBContext();
		}

		public IDBContext GetDBContext(int workspaceId = -1)
		{
			string masterConnectionString = kCura.Data.RowDataGateway.Config.ConnectionString;
			IDBContext eddsDBContext = new Relativity.API.DBContext(new Context(masterConnectionString));
			IDBContext returnDBContext = eddsDBContext;
			if (workspaceId > 0)
			{
				dynamic @params = new SqlParameter[] { new SqlParameter("@WorkspaceId", workspaceId) };
				dynamic casedb = eddsDBContext.ExecuteSqlStatementAsScalar(
						"SELECT ResourceServer.Name FROM EDDS.eddsdbo.[Case] INNER JOIN EDDS.eddsdbo.ResourceServer ON ServerID=ResourceServer.ArtifactID WHERE [Case].ArtifactID=@WorkspaceId",
						@params);
				if (casedb is DBNull || casedb == null)
					throw new Exception(string.Format("Workspace ({0}) not found.", workspaceId));
				dynamic csb = new SqlConnectionStringBuilder(masterConnectionString);
				csb.DataSource = (string)casedb;
				csb.InitialCatalog = "EDDS" + workspaceId.ToString();

				string caseConnectionString = csb.ToString();
				returnDBContext = new Relativity.API.DBContext(new Context(caseConnectionString));
			}
			return returnDBContext;
		}
	}
}
