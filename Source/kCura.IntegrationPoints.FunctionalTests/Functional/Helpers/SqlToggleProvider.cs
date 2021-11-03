using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Relativity.Toggles;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers
{
	public class SqlToggleProvider : IToggleProvider
	{
		private readonly Func<SqlConnection> _connectionFunc;

		private SqlToggleProvider(Func<SqlConnection> connectionFunc)
		{
			_connectionFunc = connectionFunc;
		}

		public static SqlToggleProvider Create()
		{
			Func<SqlConnection> connectionFunc = SqlHelper.CreateEddsConnectionFromAppConfig;

			return new SqlToggleProvider(connectionFunc);
		}

		public Task SetAsync<T>(bool enabled) where T : IToggle
		{
			using (SqlConnection connection = _connectionFunc())
			{
				connection.Open();

				string toggleName = typeof(T).FullName;

				int value = enabled ? 1 : 0;

				string sqlStatement = 
					 "BEGIN" +
					$"  IF EXISTS (SELECT * FROM [ToggleDefault] WHERE [Name] = '{toggleName}')" +
					$"    UPDATE [ToggleDefault] SET [Default] = {value} WHERE [Name] = '{toggleName}'" +
					 "  ELSE" +
					$"    INSERT [ToggleDefault] ([Name], [Default]) VALUES ('{toggleName}', {value})" +
					 "END";

				SqlCommand command = new SqlCommand(sqlStatement, connection);

				command.ExecuteNonQuery();
			}

			return Task.CompletedTask;
		}

		#region Not Implemented

		public bool IsEnabled<T>() where T : IToggle
		{
			throw new NotImplementedException();
		}

		public Task<bool> IsEnabledAsync<T>() where T : IToggle
		{
			throw new NotImplementedException();
		}

		public bool IsEnabledByName(string toggleName)
		{
			throw new NotImplementedException();
		}

		public Task<bool> IsEnabledByNameAsync(string toggleName)
		{
			throw new NotImplementedException();
		}

		public MissingFeatureBehavior DefaultMissingFeatureBehavior { get; }
		public bool CacheEnabled { get; set; }
		public int CacheTimeoutInSeconds { get; set; }

		#endregion
	}
}
