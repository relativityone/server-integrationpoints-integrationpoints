using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services
{
	public class ObjectTypeService
	{
		public const int NON_SYSTEM_FIELD_IDS = 1000000;

		private readonly IRsapiRdoQuery _rdoQuery;
		public ObjectTypeService(IRsapiRdoQuery rdoQuery)
		{
			_rdoQuery = rdoQuery;
		}

		public bool HasParent(int objectType)
		{
			var rdo = _rdoQuery.GetType(objectType);
			return rdo.ParentArtifactTypeID > NON_SYSTEM_FIELD_IDS;
		}

		public int GetObjectTypeID(string artifactTypeName)
		{
			return _rdoQuery.GetObjectTypeID(artifactTypeName);
		}

	}
}
