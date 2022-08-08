namespace kCura.IntegrationPoints.Data.Migrations
{
    public class AddWebApiConfigValueMigration : IMigration
    {
        private readonly IEddsDBContext _context;
        public AddWebApiConfigValueMigration(IEddsDBContext context)
        {
            _context = context;
        }

        public void Execute()
        {
            var sql = Resources.Resource.AddWebApiConfig;
            _context.ExecuteNonQuerySQLStatement(sql);
        }
    }
}
