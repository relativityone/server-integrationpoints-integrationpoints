using System;
using System.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoint.Tests.Core
{
    public static class AgentTypeInformationHelper
    {
        public static DataTable CreateAgentTypeInformationDataTable()
        {
            var table = new DataTable();
            table.Columns.Add("AgentTypeID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Fullnamespace", typeof(string));
            table.Columns.Add("Guid", typeof(Guid));
            return table;
        }

        public static DataRow CreateAgentTypeInformationDataRow(int agentTypeId, string name,
            string fullNamespace, Guid guid, DataTable table = null)
        {
            table = table ?? CreateAgentTypeInformationDataTable();

            DataRow row = table.NewRow();
            row["AgentTypeId"] = agentTypeId;
            row["Name"] = name;
            row["Fullnamespace"] = fullNamespace;
            row["Guid"] = guid;
            return row;
        }

        public static AgentTypeInformation CreateAgentTypeInformation(int agentTypeId, string name,
            string fullNamespace, Guid guid)
        {
            return new AgentTypeInformation(CreateAgentTypeInformationDataRow(agentTypeId, name, fullNamespace, guid));
        }
    }
}
