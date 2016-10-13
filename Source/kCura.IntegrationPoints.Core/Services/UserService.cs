using System;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using User = kCura.Relativity.Client.DTOs.User;

namespace kCura.IntegrationPoints.Core.Services
{
	public class UserService
	{
		private readonly IRSAPIClient _client;
		private readonly IAPILog _logger;

		public UserService(IRSAPIClient client, IHelper helper)
		{
			_client = client;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<IHelper>();
		}

		public User Read(int id)
		{
			ResultSet<User> result;
			var workspaceID = _client.APIOptions.WorkspaceID;
			_client.APIOptions.WorkspaceID = -1;
			try
			{
				result = _client.Repositories.User.Read(id);
			}
			catch (Exception e)
			{
				LogRetrieveUserError(e);
				throw;
			}
			finally
			{
				_client.APIOptions.WorkspaceID = workspaceID;
			}
			if (!result.Success)
			{
				var messages = result.Results.Where(x => !x.Success).Select(x => x.Message);
				LogRetrieveUserErrorWithDetails(result);
				var e = new AggregateException(result.Message, messages.Select(x => new Exception(x)));
				throw e;
			}
			return result.Results.First().Artifact;
		}

		#region Logging

		private void LogRetrieveUserErrorWithDetails(ResultSet<User> result)
		{
			_logger.LogError("Failed to retrieve user. Details: {Message}.", result.Message);
		}

		private void LogRetrieveUserError(Exception e)
		{
			_logger.LogError(e, "Failed to retrieve user.");
		}

		#endregion
	}
}