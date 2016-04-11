using System;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RsapiObjectTypeRepository : IObjectTypeRepository
	{
		private readonly IRSAPIClient _rsapiClient;

		public RsapiObjectTypeRepository(IRSAPIClient rsapiClient)
		{
			_rsapiClient = rsapiClient;
		}

		public int? RetrieveObjectTypeDescriptorArtifactTypeId(Guid objectTypeGuid)
		{
			var objectType = new ObjectType(objectTypeGuid) { Fields = FieldValue.AllFields };
			ResultSet<ObjectType> resultSet = _rsapiClient.Repositories.ObjectType.Read(new[] { objectType });

			int? objectTypeArtifactId = null;
			if (resultSet.Success && resultSet.Results.Any())
			{
				objectTypeArtifactId = resultSet.Results.First().Artifact.DescriptorArtifactTypeID;
			}

			return objectTypeArtifactId;
		}

		public void Delete(int artifactId)
		{
			var objectType = new ObjectType(artifactId);
			_rsapiClient.Repositories.ObjectType.Delete(new[] { objectType });
		}
	}
}