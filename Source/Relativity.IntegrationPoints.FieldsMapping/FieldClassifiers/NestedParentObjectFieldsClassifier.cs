using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;

namespace Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers
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
        public async Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<FieldInfo> fields, int workspaceID)
        {
            List<FieldInfo> objectFields = fields
                .Where(x => (x.Type == FieldTypeName.SINGLE_OBJECT || x.Type == FieldTypeName.MULTIPLE_OBJECT) && x.AssociativeObjectType != null)
                .ToList();

            var fieldsWithAssociativeObjectType = new List<FieldInfo>();
            using (var fieldManager = _servicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
            {
                await objectFields.Select(x =>
                    { 
                        int artifactId = 0;
                        int.TryParse(x.FieldIdentifier, out artifactId);
                        return Observable.FromAsync(() => fieldManager.ReadAsync(workspaceID, artifactId));
                    })
                    .ToObservable()
                    .Merge(10)
                    .Where(x => x.AssociativeObjectType != null)
                    .Synchronize()
                    .Do(x => fieldsWithAssociativeObjectType.Add(
                        new FieldInfo(x.ArtifactID.ToString(), x.Name, x.FieldType.ToString(), x.Length)
                        {
                            IsIdentifier = x.IsIdentifier,
                            IsRequired = x.IsRequired,
                            OpenToAssociations = x.OpenToAssociations,
                            AssociativeObjectType = x.AssociativeObjectType.Name
                        }))
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
                        $"'Name' IN [{string.Join(",", fieldsWithAssociativeObjectType.Select(x => $"'{x.AssociativeObjectType}'"))}]",
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
                        objectType.Name == x.AssociativeObjectType &&
                        (int)objectType.FieldValues.First().Value != WorkspaceArtifactTypeID))
                    .Select(x => new FieldClassificationResult(x)
                    {
                        ClassificationReason = "Field has nested parent object type.",
                        ClassificationLevel = ClassificationLevel.ShowToUser
                    });

                return filteredOutFields;
            }
        }
    }
}