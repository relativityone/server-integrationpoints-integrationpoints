using System;
using System.Collections.Generic;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RelativityFieldQuery : IRelativityFieldQuery
	{
		private readonly IRSAPIClient _client;
		private readonly IAPILog _logger;

		public RelativityFieldQuery(IRSAPIClient client, IHelper helper)
		{
			_client = client;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RelativityFieldQuery>();
		}

		public virtual List<Artifact> GetFieldsForRdo(int rdoTypeId)
		{
			return GetAllFields(rdoTypeId);
		}

		public List<Artifact> GetAllFields(int rdoTypeId)
		{
			Query q = new Query
			{
				ArtifactTypeName = "Field",
				Fields =
					new List<Field>
					{
						new Field("Name"),
						new Field("Choices"),
						new Field("Object Type Artifact Type ID"),
						new Field("Field Type"),
						new Field("Field Type ID"),
						new Field("Is Identifier"),
						new Field("Field Type Name")
					},
				Condition = new ObjectCondition {Field = "Object Type Artifact Type ID", Operator = ObjectConditionEnum.AnyOfThese, Value = new List<int> {rdoTypeId}},
				Sorts = new List<Sort> {new Sort {Direction = SortEnum.Ascending, Field = "Name", Order = 1}}
			};
			var result = _client.Query(_client.APIOptions, q);
			if (!result.Success)
			{
				LogRetrievingAllFieldsError(rdoTypeId, result);
				throw new Exception(result.Message);
			}
			return result.QueryArtifacts;
		}

		#region Logging

		private void LogRetrievingAllFieldsError(int rdoTypeId, QueryResult result)
		{
			_logger.LogError("Failed to retrieve all fields for RDO type {RdoTypeId}. Details: {Message}.", rdoTypeId, result.Message);
		}

		#endregion
	}
}