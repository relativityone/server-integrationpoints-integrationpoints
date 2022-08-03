using System;

namespace kCura.IntegrationPoints.Data.Attributes
{
    public class DynamicFieldAttribute : Attribute
    {

        public DynamicFieldAttribute(string fieldName, string fieldGuid, string type)
        {
            this.FieldName = fieldName;
            this.Type = type;
            this.FieldGuid = new Guid(fieldGuid);
        }

        public DynamicFieldAttribute(string fieldName, string fieldGuid, string type, int length)
        {
            this.FieldName = fieldName;
            this.Type = type;
            this.Length = length;
            this.FieldGuid = new Guid(fieldGuid);
        }

        public DynamicFieldAttribute(string fieldName, string fieldGuid, string type, Type choiceFieldStringEnum)
        {
            this.FieldName = fieldName;
            this.Type = type;
            this.ChoiceFieldStringEnum = choiceFieldStringEnum;
            this.FieldGuid = new Guid(fieldGuid);
        }

        public DynamicFieldAttribute(string fieldName, string fieldGuid, string type, string objectFieldArtifactType)
        {
            this.FieldName = fieldName;
            this.Type = type;
            this.ObjectFieldArtifactType = objectFieldArtifactType;
            this.FieldGuid = new Guid(fieldGuid);
        }

        public string FieldName { get; set; }
        public string Type { get; set; }
        public int Length { get; set; }
        public Type ChoiceFieldStringEnum { get; set; }
        public string ObjectFieldArtifactType { get; set; }
        public Guid FieldGuid { get; set; }

    }
}
