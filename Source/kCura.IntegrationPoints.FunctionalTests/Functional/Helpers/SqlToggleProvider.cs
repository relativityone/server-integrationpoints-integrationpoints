using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Relativity.Toggles;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers
{
    public class SqlToggleProvider : IToggleProviderExtended
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
            string toggleName = typeof(T).FullName;

            return SetAsync(toggleName, enabled);
        }

        public Task SetAsync(string name, bool enabled)
        {
            using (SqlConnection connection = _connectionFunc())
            {
                connection.Open();

                int value = enabled ? 1 : 0;

                string sqlStatement =
                     "BEGIN" +
                    $"  IF EXISTS (SELECT * FROM [ToggleDefault] WHERE [Name] = '{name}')" +
                    $"    UPDATE [ToggleDefault] SET [Default] = {value} WHERE [Name] = '{name}'" +
                     "  ELSE" +
                    $"    INSERT [ToggleDefault] ([Name], [Default]) VALUES ('{name}', {value})" +
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
