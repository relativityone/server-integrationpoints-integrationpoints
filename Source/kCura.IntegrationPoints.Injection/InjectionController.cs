using System;
using System.Data;
using System.Data.SqlClient;
using kCura.Data.RowDataGateway;
using kCura.Injection;
using kCura.Injection.Behavior;
using kCura.IntegrationPoints.Injection.Behaviors;

namespace kCura.IntegrationPoints.Injection
{
	public class InjectionController : IController
	{
		public static Context Context => _context ?? (_context = new Context());

		private static Context _context;

		public void Log(InjectionPoint injectionPoint, string message)
		{
			LogInjection(injectionPoint.ID, message);
		}

		public kCura.Injection.Injection GetInjection(string injectionPointId)
		{
			InjectionPoint injectionPoint = GetInjectionPoint(injectionPointId);

			if (injectionPoint == null)
			{
				return null;
			}

			using (DataTable injectionDataTable = GetInjectionFromDatabase(injectionPointId))
			{
				if (injectionDataTable == null)
				{
					return null; // when the behavior is set to InfiniteLoop, this will end up logging an "Infinite Loop End..." message
				}

				int injectionBehaviorInt = (int)injectionDataTable.Rows[0][0];
				string behaviorData = injectionDataTable.Rows[0][1].ToString();
				string injectionData = injectionDataTable.Rows[0][2].ToString();

				IBehavior injectionBehavior = DetermineInjectionBehavior(injectionBehaviorInt);

				var injection = new kCura.Injection.Injection(injectionPoint, injectionBehavior, behaviorData, injectionData);
				return injection;
			}
		}

		#region Helper methods

		private InjectionPoint GetInjectionPoint(string injectionPointId)
		{
			const string getInjectionPointQuery = @"SELECT [ID]
													,[Description]
													,[Feature]
													FROM [InjectionPoint] WHERE [ID] = @injectionPointId";

			SqlParameter[] parameters = {
				new SqlParameter("injectionPointId", SqlDbType.UniqueIdentifier) { Value = Guid.Parse(injectionPointId) },
			};

			try
			{
				using (DataTable dataTable = Context.ExecuteSqlStatementAsDataTable(getInjectionPointQuery, parameters))
				{
					InjectionPoint injectionPoint = null;

					if (dataTable.Rows.Count > 0)
					{
						injectionPoint = new InjectionPoint(dataTable.Rows[0][0].ToString(),
							dataTable.Rows[0][1].ToString(),
							dataTable.Rows[0][2].ToString());
					}

					return injectionPoint;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error occurred while trying to retrieve the injection point with Guid '{injectionPointId}'", ex);
			}
		}

		private DataTable GetInjectionFromDatabase(string injectionPointId)
		{
			const string getInjectionQuery = @"SELECT [Behavior]
												,[BehaviorData]
												,[InjectionData]
												FROM [Injection] WHERE [InjectionPointID] = @injectionPointId";

			SqlParameter[] parameters = {
				new SqlParameter("injectionPointId", SqlDbType.UniqueIdentifier) { Value = Guid.Parse(injectionPointId) },
			};

			try
			{
				using (DataTable dataTable = Context.ExecuteSqlStatementAsDataTable(getInjectionQuery, parameters))
				{
					if (dataTable != null && dataTable.Rows.Count > 0)
					{
						return dataTable;
					}

					return null;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error occurred while trying to retrieve the injection with Guid '{injectionPointId}'", ex);
			}
		}

		private IBehavior DetermineInjectionBehavior(int injectionBehaviorInt)
		{
			IBehavior injectionBehavior;

			switch ((InjectionBehavior.BehaviorType)injectionBehaviorInt)
			{
				case InjectionBehavior.BehaviorType.Log:
					injectionBehavior = new Log();
					break;
				case InjectionBehavior.BehaviorType.Error:
					injectionBehavior = new ErrorWithLog();
					break;
				case InjectionBehavior.BehaviorType.InfiniteLoop:
					injectionBehavior = new InfiniteLoop();
					break;
				case InjectionBehavior.BehaviorType.Sleep:
					injectionBehavior = new Sleep();
					break;
				case InjectionBehavior.BehaviorType.PerformanceLog:
					injectionBehavior = new PerformanceLog();
					break;
				case InjectionBehavior.BehaviorType.WaitUntil:
					injectionBehavior = new WaitUntil();
					break;
				default:
					injectionBehavior = null;
					break;
			}

			return injectionBehavior;
		}

		private void LogInjection(string injectionPointId, string message)
		{
			const string insertInjectionLog = @"INSERT INTO [InjectionLog] 
												([InjectionPointID], [Message], [ServerName], [TimeStamp])
												VALUES(@injectionPointId, @logMessage, @server, @timeStamp)";

			SqlParameter[] parameters = {
				new SqlParameter("injectionPointId", SqlDbType.UniqueIdentifier) { Value = Guid.Parse(injectionPointId) },
				new SqlParameter("logMessage", SqlDbType.NVarChar) { Value = message },
				new SqlParameter("server", SqlDbType.NVarChar) { Value = Context.ServerName },
				new SqlParameter("timeStamp", SqlDbType.DateTime) { Value = DateTime.UtcNow }
			};

			try
			{
				Context.ExecuteNonQuerySQLStatement(insertInjectionLog, parameters);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error occurred while trying to insert an injection log with Guid '{injectionPointId}'", ex);
			}
		}

		#endregion Helper methods
	}
}
