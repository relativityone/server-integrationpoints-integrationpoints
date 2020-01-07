using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Web.Controllers.API.FieldMappings.FieldClassifiers;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;

namespace kCura.IntegrationPoints.Web.Controllers.API.FieldMappings
{
	public class FieldsClassifierRunner : IFieldsClassifierRunner
	{
		private const int DocumentArtifactTypeID = (int)ArtifactType.Document;

		private readonly IServicesMgr _servicesMgr;

		public FieldsClassifierRunner(IServicesMgr servicesMgr)
		{
			_servicesMgr = servicesMgr;
		}

		public async Task<IList<FieldClassificationResult>> GetFilteredFieldsAsync(int workspaceID, IList<IFieldsClassifier> classifiers)
		{
			List<RelativityObject> fields = (await GetAllDocumentFieldsAsync(workspaceID).ConfigureAwait(false)).ToList();

			IEnumerable<FieldClassificationResult> classifiedFields = await ClassifyFieldsAsync(fields, classifiers, workspaceID).ConfigureAwait(false);

			IList<FieldClassificationResult> filteredFields = classifiedFields
				.Where(x => x.ClassificationLevel < ClassificationLevel.HideFromUser)
				.OrderBy(x => x.Name)
				.ToList();

			return filteredFields;
		}

		private static async Task<IEnumerable<FieldClassificationResult>> ClassifyFieldsAsync(ICollection<RelativityObject> allFields,
			IEnumerable<IFieldsClassifier> filters, int workspaceID)
		{
			Dictionary<string, FieldClassificationResult> aggregatedFilteringResults = await filters
				.Select(f => Observable.FromAsync(() => f.ClassifyAsync(allFields, workspaceID)).Retry(2)) // retry each function once
				.ToObservable()
				.Merge(3) // run up to 3 at the same time`
				.Synchronize() // make sure that we aggregate one result at the time
				.Aggregate(allFields.ToDictionary(x => x.Name, x => new FieldClassificationResult
					{
						Name = x.Name,
						FieldIdentifier = x.ArtifactID.ToString(),
						Type = GetFieldTypeName(x),
						IsIdentifier = IsIdentifier(x),
						ClassificationLevel = ClassificationLevel.AutoMap
					}), (accumulator, classificationResults) =>
					{
						foreach (FieldClassificationResult classificationResult in classificationResults)
						{
							if (accumulator[classificationResult.Name].ClassificationLevel < classificationResult.ClassificationLevel)
							{
								accumulator[classificationResult.Name] = classificationResult;
							}
						}

						return accumulator;
					}
				)
				.FirstAsync();

			return aggregatedFilteringResults.Values;
		}

		private static string GetFieldTypeName(RelativityObject fieldObject)
		{
			FieldValuePair fieldType = fieldObject.FieldValues.SingleOrDefault(x => x.Field.Name == "Field Type");
			return fieldType?.Value.ToString();
		}

		private static bool IsIdentifier(RelativityObject fieldObject)
		{
			FieldValuePair isIdentifierFieldValuePair = fieldObject.FieldValues.SingleOrDefault(x => x.Field.Name == "Is Identifier");
			return isIdentifierFieldValuePair?.Value is bool isIdentifier && isIdentifier;
		}

		private async Task<IEnumerable<RelativityObject>> GetAllDocumentFieldsAsync(int workspaceID)
		{
			using (var objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
			{
				int fieldArtifactTypeID = (int)ArtifactType.Field;
				QueryRequest queryRequest = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						ArtifactTypeID = fieldArtifactTypeID
					},
					Condition = $"'FieldArtifactTypeID' == {DocumentArtifactTypeID}",
					Fields = new[]
					{
						new FieldRef()
						{
							Name = "*"
						}
					},
					IncludeNameInQueryResult = true
				};

				const int queryBatchSize = 50;
				int resultCount = 0;
				List<RelativityObject> retrievedObjects = new List<RelativityObject>();

				do
				{
					QueryResult queryResult = await objectManager.QueryAsync(workspaceID, queryRequest,
						start: retrievedObjects.Count + 1, length: queryBatchSize).ConfigureAwait(false);
					retrievedObjects.AddRange(queryResult.Objects);
					resultCount = queryResult.ResultCount;
				} while (resultCount > 0);

				return retrievedObjects;
			}
		}
	}
}