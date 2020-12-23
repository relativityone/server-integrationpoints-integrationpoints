using System;
using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public interface IChoiceQuery
	{
		List<Choice> GetChoicesOnField(int workspaceArtifactId, Guid fieldGuid);
	}
}