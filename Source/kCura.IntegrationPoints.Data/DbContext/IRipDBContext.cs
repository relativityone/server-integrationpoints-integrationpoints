using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace kCura.IntegrationPoints.Data.DbContext
{
    public interface IRipDBContext
    {
        string ServerName { get; }

        SqlConnection GetConnection();

        void BeginTransaction();

        void CommitTransaction();

        void RollbackTransaction();

        int ExecuteNonQuerySQLStatement(string sqlStatement);

        int ExecuteNonQuerySQLStatement(string sqlStatement, IEnumerable<SqlParameter> parameters);

        DataTable ExecuteSqlStatementAsDataTable(string sqlStatement);

        DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, IEnumerable<SqlParameter> parameters);

        DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement);

        T ExecuteSqlStatementAsScalar<T>(string sqlStatement, IEnumerable<SqlParameter> parameters);

        T ExecuteSqlStatementAsScalar<T>(string sqlStatement);

        object ExecuteSqlStatementAsScalar(string sqlStatement, params SqlParameter[] parameters);

        IDataReader ExecuteSQLStatementAsReader(string sql);

        SqlDataReader ExecuteSQLStatementAsReader(string sqlStatement, int timeout = -1);
    }
}
