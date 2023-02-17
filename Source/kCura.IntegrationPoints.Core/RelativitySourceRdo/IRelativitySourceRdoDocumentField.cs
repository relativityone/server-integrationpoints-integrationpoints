using System;

namespace kCura.IntegrationPoints.Core.RelativitySourceRdo
{
    public interface IRelativitySourceRdoDocumentField
    {
        void CreateDocumentField(int workspaceArtifactId, Guid documentFieldGuid, string fieldName, int objectTypeDescriptorArtifactTypeId);
    }
}
