using System;
using Moq;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts.Entity;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public partial class ObjectManagerStub
    {
        private void SetupObjectType()
        {
            Mock.Setup(x => x.QuerySlimAsync(It.IsAny<int>(), It.Is<QueryRequest>(
                    q => IsObjectTypeQuery(q)), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new QueryResultSlim
                {
                    Objects = new List<RelativityObjectSlim>() {new RelativityObjectSlim()},
                    TotalCount = 1
                });

            Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(
                    q => IsObjectTypeQuery(q)), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new QueryResult
                {
                    Objects = new List<RelativityObject>()
                    {
                        new RelativityObject
                        {
                            Guids = new List<Guid>(),
                            FieldValues = new List<FieldValuePair>(),
                            ParentObject = new RelativityObjectRef(),
                            Name = ""
                        }
                    },
                    TotalCount = 1
                });

            Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(
                    q => IsObjectTypeQuery(q) && q.Condition == $"'DescriptorArtifactTypeID' IN [{Const.ArtifactTypesIds.ENTITY_TYPE_ARTIFACT_ID}]"), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new QueryResult
                {
                    Objects = new List<RelativityObject>()
                    {
                        new RelativityObject
                        {
                            Guids = new List<Guid>() { ObjectTypeGuids.Entity },
                            FieldValues = new List<FieldValuePair>(),
                            ParentObject = new RelativityObjectRef(),
                            Name = ""
                        }
                    },
                    TotalCount = 1
                });
        }

        private bool IsObjectTypeQuery(QueryRequest query)
        {
            return query.ObjectType.ArtifactTypeID == (int) ArtifactType.ObjectType;
        }
    }
}