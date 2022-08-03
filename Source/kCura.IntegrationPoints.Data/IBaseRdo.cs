using System;

namespace kCura.IntegrationPoints.Data
{
    public interface IBaseRdo
    {
        T GetField<T>(Guid fieldGuid);
        string GetFieldName(Guid fieldGuid);
        void SetField<T>(Guid fieldGuid, T fieldValue, bool markAsUpdated = true);
        int ArtifactId { get; set; }
        int? ParentArtifactId { get; set; }
    }
}
