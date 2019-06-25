using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;

namespace kCura.IntegrationPoints.Data.Helpers
{
	public interface IMassUpdateHelper
	{
		Task UpdateArtifactsAsync(
			ICollection<int> artifactsToUpdate,
			FieldUpdateRequestDto[] fieldsToUpdate,
			IMassUpdateRepository massUpdateRepository);

		Task UpdateArtifactsAsync(
			IScratchTableRepository artifactsToUpdateRepository,
			FieldUpdateRequestDto[] fieldsToUpdate,
			IMassUpdateRepository massUpdateRepository);
	}
}
