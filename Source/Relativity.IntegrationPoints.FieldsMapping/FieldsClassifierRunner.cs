using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;

namespace Relativity.IntegrationPoints.FieldsMapping
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
				.OrderByDescending(x => x.IsIdentifier)
				.ThenBy(x => x.Name)
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
						IsRequired = IsRequired(x),
						Length = GetLength(x),
						ClassificationLevel = ClassificationLevel.AutoMap
					}), (accumulator, classifierResults) =>
					{
						foreach (FieldClassificationResult classifierResult in classifierResults)
						{
							var field = accumulator[classifierResult.Name];

							if (field.ClassificationLevel < classifierResult.ClassificationLevel)
							{
								field.ClassificationLevel = classifierResult.ClassificationLevel;
								field.ClassificationReason = classifierResult.ClassificationReason;
							}
						}

						return accumulator;
					}
				)
				.FirstAsync();

			return aggregatedFilteringResults.Values;
		}

		private static int GetLength(RelativityObject fieldObject)
		{
			FieldValuePair lengthFieldValuePair = fieldObject.FieldValues.SingleOrDefault(x => x.Field.Name == "Length");
			return lengthFieldValuePair?.Value is int length ? length : 0;
		}

		private static string GetFieldTypeName(RelativityObject fieldObject)
		{
			FieldValuePair fieldType = fieldObject.FieldValues.SingleOrDefault(x => x.Field.Name == "Field Type");
			var fixedLengthText = "Fixed-Length Text";
			if (fieldType?.Value != null && fieldType.Value.Equals(fixedLengthText))
			{
				FieldValuePair fieldLength = fieldObject.FieldValues.SingleOrDefault(x => x.Field.Name == "Length");

				return $"{fixedLengthText}({fieldLength?.Value})";
			}

			return fieldType?.Value.ToString();
		}

		private static bool IsIdentifier(RelativityObject fieldObject)
		{
			FieldValuePair isIdentifierFieldValuePair = fieldObject.FieldValues.SingleOrDefault(x => x.Field.Name == "Is Identifier");
			return isIdentifierFieldValuePair?.Value is bool isIdentifier && isIdentifier;
		}

		private static bool IsRequired(RelativityObject fieldObject)
		{
			FieldValuePair isRequiredFieldValuePair = fieldObject.FieldValues.SingleOrDefault(x => x.Field.Name == "Is Required");
			return isRequiredFieldValuePair?.Value is bool isRequired && isRequired;
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