using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Domain;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class GetApplicationBinaries
    {
        private readonly IDBContext _eddsDBcontext;

        public GetApplicationBinaries(IDBContext eddsDBcontext)
        {
            _eddsDBcontext = eddsDBcontext;
        }

        public List<ApplicationBinary> Execute(Guid applicationGuid)
        {
            List<ApplicationBinary> binaries = new List<ApplicationBinary>();
            var sql = Resources.Resource.GetApplicationBinaries;
            var sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@ApplicationGUID", applicationGuid));

            DataTable dataTable = _eddsDBcontext.ExecuteSqlStatementAsDataTable(sql, sqlParams.ToArray());
            if (dataTable != null && dataTable.Rows != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    ApplicationBinary appBin = new ApplicationBinary()
                    {
                        ArtifactID = row.Field<int>("ArtifactID"),
                        Name = row.Field<string>("Name"),
                        FileData = row.Field<byte[]>("FileData")
                    };
                    binaries.Add(appBin);
                }
            }
            return binaries;
        }
    }

}
