using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.Services.Objects.DataContracts;
using System.Linq;
using FieldType = Relativity.IntegrationPoints.Contracts.Models.FieldType;

namespace Relativity.IntegrationPoints.FieldsMapping.Helpers
{
    public static class FieldConvert
    {
        public static FieldEntry ToFieldEntry(FieldInfo fieldInfo)
        {
            return new FieldEntry
            {
                DisplayName = fieldInfo.Name,
                FieldIdentifier = fieldInfo.FieldIdentifier.ToString(),
                FieldType = FieldType.String,
                IsIdentifier = fieldInfo.IsIdentifier,
                IsRequired = fieldInfo.IsRequired,
                Type = fieldInfo.Type
            };
        }

        public static FieldInfo ToDocumentFieldInfo(RelativityObject fieldObject)
        {
            return new FieldInfo(
                fieldObject.ArtifactID.ToString(),
                fieldObject.Name,
                GetValueOrDefault<string>(fieldObject, "Field Type"),
                GetValueOrDefault<int>(fieldObject, "Length"))
            {
                IsIdentifier = GetValueOrDefault<bool>(fieldObject, "Is Identifier"),
                IsRequired = GetValueOrDefault<bool>(fieldObject, "Is Required"),
                OpenToAssociations = ValueExists(fieldObject, "Open To Associations") ? (bool?)GetValueOrDefault<bool>(fieldObject, "Open To Associations") : null,
                AssociativeObjectType = GetValueOrDefault<string>(fieldObject, "Associative Object Type"),
                Unicode = GetValueOrDefault<bool>(fieldObject, "Unicode"),
            };
        }

        private static bool ValueExists(RelativityObject fieldObject, string name)
        {
            FieldValuePair fieldValuePair = fieldObject.FieldValues.SingleOrDefault(x => x.Field.Name == name);
            return fieldValuePair != null;
        }

        private static T GetValueOrDefault<T>(RelativityObject fieldObject, string name)
        {
            FieldValuePair fieldValuePair = fieldObject.FieldValues.SingleOrDefault(x => x.Field.Name == name);
            return fieldValuePair?.Value is T value ? value : default(T);
        }
    }
}
