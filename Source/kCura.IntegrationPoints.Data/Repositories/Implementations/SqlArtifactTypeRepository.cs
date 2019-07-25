using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SqlArtifactTypeRepository : IArtifactTypeRepository
	{
		private const int _ARTIFACT_TYPE_ID_OBJECT_TYPE = 25;
		private const string _ARTIFACT_TYPE_ID_FIELD = "Artifact Type ID";
		private const string _NAME_FIELD = "Name";

		private readonly IRelativityObjectManager _objectManager;

		public SqlArtifactTypeRepository(IRelativityObjectManager objectManager)
		{
			_objectManager = objectManager;
		}

		public int GetArtifactTypeIDFromArtifactTypeName(string artifactTypeName)
		{
			var objectType = new ObjectTypeRef {ArtifactTypeID = _ARTIFACT_TYPE_ID_OBJECT_TYPE};
			var fields = new List<FieldRef> {new FieldRef {Name = _ARTIFACT_TYPE_ID_FIELD}};

			var queryRequest = new QueryRequest
			{
				ObjectType = objectType,
				Fields = fields,
				Condition = $"((('{_NAME_FIELD}' LIKE ['{artifactTypeName}'])))"
			};

			RelativityObject artifactTypeObject = _objectManager.Query(queryRequest).Single();
			int artifactTypeID = (int) artifactTypeObject[_ARTIFACT_TYPE_ID_FIELD].Value;

			return artifactTypeID;
		}
	}
}
