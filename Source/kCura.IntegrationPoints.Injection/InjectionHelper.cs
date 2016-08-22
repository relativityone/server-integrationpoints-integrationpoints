using kCura.Data.RowDataGateway;
using kCura.Injection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace kCura.IntegrationPoints.Injection
{
	public static class InjectionHelper
	{
		public static Context Context => _context ?? (_context = new Context());

		private static Context _context;

		#region Add/Remove Injections In Environment

		public static void AddInjectionToEnvironment(string injectionPointId, InjectionBehavior.BehaviorType behavior, string behaviorData = null, string injectionData = null, bool isEnabled = true)
		{
			const string insertInjectionQuery = @"INSERT INTO [Injection]
												([InjectionPointID], [Behavior], [BehaviorData], [InjectionData], [Enabled])
												VALUES(@injectionPointId, @behavior, @behaviorData, @injectionData, @enabled)";

			SqlParameter[] parameters = {
				new SqlParameter("injectionPointId", SqlDbType.UniqueIdentifier) { Value = new Guid(injectionPointId) },
				new SqlParameter("behavior", SqlDbType.Int) { Value = (int) behavior },
				new SqlParameter("behaviorData", SqlDbType.NVarChar) { Value = behaviorData },
				new SqlParameter("injectionData", SqlDbType.NVarChar) { Value = injectionData ?? String.Empty },
				new SqlParameter("enabled", SqlDbType.Bit) { Value = isEnabled }
			};

			try
			{
				Context.ExecuteNonQuerySQLStatement(insertInjectionQuery, parameters);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error occurred while trying to insert the injection with Guid '{injectionPointId}'", ex);
			}
		}

		public static void RemoveInjectionFromEnvironment(string injectionPointId)
		{
			const string deleteInjectionQuery = @"DELETE FROM [Injection]
												WHERE [InjectionPointID] = @injectionPointId";

			SqlParameter[] parameters = {
				new SqlParameter("injectionPointId", SqlDbType.UniqueIdentifier) { Value = new Guid(injectionPointId) },
			};

			try
			{
				Context.ExecuteNonQuerySQLStatement(deleteInjectionQuery, parameters);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error occurred while trying to remove the injection with Guid '{injectionPointId}'", ex);
			}
		}

		#endregion Add/Remove Injections In Environment

		#region Setup/Teardown Injection Points

		public static void CleanupInjectionPoints(IList<InjectionPoint> injectionPoints)
		{
			foreach (InjectionPoint injectionPoint in injectionPoints)
			{
				RemoveInjectionFromEnvironment(injectionPoint.ID);
				RemoveInjectionInfoFromInjectionLogTable(injectionPoint.ID);
				RemoveInjectionPoint(injectionPoint.ID);
			}
		}

		public static void InitializeInjectionPoints(IList<InjectionPoint> injectionPoints)
		{
			foreach (InjectionPoint injectionPoint in injectionPoints)
			{
				InsertInjectionPoint(injectionPoint);
			}
		}

		public static void InitializeAndEnableInjectionPoints(IList<kCura.Injection.Injection> injections)
		{
			foreach (kCura.Injection.Injection injection in injections)
			{
				InsertInjectionPoint(injection.InjectionPoint);

				InjectionBehavior.BehaviorType typeBasedOnBehavior = InjectionBehavior.GetTypeBasedOnBehavior(injection.Behavior);
				AddInjectionToEnvironment(injection.InjectionPoint.ID, typeBasedOnBehavior, injection.BehaviorData, injection.InjectionData);
			}
		}

		public static void InsertInjectionPoint(InjectionPoint injectionPoint)
		{
			const string insertInjectionPointQuery = @"INSERT INTO [InjectionPoint]
													([ID], [Description], [Feature])
													VALUES(@injectionPointId, @description, @feature)";

			SqlParameter[] parameters = {
					new SqlParameter("injectionPointId", SqlDbType.UniqueIdentifier) { Value = new Guid(injectionPoint.ID) },
					new SqlParameter("description", SqlDbType.NVarChar) { Value = injectionPoint.Description },
					new SqlParameter("feature", SqlDbType.NVarChar) { Value = injectionPoint.Feature }
				};

			try
			{
				Context.ExecuteNonQuerySQLStatement(insertInjectionPointQuery, parameters);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error occurred while trying to insert the injection point with Guid '{injectionPoint.ID}'", ex);
			}
		}

		public static void RemoveInjectionPoint(string injectionPointId)
		{
			const string deleteInjectionQuery = @"DELETE FROM [InjectionPoint]
												WHERE [ID] = @injectionPointId";

			SqlParameter[] parameters = {
				new SqlParameter("injectionPointId", SqlDbType.UniqueIdentifier) { Value = new Guid(injectionPointId) },
			};

			try
			{
				Context.ExecuteNonQuerySQLStatement(deleteInjectionQuery, parameters);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error occurred while trying to remove the injection point with Guid '{injectionPointId}'", ex);
			}
		}

		#endregion Setup/Teardown Injection Points

		#region InjectionLog Table

		public static void RemoveInjectionInfoFromInjectionLogTable(string injectionPointId)
		{
			const string deleteInjectionLogQuery = @"DELETE FROM [InjectionLog]
													WHERE [InjectionPointID] = @injectionPointId";

			SqlParameter[] parameters = {
				new SqlParameter("injectionPointId", SqlDbType.UniqueIdentifier) { Value = new Guid(injectionPointId) }
			};

			try
			{
				Context.ExecuteNonQuerySQLStatement(deleteInjectionLogQuery, parameters);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error occurred while trying to remove the injection log with Guid '{injectionPointId}'", ex);
			}
		}

		public static int GetInjectionExecutionCount(string injectionPointId, DateTime startTime)
		{
			const string executionCountQuery = @"SELECT Count(*)
												FROM [InjectionLog]
												WHERE [InjectionPointID] = @injectionPointId AND [TimeStamp] >= @startTime";

			SqlParameter[] parameters = {
				new SqlParameter("injectionPointId", SqlDbType.UniqueIdentifier) { Value = new Guid(injectionPointId) },
				new SqlParameter("startTime", SqlDbType.DateTime) { Value = startTime },
			};

			try
			{
				int executionCount = Context.ExecuteSqlStatementAsScalar<int>(executionCountQuery, parameters);
				return executionCount;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error occurred while trying to retrieve the execution count for injection point with Guid '{injectionPointId}'", ex);
			}
		}

		public static void WaitUntilInjectionPointIsReached(string injectionPointId, DateTime startTime, int timeoutInSeconds = 180, int intervalInMilliseconds = 500)
		{
			double timeWaitedInSeconds = 0.0;
			int executionCount = GetInjectionExecutionCount(injectionPointId, startTime);
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
				executionCount = GetInjectionExecutionCount(injectionPointId, startTime);
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
				int executionCount = Context.ExecuteSqlStatementAsScalar<int>(query, parameters);

				return executionCount > 0;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error occurred while trying to check if InjectionPointId: { injectionPointId } is enabled. { ex.Message }");
			}
		}

		#endregion InjectionLog Table
	}
}