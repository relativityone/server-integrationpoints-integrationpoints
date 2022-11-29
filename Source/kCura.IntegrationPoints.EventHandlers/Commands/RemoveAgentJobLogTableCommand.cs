using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

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
            IEddsDBContext context = new DbContextFactory(_context.Helper).CreatedEDDSDbContext();
            string tableName = $"AgentJobLog_{GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID}";
            string query = $"IF EXISTS (SELECT * FROM EDDS.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{tableName}') DROP TABLE [EDDSDBO].[{tableName}]";
            context.ExecuteNonQuerySQLStatement(query);
        }
    }
}
