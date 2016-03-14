using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.Relativity.ImportAPI;
using Field = kCura.Relativity.Client.Field;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class GetFolderPathFieldsController : ApiController
	{
		private readonly IRSAPIClient _client;
		private const string _USERNAME = "XxX_BearerTokenCredentials_XxX";

		public GetFolderPathFieldsController(IRSAPIClient client)
		{
			_client = client;
		}

		[HttpGet]
		public HttpResponseMessage Get()
		{
			string authToken = System.Security.Claims.ClaimsPrincipal.Current.Claims.Single(x => x.Type.Equals("access_token")).Value;
			IImportAPI importApi = new ExtendedImportAPI(_USERNAME, authToken, Config.WebAPIPath);

			List<FieldEntry> fieldEntries = GetTextFields(Convert.ToInt32(ArtifactType.Document));
			IEnumerable<Relativity.ImportAPI.Data.Field> workspaceFields = importApi.GetWorkspaceFields(_client.APIOptions.WorkspaceID, Convert.ToInt32(ArtifactType.Document));
			HashSet<int> mappableArtifactIds = new HashSet<int>(workspaceFields.Select(x => x.ArtifactID));
			IEnumerable<FieldEntry> abc = fieldEntries.Where(x => mappableArtifactIds.Contains(Convert.ToInt32(x.FieldIdentifier)));

			return Request.CreateResponse(HttpStatusCode.OK, abc, Configuration.Formatters.JsonFormatter);
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
				Fields = new List<Field>(),
				Sorts = new List<Sort>()
				{
					new Sort()
					{
						Field = "Name",
						Direction = SortEnum.Ascending
					}
				}
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