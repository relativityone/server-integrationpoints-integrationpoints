using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Toggles.Providers;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public static class ToggleProviderHelper
    {
        public static SqlServerToggleProvider CreateSqlServerToggleProvider(IHelper helper)
        {
            return new SqlServerToggleProvider(() => ConnectionFactory(helper), () => AsyncConnectionFactory(helper)) { CacheEnabled = true };
        }

        private static Task<SqlConnection> AsyncConnectionFactory(IHelper helper)
        {
            return Task.Run(() => ConnectionFactory(helper));
        }

        private static SqlConnection ConnectionFactory(IHelper helper)
        {
            SqlConnection connection = helper.GetDBContext(-1).GetConnection(true);

            return connection;
        }
    }

    
}
