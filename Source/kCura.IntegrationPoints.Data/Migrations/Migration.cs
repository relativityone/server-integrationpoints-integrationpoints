
namespace kCura.IntegrationPoints.Data.Migrations
{
	public class Migration : IMigration
	{
		private readonly IEddsDBContext _eddsContext;
		private readonly string _sql;

		public Migration(IEddsDBContext eddsContext, string sql)
		{
			_eddsContext = eddsContext;
			_sql = sql;
		}

		public void Execute()
		{
			_eddsContext.ExecuteNonQuerySQLStatement(_sql);
		}
	}
}
