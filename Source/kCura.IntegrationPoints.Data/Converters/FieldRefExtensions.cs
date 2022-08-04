using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Field;

namespace kCura.IntegrationPoints.Data.Converters
{
    internal static class FieldRefExtensions
    {
        public static ArtifactFieldDTO ToArtifactFieldDTO(this FieldRef artifactDTO)
        {
            if (artifactDTO == null)
            {
                return null;
            }

            return new ArtifactFieldDTO
            {
                ArtifactId = artifactDTO.ArtifactID,
                Name = artifactDTO.Name
            };
        }

        public static IEnumerable<ArtifactFieldDTO> ToArtifactFieldDTOs(this IEnumerable<FieldRef> artifactDTOs) =>
            artifactDTOs?.Select(ToArtifactFieldDTO);
    }
}
