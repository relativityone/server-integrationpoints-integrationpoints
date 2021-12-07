﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
            IList<FieldTest> FieldsByObjectTypeFilter(QueryRequest request, IList<FieldTest> list)
            {
                if(request.Condition == @"'Object Type Artifact Type Id' == OBJECT 10")
                {
                    return list.Where(x => x.ObjectTypeId == (int)ArtifactType.Document).ToList();
                }
                if (IsRDOFieldTypeCondition(request.Condition, out int objectTypeId))
                {
                    return list.Where(x => x.ObjectTypeId == objectTypeId).ToList();
                }

                return new List<FieldTest>();
            }

            Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), 
                    It.Is<QueryRequest>(r => IsFieldQuery(r) & !HasFieldName(r)), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
            {
                QueryResult result = GetRelativityObjectsForRequest(x => x.Fields, FieldsByObjectTypeFilter, workspaceId, request, length);
                return Task.FromResult(result);
            }
            );

            Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), 
                    It.Is<QueryRequest>(r => IsFieldQuery(r) & HasFieldName(r)),
                    It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
            {
                QueryResult result = GetRelativityObjectsForRequest(x => x.Fields, (queryRequest, list) =>
                {
                    List<FieldTest> fields = list.Where(x => x.ObjectTypeId == Const.FIXED_LENGTH_TEXT_TYPE_ARTIFACT_ID).Skip(start - 1).ToList();
                    return fields;
                }, workspaceId, request, length);
                foreach (var relativityObject in result.Objects)
                {
                    relativityObject.Name = relativityObject.FieldValues[1].Value.ToString();
                }

                return Task.FromResult(result);
            }
            );

            Mock.Setup(x => x.QueryAsync(It.IsAny<int>(),
                    It.Is<QueryRequest>(r => HasObjectTypeRefWithGuid(r)),
                    It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
            {
                QueryResult result = GetRelativityObjectsForRequest(x => x.Fields, (queryRequest, list) =>
                {
                    List<FieldTest> fields = list.Where(x => x.ObjectTypeId == Const.FIXED_LENGTH_TEXT_TYPE_ARTIFACT_ID).Skip(start - 1).ToList();
                    return fields;
                }, workspaceId, request, length);
                foreach (var relativityObject in result.Objects)
                {
                    relativityObject.Name = relativityObject.FieldValues[1].Value.ToString();
                }

                return Task.FromResult(result);
            }
            );

            Mock.Setup(x => x.QuerySlimAsync(It.IsAny<int>(),
                    It.Is<QueryRequest>(r => IsFieldQuery(r)), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
            {
                QueryResultSlim result = GetQuerySlimsForRequest(x=>x.Fields, FieldsByObjectTypeFilter, workspaceId, request, length);
                return Task.FromResult(result);
            });

            
        }

        private bool IsRDOFieldTypeCondition(string condition, out int objectTypeId)
        {
            Match match = Regex.Match(condition, @"'Object Type Artifact Type ID' == (\d+)");

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
	        return r.ObjectType.ArtifactTypeID == (int) ArtifactType.Field;
        }

        private bool HasFieldName(QueryRequest r)
        {
            return r.Fields.Count(x => x.Name == "*") > 0;
        } 
        
        private bool HasObjectTypeRefWithGuid(QueryRequest r)
        {
            return r.ObjectType.Guid.HasValue;
        }
    }
}