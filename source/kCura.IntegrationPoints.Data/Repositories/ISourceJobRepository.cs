using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Responsible for handling Source Job rdos and their functionality
	/// </summary>
	public interface ISourceJobRepository : IRelativityProviderObjectRepository
	{
		/// <summary>
		/// Creates an instance of the Source Job rdo
		/// </summary>
		/// <param name="sourceJobArtifactTypeId">The artifact type id of the Source Job</param>
		/// <param name="sourceJobDto">The Source Job to create</param>
		/// <returns>The artifact id of the newly created rdo</returns>
		int Create(int sourceJobArtifactTypeId, SourceJobDTO sourceJobDto);
	}
}