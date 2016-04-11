using System;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IArtifactGuidRepository
	{
		void InsertArtifactGuidForArtifactId(int artifactId, Guid guid);
	}
}