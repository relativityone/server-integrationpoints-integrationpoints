using System.Data.SqlClient;
using System.Net;
using System.Security;
using System.Threading.Tasks;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class ToggleHelper
	{
		public static async Task SetToggleAsync(string toggleName, bool toggleValue)
		{
			SecureString password = new NetworkCredential("", SharedVariables.DatabasePassword).SecurePassword;
			password.MakeReadOnly();

			SqlCredential credential = new SqlCredential(SharedVariables.DatabaseUserId, password);

			using (SqlConnection connection = new SqlConnection($"Data Source={SharedVariables.SqlServer};Initial Catalog=EDDS", credential))
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
					SqlCommand toggleInsertCommand = new SqlCommand(@"INSERT INTO [EDDS].[eddsdbo].[Toggle] ([Name], [IsEnabled], [Default]) VALUES (@toggleName, @toggleValue, 0)", connection);
					toggleInsertCommand.Parameters.AddWithValue("toggleName", toggleName);
					toggleInsertCommand.Parameters.AddWithValue("toggleValue", toggleValue);

					await toggleInsertCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
				}
			}
		}
	}
}
