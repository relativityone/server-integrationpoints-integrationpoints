using Relativity.API;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class CreateCustodianManagerResourceTable
	{
		private IDBContext _caseDBcontext;

		public CreateCustodianManagerResourceTable(IDBContext caseDBcontext)
		{
			_caseDBcontext = caseDBcontext;
		}

		public void Execute(string tableName)
		{
			var sql = string.Format(Resources.Resource.CreateCustodianManagerResourceTable, tableName);
			_caseDBcontext.ExecuteNonQuerySQLStatement(sql);
		}
	}
}