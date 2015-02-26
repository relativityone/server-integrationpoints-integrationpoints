using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data
{
	public class GetArtifactForMassAction
	{
		public GetArtifactForMassAction()
		{
			
		}
		public List<Int32> GetArtifactsToBeDeleted(IWorkspaceDBContext workspaceContext, String tempTableName)
		{
			//create a sql statement which will select the list of ArtifactIDs from the TempTableNameWithParentArtifactsToDelete scratch table
			//Note: we are linking to the EDDSResource database that is on the same database server as the workspace this action is being performed on
			string selectSQL = string.Format("SELECT [ArtifactID] FROM [EDDSResource]..[{0}]", tempTableName);
			//get the artifact ids from the table and convert to a generic list of Int32
			return workspaceContext.ExecuteSqlStatementAsDataTable(selectSQL).Rows.Cast<System.Data.DataRow>().Select(dr => Convert.ToInt32(dr["ArtifactID"])).ToList();
		}
	}
}
