using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Extensions
{
    public static class DBContextExtensions
    {
        public static int GetArtifactIDByGuid(this IDBContext context, Guid artifactGuid)
        {
            var sql = Resources.Resource.GetArtifactIDByGuid;
            var sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@ArtifactGuid", artifactGuid.ToString()));
            return context.ExecuteSqlStatementAsScalar<int>(sql, sqlParams);
        }
    }
}
