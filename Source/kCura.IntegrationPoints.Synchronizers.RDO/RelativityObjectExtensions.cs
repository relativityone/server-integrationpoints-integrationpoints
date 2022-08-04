using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public static class RelativityObjectExtensions
    {
        public static bool FixIdentifierField(this RelativityObject field)
        {
            const string isIdentifierFieldName = "Is Identifier";
            bool isIdentifier = false;

            if (field.FieldValuePairExists(isIdentifierFieldName))
            {
                object value = field[isIdentifierFieldName].Value;
                isIdentifier = value is bool ? (bool)value : false;

                if (isIdentifier)
                {
                    field.Name += " [Object Identifier]";
                }
            }

            return isIdentifier;
        }
    }
}