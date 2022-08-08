using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace kCura.IntegrationPoints.Data
{
    public interface IRipDBContext
    {
        string ServerName { get; }
        void BeginTransaction();
        void CommitTransaction();
        int ExecuteNonQuerySQLStatement(string sqlStatement);
        int ExecuteNonQuerySQLStatement(string sqlStatement, IEnumerable<SqlParameter> parameters);
        DataTable ExecuteSqlStatementAsDataTable(string sqlStatement);
        DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, IEnumerable<SqlParameter> parameters);
        T ExecuteSqlStatementAsScalar<T>(string sqlStatement, IEnumerable<SqlParameter> parameters);
        object ExecuteSqlStatementAsScalar(string sqlStatement, params SqlParameter[] parameters);
        IDataReader ExecuteSQLStatementAsReader(string sql);
    }
}
