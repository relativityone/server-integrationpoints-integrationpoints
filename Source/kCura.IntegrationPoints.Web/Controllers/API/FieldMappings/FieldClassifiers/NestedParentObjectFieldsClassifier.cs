using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;

namespace kCura.IntegrationPoints.Web.Controllers.API.FieldMappings.FieldClassifiers
{
	public class NestedParentObjectFieldsClassifier : IFieldsClassifier
	{
		private const int WorkspaceArtifactTypeID = (int)ArtifactType.Case;

		private readonly IServicesMgr _servicesMgr;

		public NestedParentObjectFieldsClassifier(IServicesMgr servicesMgr)
		{
			_servicesMgr = servicesMgr;
		}

		// TODO
		// FieldManager is painfully slow and ObjectManager has defect:
		// https://jira.kcura.com/browse/REL-369204
		public async Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<RelativityObject> fields, int workspaceID)
		{
			List<RelativityObject> objectFields = fields
				.Where(x => x.FieldValues.Exists(fieldValuePair =>
								(fieldValuePair.Field.Name == "Field Type" &&
								 (fieldValuePair.Value.ToString() == "Single Object" ||
								  fieldValuePair.Value.ToString() == "Multiple Object"))) &&
							x.FieldValues.Exists(fieldValuePair =>
								fieldValuePair.Field.Name == "Associative Object Type" && fieldValuePair.Value != null))
				.ToList();

			var fieldsWithAssociativeObjectType = new List<FieldResponse>();
			using (var fieldManager = _servicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
			{
				await objectFields.Select(x =>
						Observable.FromAsync(() => fieldManager.ReadAsync(workspaceID, x.ArtifactID)))
					.ToObservable()
					.Merge(10)
					.Where(x => x.AssociativeObjectType != null)
					.Synchronize()
					.Do(x => fieldsWithAssociativeObjectType.Add(x))
					.LastOrDefaultAsync();
			}

			using (var objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				QueryRequest queryRequest = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						ArtifactTypeID = (int)ArtifactType.ObjectType
					},
					Condition =
						$"'Name' IN [{string.Join(",", fieldsWithAssociativeObjectType.Select(x => $"'{x.AssociativeObjectType.Name}'"))}]",
					Fields = new[]
					{
						new FieldRef()
						{
							Name = "Parent ArtifactTypeID"
						}
					},
					IncludeNameInQueryResult = true
				};

				QueryResult queryResult = await objectManager
					.QueryAsync(workspaceID, queryRequest, 1, fieldsWithAssociativeObjectType.Count)
					.ConfigureAwait(false);

				IEnumerable<FieldClassificationResult> filteredOutFields = fieldsWithAssociativeObjectType
					.Where(x => queryResult.Objects.Exists(objectType =>
						objectType.Name == x.AssociativeObjectType.Name &&
						(int)objectType.FieldValues.First().Value != WorkspaceArtifactTypeID))
					.Select(x => new FieldClassificationResult()
					{
						Name = x.Name,
						ClassificationReason = "Field has nested parent object type.",
						ClassificationLevel = ClassificationLevel.ShowToUser
					});

				return filteredOutFields;
			}
		}
	}
}