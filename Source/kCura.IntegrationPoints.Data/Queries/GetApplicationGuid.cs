using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class GetApplicationGuid
	{
		private readonly IDBContext _caseDBcontext;
		public GetApplicationGuid(IDBContext caseDBcontext)
		{
			_caseDBcontext = caseDBcontext;
		}

		public Guid? Execute(int applicationID)
		{
			Guid? applicationGuid = null;
			var sql = Resources.Resource.GetApplicationGuid;
			var sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@ApplicationID", applicationID));

			object returnValue = _caseDBcontext.ExecuteSqlStatementAsScalar(sql, sqlParams.ToArray());
			if (returnValue != null)
			{
				try { applicationGuid = Guid.Parse(returnValue.ToString()); }
				catch { }
			}
			return applicationGuid;
		}
	}
}
