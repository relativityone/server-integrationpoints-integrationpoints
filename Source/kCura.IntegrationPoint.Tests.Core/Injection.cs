using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using kCura.Data.RowDataGateway;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Injection
	{
		public enum InjectionBehavior
		{
			Log = 1,
			Error = 2,
			InfiniteLoop = 3,
			Sleep = 4,
			PerformanceLog = 5,
			WaitUntil = 6,
			ErrorUntil = 7,
			SetTime = 8,
			ErrorWithParameters = 9
		};

		public static IDBContext DbContext => GetDbContext();

		public static void EnableInjectionPoint(string injectionPointId, InjectionBehavior injectionBehavior, string behaviorData, string injectionData)
		{
			string query = "INSERT INTO [Injection] VALUES (@injectionPointId, @injectionBehavior, @behaviorData, @injectionData, @enabled)";

			SqlParameter[] parameters = new SqlParameter[]
			{
				new SqlParameter("injectionPointId", SqlDbType.UniqueIdentifier) { Value = new Guid(injectionPointId) },
				new SqlParameter("injectionBehavior", SqlDbType.Int) { Value = (int)injectionBehavior },
				new SqlParameter("behaviorData", SqlDbType.NVarChar) { Value = behaviorData },
				new SqlParameter("injectionData", SqlDbType.NVarChar) { Value = injectionData },
				new SqlParameter("enabled", SqlDbType.Bit) { Value = 1 }
			};

			try
			{
				DbContext.ExecuteNonQuerySQLStatement(query, parameters);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error occurred while trying to enable InjectionPointId: { injectionPointId }. { ex.Message }");
			}
		}

		public static void RemoveInjectionPoint(string injectionPointId)
		{
			string query = "DELETE FROM [Injection] WHERE [InjectionPointID] = @injectionPointId";

			SqlParameter[] parameters = new SqlParameter[]
			{
				new SqlParameter("injectionPointId", SqlDbType.UniqueIdentifier) { Value = new Guid(injectionPointId) },
			};

			try
			{
				DbContext.ExecuteNonQuerySQLStatement(query, parameters);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error occurred while trying to remove InjectionPointId: { injectionPointId }. { ex.Message }");
			}
		}

		public static void WaitUntilInjectionPointIsReached(string injectionPointId, DateTime startTime, int timeoutInSeconds = 180, int intervalInMilliseconds = 500)
		{
			double timeWaitedInSeconds = 0.0;
			int executionCount = GetInjectionPointCount(injectionPointId, startTime);
			bool injectionPointEnabled = IsInjectionPointEnabled(injectionPointId);

			if (!injectionPointEnabled)
			{
				throw new Exception($"InjectionPointId: { injectionPointId } is not enabled.");
			}

			while (executionCount == 0)
			{
				if (timeWaitedInSeconds >= timeoutInSeconds)
				{
					throw new Exception($"Timed out waiting for InjectionPointId: { injectionPointId } to be reached. Waited { timeWaitedInSeconds } seconds.");
				}

				Thread.Sleep(intervalInMilliseconds);
				timeWaitedInSeconds += (intervalInMilliseconds / 1000.0);
				executionCount = GetInjectionPointCount(injectionPointId, startTime);
			}
		}

		private static IDBContext GetDbContext()
		{
			Context baseContext = new Context(SharedVariables.EddsConnectionString);
			TestDbContext context = new TestDbContext(baseContext);
			return context;
		}

		private static int GetInjectionPointCount(string injectionPointId, DateTime startTime)
		{
			string query = "SELECT Count(*) FROM [InjectionLog] WHERE [InjectionPointID] = @injectionPointId AND [TimeStamp] >= @startTime";

			SqlParameter[] parameters = new SqlParameter[]
			{
				new SqlParameter("injectionPointId", SqlDbType.UniqueIdentifier) { Value = new Guid(injectionPointId) },
				new SqlParameter("startTime", SqlDbType.DateTime) { Value = startTime },
			};

			try
			{
				int executionCount = DbContext.ExecuteSqlStatementAsScalar<int>(query, parameters);
				return executionCount;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error occurred while trying to retrieve the execution count for InjectionPointId: { injectionPointId }. { ex.Message }");
			}
		}

		private static bool IsInjectionPointEnabled(string injectionPointId)
		{
			string query = "SELECT Count(*) FROM [Injection] WHERE [InjectionPointID] = @injectionPointId";

			SqlParameter[] parameters = new SqlParameter[]
			{
				new SqlParameter("injectionPointId", SqlDbType.UniqueIdentifier) { Value = new Guid(injectionPointId) }
			};

			try
			{
				int executionCount = DbContext.ExecuteSqlStatementAsScalar<int>(query, parameters);

				return executionCount > 0;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error occurred while trying to check if InjectionPointId: { injectionPointId } is enabled. { ex.Message }");
			}
		}
	}
}