using System;

namespace kCura.IntegrationPoints.Data.Attributes
{
    public class DynamicObjectAttribute : Attribute
    {

        public DynamicObjectAttribute(string artifactTypeName, string parentArtifactTypeName, string identiferFieldName, string guid)
        {
            this.ArtifactTypeName = artifactTypeName;
            this.ParentArtifactTypeName = parentArtifactTypeName;
            this.IdentifierFieldName = identiferFieldName;
            this.ArtifactTypeGuid = guid;
        }

        public string ArtifactTypeName { get; set; }
        public string ParentArtifactTypeName { get; set; }
        public string IdentifierFieldName { get; set; }
        public string ArtifactTypeGuid { get; set; }
        public string ParentArtifactTypeGuid { get; set; }

    }
}
