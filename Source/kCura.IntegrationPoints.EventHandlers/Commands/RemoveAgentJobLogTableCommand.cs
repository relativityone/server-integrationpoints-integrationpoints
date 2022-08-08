using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class RemoveAgentJobLogTableCommand : IEHCommand
    {
        private readonly IEHContext _context;

        public RemoveAgentJobLogTableCommand(IEHContext context)
        {
            _context = context;
        }

        public void Execute()
        {
            IDBContext context = _context.Helper.GetDBContext(-1);
            string tableName = $"AgentJobLog_{GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID}";
            string query = $"IF EXISTS (SELECT * FROM EDDS.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{tableName}') DROP TABLE [EDDSDBO].[{tableName}]";
            context.ExecuteNonQuerySQLStatement(query);
        }
    }
}
