using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IArtifactGuidRepository
	{
		void InsertArtifactGuidForArtifactId(int artifactId, Guid guid);
		void InsertArtifactGuidsForArtifactIds(IDictionary<Guid, int> guidToIdDictionary);
		IDictionary<Guid, bool> GuidsExist(IEnumerable<Guid> guids);
		bool GuidExists(Guid guid);
	}
}