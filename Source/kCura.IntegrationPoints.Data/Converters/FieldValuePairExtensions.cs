using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Converters
{
	internal static class FieldValuePairExtensions
	{
		public static ArtifactFieldDTO ToArtifactFieldDTO(this FieldValuePair fieldValuePair)
		{
			return new ArtifactFieldDTO
			{
				Name = fieldValuePair.Field.Name,
				ArtifactId = fieldValuePair.Field.ArtifactID,
				FieldType = fieldValuePair.Field.FieldType.ToString(),
				Value = fieldValuePair.Value
			};
		}
	}
}
