using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
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
				Fields = new List<Field>() { new Field("Name"), new Field("Choices"), new Field("Object Type Artifact Type ID"), new Field("Field Type"), new Field("Field Type ID"), new Field("Is Identifier") },
				Condition = new ObjectCondition { Field = "Object Type Artifact Type ID", Operator = ObjectConditionEnum.AnyOfThese, Value = new List<int> { rdoTypeID } },
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
