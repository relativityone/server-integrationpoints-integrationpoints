using System;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services
{
	public class UserService
	{
		private readonly IRSAPIClient _client;

		public UserService(IRSAPIClient client)
		{
			_client = client;
		}

		public Relativity.Client.DTOs.User Read(int id)
		{
			ResultSet<Relativity.Client.DTOs.User> result;
			var workspaceID = _client.APIOptions.WorkspaceID;
			_client.APIOptions.WorkspaceID = -1;
			try
			{
				result = _client.Repositories.User.Read(id);
			}
			finally
			{
				_client.APIOptions.WorkspaceID = workspaceID;
			}
			if (!result.Success)
			{
				var messages = result.Results.Where(x => !x.Success).Select(x => x.Message);
				var e = new AggregateException(result.Message, messages.Select(x => new Exception(x)));
				throw e;
			}
			return result.Results.First().Artifact;
		}
	}
}
