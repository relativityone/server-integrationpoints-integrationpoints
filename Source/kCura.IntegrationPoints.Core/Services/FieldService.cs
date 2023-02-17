using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services
{
    public class FieldService : IFieldService
    {
        private const string _GET_TEXT_FIELDS_ERROR = "Integration Points failed during getting text fields";
        private const string _LONG_TEXT_QUERY = "'Field Type' == 'Long Text'";
        private const string _FIXED_LENGTH_TEXT_QUERY = "'Field Type' == 'Fixed-Length Text'";
        private readonly IRelativityObjectManagerFactory _objectManagerFactory;

        public FieldService(IRelativityObjectManagerFactory objectManagerFactory)
        {
            _objectManagerFactory = objectManagerFactory;
        }

        public IEnumerable<FieldEntry> GetAllTextFields(int workspaceId, int rdoTypeId)
        {
            return GetFields(workspaceId, rdoTypeId, $"{_FIXED_LENGTH_TEXT_QUERY} OR {_LONG_TEXT_QUERY}");
        }

        public IEnumerable<FieldEntry> GetLongTextFields(int workspaceId, int rdoTypeId)
        {
            return GetFields(workspaceId, rdoTypeId, _LONG_TEXT_QUERY);
        }

        private IEnumerable<FieldEntry> GetFields(int workspaceId, int rdoTypeId, string fieldTypeCondition)
        {
            try
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        ArtifactTypeID = (int)ArtifactType.Field
                    },
                    Condition = $"'Object Type Artifact Type ID' == {rdoTypeId} AND ({fieldTypeCondition})",
                    IncludeNameInQueryResult = true,
                    Sorts = new[]
                    {
                        new Sort
                        {
                            Direction = SortEnum.Ascending,
                            FieldIdentifier = new FieldRef
                            {
                                Name = "Name"
                            }
                        }
                    }
                };

                IRelativityObjectManager objectManager = _objectManagerFactory.CreateRelativityObjectManager(workspaceId);
                List<RelativityObject> fields = objectManager.QueryAsync(request).GetAwaiter().GetResult();
                List<FieldEntry> fieldEntries = fields.Select(x => new FieldEntry()
                {
                    DisplayName = x.Name,
                    FieldIdentifier = x.ArtifactID.ToString(),
                    IsRequired = false
                }).ToList();
                return fieldEntries;
            }
            catch (IntegrationPointsException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new IntegrationPointsException(_GET_TEXT_FIELDS_ERROR, ex);
            }
        }
    }
}
