using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public class SqlObjectTypeQuery : IObjectTypeQuery
	{
		private readonly IWorkspaceDBContext _context;
		public SqlObjectTypeQuery(IWorkspaceDBContext context)
		{
			_context = context;
		}

		public List<ObjectType> GetAllTypes(int userId)
		{
			var sql = Resources.Resource.GetObjectTypes;
			var param = new SqlParameter("@userID", userId);
			var result = _context.ExecuteSqlStatementAsDataTable(sql, new List<SqlParameter> { param });
			if (result != null && result.Rows != null)
			{
				return result.Rows.Cast<DataRow>().Select(x => new ObjectType
				{
					Name = x.Field<string>("Name"),
					DescriptorArtifactTypeID = x.Field<int>("DescriptorArtifactTypeID")
				}).ToList();
			}
			return new List<ObjectType>();
		}
	}
}
