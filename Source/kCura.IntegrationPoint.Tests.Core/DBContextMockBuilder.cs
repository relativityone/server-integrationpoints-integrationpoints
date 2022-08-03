using System.Collections.Generic;
using System.Data.SqlClient;
using kCura.Data.RowDataGateway;
using NSubstitute;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
    public static class DBContextMockBuilder
    {
        public static IDBContext Build(kCura.Data.RowDataGateway.BaseContext baseContext)
        {
            IDBContext context = Substitute.For<IDBContext>();
            context.ServerName
                .Returns(x => 
                    baseContext.ServerName
                );

            context
                .When(x => x.BeginTransaction())
                .Do(x => baseContext
                    .BeginTransaction()
                );

            context
                .When(x => x.CommitTransaction())
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

            context.ExecuteSQLStatementAsReader(Arg.Any<string>())
                .Returns(x => baseContext
                    .ExecuteSQLStatementAsReader(
                        x.Arg<string>()
                    )
                );

            context.GetConnection(Arg.Any<bool>())
                .Returns(x => baseContext
                    .GetConnection(
                        x.Arg<bool>()
                    )
                );

            context.ExecuteSqlStatementAsScalar(Arg.Any<string>(), Arg.Any<SqlParameter[]>())
                .Returns(x => baseContext.ExecuteSqlStatementAsScalar(
                    x.ArgAt<string>(0),
                    x.ArgAt<SqlParameter[]>(1)
                ));

            context.Database.Returns(baseContext.Database);

            SetupExecuteSqlStatementAsScalar<int>(context, baseContext);
            SetupExecuteSqlStatementAsScalar<bool>(context, baseContext);

            return context;
        }

        private static void SetupExecuteSqlStatementAsScalar<T>(IDBContext context, BaseContext baseContext)
        {
            context.ExecuteSqlStatementAsScalar<T>(Arg.Any<string>())
                .Returns(x => baseContext
                    .ExecuteSqlStatementAsScalar<T>(
                        x.Arg<string>()
                    )
                );

            context.ExecuteSqlStatementAsScalar<T>(Arg.Any<string>(), Arg.Any<IEnumerable<SqlParameter>>())
                .Returns(x => baseContext
                    .ExecuteSqlStatementAsScalar<T>(
                        x.ArgAt<string>(0),
                        x.ArgAt<IEnumerable<SqlParameter>>(1)
                    )
                );
        }
    }
}
