using System;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects.DataContracts;
using kCura.IntegrationPoints.Data.UtilityDTO;
using ArtifactType = Relativity.ArtifactType;

namespace kCura.IntegrationPoints.UITests.Configuration.Models
{
	public class FieldObject
	{
        public int ArtifactID { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
        public string Keywords { get; set; }
		public bool IsIdentifier { get; set; }
        public bool OpenToAssociations { get; set; }
        public int Length { get; set; }
        public string DisplayType => Type.Equals(TestConstants.FieldTypeNames.FIXED_LENGTH_TEXT) ? $"{Type}({Length})" : Type;
        public string DisplayName => IsIdentifier ? $"{Name} [Object Identifier]" : $"{Name} [{DisplayType}]";
        
        public FieldObject(RelativityObject serializedObject)
        {
            ArtifactID = serializedObject.ArtifactID;
            Name = Fields.GetFieldValueStringByFieldName(serializedObject, "Name");
            Type = Fields.GetFieldValueStringByFieldName(serializedObject, "Field Type");
            Length = Fields.GetFieldObjectLength(serializedObject);
            Keywords = Fields.GetFieldValueStringByFieldName(serializedObject, "Keywords");
            IsIdentifier = Fields.GetFieldValueBoolByFieldName(serializedObject, "Is Identifier");
            OpenToAssociations = Fields.GetFieldValueBoolByFieldName(serializedObject, "Open To Associations");
        }

        public static string GetRandomName(string fieldName)
        {
            const int nameMaxLength = 49;
            string randomName = $"{fieldName}" + Guid.NewGuid();
            return randomName.Substring(0, randomName.Length <= nameMaxLength ? randomName.Length : nameMaxLength);
        }

        public static async Task CreateFixedLengthFieldsWithSpecialCharactersAsync(int workspaceID, IFieldManager fieldManager)
        {
            char[] specialCharacters = @"!@#$%^&*()-_+= {}|\/;'<>,.?~`".ToCharArray();
            for (int i = 0; i < specialCharacters.Length; i++)
            {
                char special = specialCharacters[i];
                string generatedFieldName = $"aaaaa{special}{i}";
                var fixedLengthTextFieldRequest = new FixedLengthFieldRequest
                {
                    ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = (int)ArtifactType.Document },
                    Name = $"{generatedFieldName} FLT",
                    Length = 255
                };

                var longTextFieldRequest = new LongTextFieldRequest
                {
                    ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = (int)ArtifactType.Document },
                    Name = $"{generatedFieldName} LTF"
                };

                await fieldManager.CreateLongTextFieldAsync(workspaceID, longTextFieldRequest).ConfigureAwait(false);
                await fieldManager.CreateFixedLengthFieldAsync(workspaceID, fixedLengthTextFieldRequest).ConfigureAwait(false);
            }
            Guid randomNumber = Guid.NewGuid();
            var longTextRandomNameFieldRequest = new LongTextFieldRequest
            {
                ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = (int)ArtifactType.Document },
                Name = $"{randomNumber} LTF"
            };
            await fieldManager.CreateLongTextFieldAsync(workspaceID, longTextRandomNameFieldRequest).ConfigureAwait(false);
        }


        public static async Task<FieldObject> GetFieldObjectFromWorkspaceAsync(string fieldName, TestContext workspaceContext)
        {
            QueryRequest fieldsRequest = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
                Condition = $"'Object Type Artifact Type ID' == {(int)ArtifactType.Document} AND 'Name' == '{fieldName}'",
                Fields = new[]
                {
                    new FieldRef {Name = "Name"},
                    new FieldRef {Name = "Field Type"},
                    new FieldRef {Name = "Length"},
                    new FieldRef {Name = "Keywords"},
                    new FieldRef {Name = "Is Identifier"},
                    new FieldRef {Name = "Open To Associations"}
                }
            };

            ResultSet<RelativityObject> foundField =
                await workspaceContext.ObjectManager.QueryAsync(fieldsRequest, 0, 1).ConfigureAwait(false);
            RelativityObject firstFoundField = foundField.Items.First();

            return new FieldObject(firstFoundField);
        }

        public static async Task RenameFieldAsync(string fieldName, string newFieldName, TestContext workspaceContext, IFieldManager workspaceFieldManager)
        {
	        FieldObject fieldToBeChanged = await GetFieldObjectFromWorkspaceAsync(fieldName, workspaceContext).ConfigureAwait(false);
            var fixedLengthTextFieldUpdateRequest = new FixedLengthFieldRequest
            {
                ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = (int)ArtifactType.Document },
                Name = $"{newFieldName}",
                Length = fieldToBeChanged.Length
            };
            await workspaceFieldManager
                .UpdateFixedLengthFieldAsync(workspaceContext.GetWorkspaceId(), fieldToBeChanged.ArtifactID,
                    fixedLengthTextFieldUpdateRequest).ConfigureAwait(false);
        }
    }
}

