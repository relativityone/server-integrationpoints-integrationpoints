using System;
using System.Collections.Generic;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Data
{
	public class ChoiceQuery : IChoiceQuery
	{
		private readonly IRSAPIClient _client;

		public ChoiceQuery(IRSAPIClient client)
		{
			_client = client;
		}

		public List<Relativity.Client.DTOs.Choice> GetChoicesOnField(int fieldArtifactId)
		{
			var field = _client.Repositories.Field.ReadSingle(fieldArtifactId);
			return field.Choices;
		}

		public List<Relativity.Client.DTOs.Choice> GetChoicesOnField(Guid fieldGuid)
		{
			var field = _client.Repositories.Field.ReadSingle(fieldGuid);
			return field.Choices;
		}

		public List<Artifact> GetChoicesByQuery(Query query)
		{
			QueryResult result = _client.Query(_client.APIOptions, query);
			if (!result.Success)
			{
				throw new Exception(result.Message);
			}

			return result.QueryArtifacts;
		}
	}
}
