using System.Collections.Generic;
using System.Data.SqlClient;
using NSubstitute;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class DBContextMockBuilder
	{
		public static IDBContext Build(kCura.Data.RowDataGateway.BaseContext baseContext)
		{
			IDBContext context = Substitute.For<IDBContext>();

			context
				.When(x => x.BeginTransaction())
				.Do(x => baseContext
					.BeginTransaction()
				);

			context.When(x => x.CommitTransaction())
				.Do(x => baseContext
					.CommitTransaction()
				);

			context
				.ExecuteNonQuerySQLStatement(Arg.Any<string>())
				.Returns(x => baseContext
					.ExecuteNonQuerySQLStatement(
						x.Arg<string>()
					)
				);

			context
				.ExecuteNonQuerySQLStatement(Arg.Any<string>(), Arg.Any<IEnumerable<SqlParameter>>())
				.Returns(x => baseContext
					.ExecuteNonQuerySQLStatement(
						x.ArgAt<string>(0),
						x.ArgAt<IEnumerable<SqlParameter>>(1)
					)
				);

			context
				.ExecuteSqlStatementAsDataTable(Arg.Any<string>())
				.Returns(x => baseContext
					.ExecuteSqlStatementAsDataTable(
						x.Arg<string>()
					)
				);

			context.ExecuteSqlStatementAsDataTable(Arg.Any<string>(), Arg.Any<IEnumerable<SqlParameter>>())
				.Returns(x => baseContext
					.ExecuteSqlStatementAsDataTable(
						x.ArgAt<string>(0),
						x.ArgAt<IEnumerable<SqlParameter>>(1)
					)
				);

			return context;
		}
	}
}
