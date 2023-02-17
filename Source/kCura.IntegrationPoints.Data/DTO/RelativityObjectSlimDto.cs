using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.DTO
{
    public class RelativityObjectSlimDto
    {
        public RelativityObjectSlimDto(int artifactID, IDictionary<string, object> fieldValues)
        {
            ArtifactID = artifactID;
            FieldValues = fieldValues;
        }

        public int ArtifactID { get; }

        public IDictionary<string, object> FieldValues { get; }
    }
}
