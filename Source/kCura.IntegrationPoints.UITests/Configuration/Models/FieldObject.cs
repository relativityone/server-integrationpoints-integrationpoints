using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Services.Objects.DataContracts;

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
    }
}

