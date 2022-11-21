using kCura.IntegrationPoints.Data.DbContext;

namespace kCura.IntegrationPoints.Data.Migrations
{
    public class UpdateJobErrorsBlankToNo : IMigration
    {
        private readonly IWorkspaceDBContext _context;

        public UpdateJobErrorsBlankToNo(IWorkspaceDBContext context)
        {
            _context = context;
        }

        public void Execute()
        {
            var sql = Resources.Resource.SetBlankLogErrorsToNo;
            _context.ExecuteSqlStatementAsDataTable(sql);
        }
    }
}
