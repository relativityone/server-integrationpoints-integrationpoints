using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data
{
    public class SqlObjectTypeQuery : IObjectTypeQuery
    {
        private readonly IWorkspaceDBContext _context;

        public SqlObjectTypeQuery(IWorkspaceDBContext context)
        {
            _context = context;
        }

        public List<ObjectTypeDTO> GetAllTypes(int userId)
        {
            string sql = Resources.Resource.GetObjectTypes;
            SqlParameter param = new SqlParameter("@userID", userId);
            DataTable result = _context.ExecuteSqlStatementAsDataTable(sql, new List<SqlParameter> { param });

            if (result != null && result.Rows != null)
            {
                return result.Rows.Cast<DataRow>().Select(x => new ObjectTypeDTO
                {
                    ArtifactId = x.Field<int>("ArtifactID"),
                    Name = x.Field<string>("Name"),
                    DescriptorArtifactTypeId = x.Field<int>("DescriptorArtifactTypeID")
                }).ToList();
            }

            return new List<ObjectTypeDTO>();
        }
    }
}
