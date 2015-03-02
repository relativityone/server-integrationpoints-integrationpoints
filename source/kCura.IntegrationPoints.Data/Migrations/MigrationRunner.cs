using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Migrations
{
	public class MigrationRunner
	{
		private readonly IEddsDBContext _context;
		public MigrationRunner(IEddsDBContext context)
		{
			_context = context;
		}
		public void Run()
		{
			foreach (var migration in GetMigrations())
			{
				migration.Execute();
			}
		}

		public virtual IEnumerable<IMigration> GetMigrations()
		{
			yield return new AddWebApiConfigValueMigration(_context);
			yield return new UpdateJobErrorsBlankToNo(_context);
		}

	}
}
