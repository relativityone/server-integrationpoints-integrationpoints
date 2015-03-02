using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Migrations
{
	public class UpdateJobErrorsBlankToNo
	{	private readonly IEddsDBContext _context;
	public UpdateJobErrorsBlankToNo(IEddsDBContext context)
		{
			_context = context;
		}

		public void Execute()
		{
			var sql = Resources.Resource.SetBlankLogErrorsToNo;
			_context.ExecuteNonQuerySQLStatement(sql);
		}
	}
}
