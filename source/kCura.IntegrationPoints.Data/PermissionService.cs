using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public class PermissionService : IPermissionService
	{
		private readonly IWorkspaceDBContext _context;
		public PermissionService(IWorkspaceDBContext context)
		{
			_context = context;
		}


		public bool userCanImport(int userId)
		{
			var sql = Resources.Resource.CheckImportPermission;
			var param = new SqlParameter("@userID", userId);
			var result = _context.ExecuteSqlStatementAsDataTable(sql, new List<SqlParameter> { param });
			if (result != null && result.Rows != null)
			{
				var user = result.Rows.Cast<DataRow>().Select(x => x.Field<int>("UserArtifactID")).FirstOrDefault();
				if (user > 0)
				{
					return true;
				}
			}
			return false;
		}

	}
}
