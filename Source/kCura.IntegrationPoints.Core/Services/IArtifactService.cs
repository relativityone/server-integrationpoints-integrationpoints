using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IArtifactService
	{
		IEnumerable<Artifact> GetArtifacts(int workspaceArtifactId, string artifactTypeName);
	}
}