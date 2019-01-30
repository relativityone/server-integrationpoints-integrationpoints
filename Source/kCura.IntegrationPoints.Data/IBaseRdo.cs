using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Attributes;

namespace kCura.IntegrationPoints.Data
{
	public interface IBaseRdo
	{
		T GetField<T>(Guid fieldGuid);
		string GetFieldName(Guid fieldGuid);
		void SetField<T>(Guid fieldName, T fieldValue, bool markAsUpdated = true);
		int ArtifactId { get; set; }
		int? ParentArtifactId { get; set; }
		Dictionary<Guid, DynamicFieldAttribute> FieldMetadata { get; }
		DynamicObjectAttribute ObjectMetadata { get; }
	}
}
