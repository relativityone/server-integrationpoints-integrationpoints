using NSubstitute;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class DBContextMockBuilder
	{
		public static IDBContext GetMockContext(kCura.Data.RowDataGateway.BaseContext baseContext)
		{
			IDBContext context = Substitute.For<IDBContext>();
			context.ExecuteNonQuerySQLStatement(Arg.Any<string>())
				.Returns(x => baseContext.ExecuteNonQuerySQLStatement(x.Arg<string>()));


			return context;
		}
	}
}
