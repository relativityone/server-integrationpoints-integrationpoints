using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;

namespace Relativity.IntegrationPoints.FieldsMapping
{
    public class FieldsClassifierRunner : IFieldsClassifierRunner
    {
        private readonly IFieldsRepository _fieldsRepository;

        private readonly IList<IFieldsClassifier> _fieldsClassifiers;

        public FieldsClassifierRunner(IFieldsRepository fieldsRepository, IList<IFieldsClassifier> fieldsClassifiers = null)
        {
            _fieldsRepository = fieldsRepository;
            _fieldsClassifiers = fieldsClassifiers ?? new List<IFieldsClassifier>();
        }

        public async Task<IList<FieldClassificationResult>> GetFilteredFieldsAsync(int workspaceID, int artifactTypeId)
        {
            List<FieldInfo> fields = (await _fieldsRepository.GetAllFieldsAsync(workspaceID, artifactTypeId).ConfigureAwait(false)).ToList();

            IEnumerable<FieldClassificationResult> classifiedFields = await ClassifyFieldsAsync(fields, workspaceID).ConfigureAwait(false);

            IList<FieldClassificationResult> filteredFields = classifiedFields
                .Where(x => x.ClassificationLevel < ClassificationLevel.HideFromUser)
                .OrderByDescending(x => x.FieldInfo.IsIdentifier)
                .ThenBy(x => x.FieldInfo.Name)
                .ToList();

            return filteredFields;
        }

        public async Task<IEnumerable<FieldClassificationResult>> ClassifyFieldsAsync(ICollection<string> artifactIDs, int workspaceID)
        {
            var fields =
                (await _fieldsRepository.GetFieldsByArtifactsIdAsync(artifactIDs, workspaceID).ConfigureAwait(false))
                .ToList();

            return await ClassifyFieldsAsync(fields, workspaceID).ConfigureAwait(false);
        }

        public async Task<IEnumerable<FieldClassificationResult>> ClassifyFieldsAsync(ICollection<FieldInfo> fields, int workspaceID)
        {
            Dictionary<string, FieldClassificationResult> aggregatedFilteringResults = await _fieldsClassifiers
                .Select(f => Observable.FromAsync(() => f.ClassifyAsync(fields, workspaceID)).Retry(2)) // retry each function once
                .ToObservable()
                .Merge(3) // run up to 3 at the same time`
                .Synchronize() // make sure that we aggregate one result at the time
                .Aggregate(fields.ToDictionary(x => x.Name, x => new FieldClassificationResult(x)
                    {
                        ClassificationLevel = ClassificationLevel.AutoMap
                    }), (accumulator, classifierResults) =>
                    {
                        foreach (FieldClassificationResult classifierResult in classifierResults)
                        {
                            var field = accumulator[classifierResult.FieldInfo.Name];

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
    }
}