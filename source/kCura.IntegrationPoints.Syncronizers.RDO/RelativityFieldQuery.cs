using System.Collections.Generic;
using kCura.Relativity.Client;
using Artifact = kCura.Relativity.Client.Artifact;
using Field = kCura.Relativity.Client.Field;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RelativityFieldQuery
	{
		private readonly IRSAPIClient _client;

		public RelativityFieldQuery(IRSAPIClient client)
		{
			_client = client;
		}
		public virtual List<Artifact> GetFieldsForRDO(int rdoTypeID)
		{
			return GetAllFields(rdoTypeID);
		}

		public List<Artifact> GetAllFields(int rdoTypeID)
		{
			Query q = new Query()
			{
				ArtifactTypeName = "Field",
				Fields = new List<Field>() { new Field("Name"), new Field("Choices"), new Field("Object Type Artifact Type ID"), new Field("Field Type"), new Field("Field Type ID") },
				Condition = new ObjectCondition { Field = "Object Type Artifact Type ID", Operator = ObjectConditionEnum.AnyOfThese, Value = new List<int> { rdoTypeID } },
				Sorts = new List<Sort>() { new Sort() { Direction = SortEnum.Ascending, Field = "ArtifactID", Order = 1 } }
			};
			var results = _client.Query(_client.APIOptions, q);
			if (!results.Success)
			{
				//TODO: handle better
			}
			return results.QueryArtifacts;
		}
	}
}
