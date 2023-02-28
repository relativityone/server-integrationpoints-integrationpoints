using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Domain.Tests.Models
{
    [TestFixture, Category("Unit")]
    public class SourceWorkspaceDTOTests
    {
        [TestCase]
        public void ItShouldSetUp_SourceWorkspaceArtifactId_FromFieldValuesList()
        {
            int sourceWorkspaceArtifactId = 4325;

            var fieldValues = new List<FieldValuePair>
            {
                new FieldValuePair
                {
                    Field = new Field {Name = Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME },
                    Value = sourceWorkspaceArtifactId
                }
            };

            var dto = new SourceWorkspaceDTO(1, fieldValues);

            Assert.AreEqual(sourceWorkspaceArtifactId, dto.SourceCaseArtifactId);
        }

        [TestCase]
        public void ItShouldSetUp_SourceWorkspaceName_FromFieldValuesList()
        {
            string sourceWorkspaceName = "test workspace";

            var fieldValues = new List<FieldValuePair>
            {
                new FieldValuePair
                {
                    Field = new Field {Name = Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME },
                    Value = sourceWorkspaceName
                }
            };

            var dto = new SourceWorkspaceDTO(1, fieldValues);

            Assert.AreEqual(sourceWorkspaceName, dto.SourceCaseName);
        }

        [TestCase]
        public void ItShouldSetUp_Name_FromFieldValuesList()
        {
            string name = "name!#@#$!";

            var fieldValues = new List<FieldValuePair>
            {
                new FieldValuePair
                {
                    Field = new Field {Name = Constants.SOURCEWORKSPACE_NAME_FIELD_NAME },
                    Value = name
                }
            };

            var dto = new SourceWorkspaceDTO(1, fieldValues);

            Assert.AreEqual(name, dto.Name);
        }

        [TestCase]
        public void ItShouldSetUp_InstanceName_FromFieldValuesList()
        {
            string sourceInstanceName = "source % Instance - name";

            var fieldValues = new List<FieldValuePair>
            {
                new FieldValuePair
                {
                    Field = new Field {Name = Constants.SOURCEWORKSPACE_INSTANCENAME_FIELD_NAME },
                    Value = sourceInstanceName
                }
            };

            var dto = new SourceWorkspaceDTO(1, fieldValues);

            Assert.AreEqual(sourceInstanceName, dto.SourceInstanceName);
        }

        [TestCase]
        public void ItShouldReturn_CorrectObjectTypeRef()
        {
            var dto = new SourceWorkspaceDTO();

            ObjectTypeRef objectType = dto.ObjectTypeRef;
            Assert.AreEqual(SourceWorkspaceDTO.ObjectTypeGuid, objectType.Guid);
        }

        [TestCase]
        public void ItShouldReturn_CorrectFieldRefValuePairs()
        {
            int sourceWorkspaceId = 56543;
            string sourceWorkspaceName = "SW Name";
            string name = "name!";
            string instanceName = "instance";

            var dto = new SourceWorkspaceDTO
            {
                SourceCaseArtifactId = sourceWorkspaceId,
                SourceCaseName = sourceWorkspaceName,
                Name = name,
                SourceInstanceName = instanceName
            };

            var fields = dto.FieldRefValuePairs;

            AssertValueExistInFieldsList(fields, Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME, sourceWorkspaceId);
            AssertValueExistInFieldsList(fields, Constants.SOURCEWORKSPACE_NAME_FIELD_NAME, name);
            AssertValueExistInFieldsList(fields, Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME, sourceWorkspaceName);
            AssertValueExistInFieldsList(fields, Constants.SOURCEWORKSPACE_INSTANCENAME_FIELD_NAME, instanceName);
        }

        private void AssertValueExistInFieldsList(List<FieldRefValuePair> fields, string fieldName, object expectedValue)
        {
            var field = fields.Single(x => x.Field.Name == fieldName);
            Assert.AreEqual(expectedValue, field.Value);
        }
    }
}
