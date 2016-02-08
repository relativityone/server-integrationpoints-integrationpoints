using System;
using System.Collections.Generic;
using kCura.Relativity.Client;
using Artifact = kCura.Relativity.Client.Artifact;
using Field = kCura.Relativity.Client.Field;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RelativityFieldQuery : IRelativityFieldQuery
	{
		private readonly IRSAPIClient _client;

		public RelativityFieldQuery(IRSAPIClient client)
		{
			_client = client;
		}

		public virtual List<Artifact> GetFieldsForRdo(int rdoTypeId)
		{
			return GetAllFields(rdoTypeId);
		}

		public virtual List<Artifact> GetFieldsForRdo(int rdoTypeId, int workspaceId)
		{
			int previousWorkspaceId = _client.APIOptions.WorkspaceID;
			_client.APIOptions.WorkspaceID = workspaceId;
			try
			{
				return GetAllFields(rdoTypeId);
			}
			finally
			{
				_client.APIOptions.WorkspaceID = previousWorkspaceId;
			}
		}

		public List<Artifact> GetAllFields(int rdoTypeId)
		{
			Query q = new Query()
			{
				ArtifactTypeName = "Field",
				Fields = new List<Field>() { new Field("Name"), new Field("Choices"), new Field("Object Type Artifact Type ID"), new Field("Field Type"), new Field("Field Type ID"), new Field("Is Identifier"), new Field("Field Type Name") },
				Condition = new ObjectCondition { Field = "Object Type Artifact Type ID", Operator = ObjectConditionEnum.AnyOfThese, Value = new List<int> { rdoTypeId } },

				Sorts = new List<Sort>() { new Sort() { Direction = SortEnum.Ascending, Field = "Name", Order = 1 } }
			};
			var result = _client.Query(_client.APIOptions, q);
			if (!result.Success)
			{
				var messages = result.Message;
				var e = messages; 
				throw new Exception(e);
			}
			return result.QueryArtifacts;
		}
	}
}
