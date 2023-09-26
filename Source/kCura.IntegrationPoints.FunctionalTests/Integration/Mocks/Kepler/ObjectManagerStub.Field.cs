using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;
using Match = System.Text.RegularExpressions.Match;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public partial class ObjectManagerStub
    {
        private void SetupDocumentFields()
        {
            IList<FieldFake> FieldsByObjectTypeFilter(QueryRequest request, IList<FieldFake> list, int start)
            {
                if (request.Condition == @"'Object Type Artifact Type Id' == OBJECT 10")
                {
                    return list.Where(x => x.ObjectTypeId == (int)ArtifactType.Document).ToList();
                }
                if (IsRDOFieldTypeCondition(request.Condition, out int objectTypeId))
                {
                    return list.Where(x => x.ObjectTypeId == objectTypeId).ToList();
                }
                if (HasFieldName(request) | HasObjectTypeRefWithGuid(request))
                {
                    List<FieldFake> fields = list.Where(x => x.ObjectTypeId == Const.FIXED_LENGTH_TEXT_TYPE_ARTIFACT_ID).Skip(start - 1).ToList();
                    return fields;
                }
                if (IsChoiceQuery(request))
                {
                    List<FieldFake> fields = list
                        .Where(x => x.Guid == IntegrationPointProfileFieldGuids.OverwriteFieldsGuid).ToList();
                    return fields;
                }

                return new List<FieldFake>();
            }

            Mock.Setup(x => x.QueryAsync(It.IsAny<int>(),
                    It.Is<QueryRequest>(r => IsFieldQuery(r)), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
            {
                QueryResult result = GetRelativityObjectsForRequest(x => x.Fields, FieldsByObjectTypeFilter, workspaceId, request, start, length);
                return Task.FromResult(result);
            }
            );

            Mock.Setup(x => x.QuerySlimAsync(It.IsAny<int>(),
                    It.Is<QueryRequest>(r => IsFieldQuery(r)), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
            {
                QueryResultSlim result = GetQuerySlimsForRequest(x => x.Fields, FieldsByObjectTypeFilter, workspaceId, request, start, length);
                return Task.FromResult(result);
            });
        }

        private bool IsRDOFieldTypeCondition(string condition, out int objectTypeId)
        {
            Match match = Regex.Match(condition, @"'Object Type Artifact Type ID' == OBJECT (\d+)", RegexOptions.IgnoreCase);

            if (match.Success && int.TryParse(match.Groups[1].Value, out int extractedArtifactId))
            {
                objectTypeId = extractedArtifactId;
                return true;
            }

            objectTypeId = -1;
            return false;
        }

        private bool IsFieldQuery(QueryRequest r)
        {
            return r.ObjectType.ArtifactTypeID == (int)ArtifactType.Field;
        }

        private bool HasFieldName(QueryRequest r)
        {
            return r.Fields.Count(x => x.Name == "*") > 0;
        }

        private bool HasObjectTypeRefWithGuid(QueryRequest r)
        {
            return r.ObjectType.Guid.HasValue;
        }

        private bool IsChoiceQuery(QueryRequest query)
        {
            Match imageMatch = Regex.Match(query.Condition, $"'Name' == '{DocumentFieldsConstants.HasImagesFieldName}'");
            return imageMatch.Success;
        }
    }
}
