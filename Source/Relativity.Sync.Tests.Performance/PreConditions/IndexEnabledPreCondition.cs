using System;
using System.Data.SqlClient;
using Relativity.Sync.Tests.System.Core.Helpers;

namespace Relativity.Sync.Tests.Performance.PreConditions
{
	internal class IndexEnabledPreCondition : IPreCondition
	{
		private readonly int _workspaceId;

		public string Name => $"{nameof(IndexEnabledPreCondition)} - {_workspaceId}";

		public IndexEnabledPreCondition(int workspaceArtifactId)
		{
			_workspaceId = workspaceArtifactId;
		}


		public bool Check()
		{
			using (SqlConnection connection = SqlHelper.CreateConnectionFromAppConfig(_workspaceId))
			{
				connection.Open();

				string sqlStatement =
					"SELECT COUNT(*) FROM sys.indexes WHERE name = 'IX_Identifier' AND object_id = OBJECT_ID('EDDSDBO.Document') AND is_disabled = 0";

				SqlCommand command = new SqlCommand(sqlStatement, connection);

				int enabledIndex = (int)command.ExecuteScalar();
				return enabledIndex > 0;
			}
		}

		public FixResult TryFix()
		{
			using (SqlConnection connection = SqlHelper.CreateConnectionFromAppConfig(_workspaceId))
			{
				connection.Open();

				string sqlStatement = "ALTER INDEX IX_Identifier ON EDDSDBO.Document REBUILD";

				SqlCommand command = new SqlCommand(sqlStatement, connection);

				command.ExecuteNonQuery();
			}

			return Check()
				? FixResult.Fixed()
				: FixResult.Error(new Exception($"{nameof(IndexEnabledPreCondition)} - Check is still failing after fix"));
		}
	}
}
