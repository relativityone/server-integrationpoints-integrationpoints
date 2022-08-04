using System;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Converters
{
    internal static class FieldValuePairExtensions
    {
        public static ArtifactFieldDTO ToArtifactFieldDTO(this FieldValuePair fieldValuePair)
        {
            if (fieldValuePair == null)
            {
                return null;
            }

            if (fieldValuePair.Field == null)
            {
                throw new ArgumentException(
                    $"{nameof(FieldValuePair)} is not in a valid state",
                    nameof(fieldValuePair));
            }

            return new ArtifactFieldDTO
            {
                Name = fieldValuePair.Field.Name,
                ArtifactId = fieldValuePair.Field.ArtifactID,
                FieldType = (FieldTypeHelper.FieldType) fieldValuePair.Field.FieldType,
                Value = fieldValuePair.Value
            };
        }
    }
}
