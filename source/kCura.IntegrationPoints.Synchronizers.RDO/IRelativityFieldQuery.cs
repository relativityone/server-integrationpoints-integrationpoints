using System.Collections.Generic;
using Artifact = kCura.Relativity.Client.Artifact;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public interface IRelativityFieldQuery
	{
		List<Artifact> GetFieldsForRdo(int rdoTypeId);
		List<Artifact> GetAllFields(int rdoTypeId);
	}
}
