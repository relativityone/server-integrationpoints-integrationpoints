using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using Field = kCura.Relativity.Client.Field;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class GetFolderPathFieldsController : ApiController
	{
		private readonly IRSAPIClient _client;

		public GetFolderPathFieldsController(IRSAPIClient client)
		{
			_client = client;
		}

		[HttpGet]
		public HttpResponseMessage Get()
		{
			List<FieldEntry> fields = GetTextFields(Convert.ToInt32(ArtifactType.Document));
			return Request.CreateResponse(HttpStatusCode.OK, fields, Configuration.Formatters.JsonFormatter);
		}

		private List<FieldEntry> GetTextFields(int rdoTypeId)
		{
			var rdoCondition = new ObjectCondition
			{
				Field = "Object Type Artifact Type ID",
				Operator = ObjectConditionEnum.AnyOfThese,
				Value = new List<int> { rdoTypeId }
			};

			var longTextCondition = new TextCondition
			{
				Field = "Field Type",
				Operator = TextConditionEnum.EqualTo,
				Value = "Long Text"
			};

			var fixedLengthTextCondition = new TextCondition
			{
				Field = "Field Type",
				Operator = TextConditionEnum.EqualTo,
				Value = "Fixed-Length Text"
			};

			Query query = new Query
			{
				ArtifactTypeName = "Field",
				Fields = new List<Field>()
			};
			CompositeCondition documentLongTextCondition = new CompositeCondition(rdoCondition, CompositeConditionEnum.And, longTextCondition);
			CompositeCondition documentFixedLengthTextCondition = new CompositeCondition(rdoCondition, CompositeConditionEnum.And, fixedLengthTextCondition);
			query.Condition = new CompositeCondition(documentLongTextCondition, CompositeConditionEnum.Or, documentFixedLengthTextCondition);

			var result = _client.Query(_client.APIOptions, query);

			if (!result.Success)
			{
				throw new Exception(result.Message);
			}
			List<FieldEntry> fieldEntries = ConvertToFieldEntries(result.QueryArtifacts);
			return fieldEntries;
		}

		private List<FieldEntry> ConvertToFieldEntries(List<Artifact> artifacts)
		{
			List<FieldEntry> fieldEntries = new List<FieldEntry>();

			foreach (Artifact artifact in artifacts)
			{
				foreach (Field field in artifact.Fields)
				{
					if (field.Name == "Name")
					{
						fieldEntries.Add(new FieldEntry()
						{
							DisplayName = field.Value as string,
							FieldIdentifier = artifact.ArtifactID.ToString(),
							IsRequired = false
						});
						break;
					}
				}
			}
			return fieldEntries;
		}
	}
}