using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.FieldsMapping.ImportApi;

namespace Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers
{
    public class NotSupportedByIAPIFieldsClassifier : IFieldsClassifier
    {
        private readonly IImportApiFacade _importApiFacade;
        private readonly int _artifactTypeId;

        public NotSupportedByIAPIFieldsClassifier(IImportApiFacade importApiFacade, int artifactTypeId)
        {
            _importApiFacade = importApiFacade;
            _artifactTypeId = artifactTypeId;
        }

        public Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<FieldInfo> fields, int workspaceID)
        {
            HashSet<string> fieldsSupportedByIAPI = new HashSet<string>(GetFieldsSupportedByIAPI(workspaceID));

            IEnumerable<FieldClassificationResult> filteredOutFields = fields
                .Where(field => !fieldsSupportedByIAPI.Contains(field.Name))
                .Select(x => new FieldClassificationResult(x)
                {
                    ClassificationLevel = ClassificationLevel.HideFromUser,
                    ClassificationReason = "Field not supported by IAPI."
                });

            return Task.FromResult(filteredOutFields);
        }

        private IEnumerable<string> GetFieldsSupportedByIAPI(int workspaceId)
        {
            IEnumerable<string> workspaceFields = _importApiFacade.GetWorkspaceFieldsNames(workspaceId, _artifactTypeId).Values;
            return workspaceFields;
        }
    }
}
