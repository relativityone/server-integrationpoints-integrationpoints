using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
	public class ToggleHelper
	{
		private static Func<SqlConnection> SqlConnectionFunc => SqlHelper.CreateEddsConnectionFromAppConfig;

		public static async Task<bool?> GetToggleAsync(string toggleName)
		{
			using (SqlConnection connection = SqlConnectionFunc())
			{
				connection.Open();

				SqlCommand toggleReadIsEnabledCommand = new SqlCommand(@"SELECT [IsEnabled] FROM [EDDS].[eddsdbo].[Toggle] WHERE Name = @toggleName", connection);
				toggleReadIsEnabledCommand.Parameters.AddWithValue("toggleName", toggleName);

				return (bool?) await toggleReadIsEnabledCommand.ExecuteScalarAsync().ConfigureAwait(false);
			}
		}

		public static async Task SetToggleAsync(string toggleName, bool toggleValue)
		{
			using (SqlConnection connection = SqlConnectionFunc())
			{
				connection.Open();

				SqlCommand toggleExistsCommand = new SqlCommand(@"SELECT Count(*) FROM [EDDS].[eddsdbo].[Toggle] WHERE Name = @toggleName", connection);
				toggleExistsCommand.Parameters.AddWithValue("toggleName", toggleName);
				if ((int)await toggleExistsCommand.ExecuteScalarAsync().ConfigureAwait(false) > 0)
				{
					SqlCommand toggleUpdateCommand = new SqlCommand(@"UPDATE [EDDS].[eddsdbo].[Toggle] SET IsEnabled = @toggleValue WHERE Name = @toggleName", connection);
					toggleUpdateCommand.Parameters.AddWithValue("toggleValue", toggleValue);
					toggleUpdateCommand.Parameters.AddWithValue("toggleName", toggleName);

					await toggleUpdateCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
				}
				else
				{
					SqlCommand toggleInsertCommand = new SqlCommand(@"INSERT INTO [EDDS].[eddsdbo].[Toggle] (Name, IsEnabled) VALUES (@toggleName, @toggleValue)", connection);
					toggleInsertCommand.Parameters.AddWithValue("toggleName", toggleName);
					toggleInsertCommand.Parameters.AddWithValue("toggleValue", toggleValue);

					await toggleInsertCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
				}
			}
		}
	}
}
