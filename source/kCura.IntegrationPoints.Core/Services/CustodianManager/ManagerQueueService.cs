using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Queries;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.CustodianManager
{
    public class ManagerQueueService
    {
        private ICaseServiceContext _caseServiceContext;
        private IEddsServiceContext _eddsServiceContext;
        public ManagerQueueService(ICaseServiceContext caseServiceContext, IEddsServiceContext eddsServiceContext)
        {
            _caseServiceContext = caseServiceContext;
            _eddsServiceContext = eddsServiceContext;
        }

        public string GetTempTableName(Job job, Guid batchInstance)
        {
            return this.GetTempTableName(job.WorkspaceID, job.RelatedObjectArtifactID, batchInstance);
        }
        public string GetTempTableName(int workspaceID, int relatedObjectArtifactID, Guid batchInstance)
        {
            return string.Format("RIP_CustodianManager_{0}_{1}_{2}", workspaceID.ToString("D7"), relatedObjectArtifactID.ToString("D7"), batchInstance.ToString());
        }

        public List<CustodianManagerMap> GetCustodianManagerLinksToProcess(Job job, Guid batchInstance, List<CustodianManagerMap> jobCustodianManagerMap)
        {
            //Create temp table if does not exist and delete old links
            string tableName = GetTempTableName(job, batchInstance);
            new CreateCustodianManagerResourceTable(_caseServiceContext.SqlContext).Execute(tableName);

            //insert job Custodian Manager links
            InsertData(tableName, jobCustodianManagerMap);

            //Get links to process
            List<CustodianManagerMap> newLinkMap = GetLinksToProcess(tableName, job);

            return newLinkMap;
        }

        private List<CustodianManagerMap> GetLinksToProcess(string tableName, Job job)
        {
            List<CustodianManagerMap> newLinkMap = new List<CustodianManagerMap>();
            DataTable dt = new GetJobCustodianManagerLinks(_caseServiceContext.SqlContext).Execute(tableName, job.JobId);
            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    newLinkMap.Add(new CustodianManagerMap()
                    {
                        CustodianID = row.Field<string>("CustodianID"),
                        OldManagerID = row.Field<string>("ManagerID")
                    });
                }
            }
            return newLinkMap;
        }

        public bool AreAllTasksOfTheBatchDone(Job job, string[] taskTypeExceptions)
        {
            bool result = true;
            DataTable dt = new GetJobsCount(_eddsServiceContext.SqlContext).Execute(job.RootJobId ?? job.JobId, taskTypeExceptions);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                int count = int.Parse(dt.Rows[0][0].ToString());
                //do not count last final job
                result = count < 2;
            }
            return result;
        }

        private void InsertData(string tableName, List<CustodianManagerMap> jobCustodianManagerMap)
        {
            DataTable dtInsertRows = GetDataTable(jobCustodianManagerMap);
            using (SqlBulkCopy sbc = new SqlBulkCopy(_caseServiceContext.SqlContext.GetConnection()))
            {
                sbc.DestinationTableName = string.Format("[EDDSResource].[eddsdbo].[{0}]", tableName);

                // Map the Source Column from DataTabel to the Destination Columns
                sbc.ColumnMappings.Add("CustodianID", "CustodianID");
                sbc.ColumnMappings.Add("ManagerID", "ManagerID");
                sbc.ColumnMappings.Add("CreatedOn", "CreatedOn");

                // Finally write to server
                sbc.WriteToServer(dtInsertRows);
                sbc.Close();
            }
        }

        private DataTable GetDataTable(List<CustodianManagerMap> jobCustodianManagerMap)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("CustodianID", typeof(System.String)));
            dataTable.Columns.Add(new DataColumn("ManagerID", typeof(System.String)));
            dataTable.Columns.Add(new DataColumn("CreatedOn", typeof(System.DateTime)));

            foreach (var map in jobCustodianManagerMap)
            {
                DataRow row = dataTable.NewRow();
                row["CustodianID"] = map.CustodianID;
                row["ManagerID"] = map.OldManagerID;
                row["CreatedOn"] = DateTime.UtcNow;
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }
    }
}
