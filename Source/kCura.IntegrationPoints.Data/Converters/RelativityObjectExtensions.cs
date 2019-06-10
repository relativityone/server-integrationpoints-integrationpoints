using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Converters
{
	internal static class RelativityObjectExtensions
	{
		public static WorkspaceDTO ToWorkspaceDTO(this RelativityObject relativityObject)
		{
			return new WorkspaceDTO
			{
				ArtifactId = relativityObject.ArtifactID,
				Name = (string)relativityObject.FieldValues[0].Value
			};
		}

		public static IEnumerable<WorkspaceDTO> ToWorkspaceDTOs(this IEnumerable<RelativityObject> relativityObjects) =>
			relativityObjects.Select(ToWorkspaceDTO);

		public static SavedSearchDTO ToSavedSearchDTO(this RelativityObject relativityObject)
		{
			return new SavedSearchDTO
			{
				ArtifactId = relativityObject.ArtifactID,
				ParentContainerId = relativityObject.ParentObject.ArtifactID,
				Name = relativityObject.FieldValues.FirstOrDefault(x => x.Field.Name == SavedSearchFieldsConstants.NAME_FIELD)?.Value as string,
				Owner = relativityObject.FieldValues.FirstOrDefault(x => x.Field.Name == SavedSearchFieldsConstants.OWNER_FIELD)?.Value as string
			};
		}

		public static IEnumerable<SavedSearchDTO> ToSavedSearchDTOs(this IEnumerable<RelativityObject> relativityObjects) =>
			relativityObjects.Select(ToSavedSearchDTO);

		public static FederatedInstanceDto ToFederatedInstanceDTO(this RelativityObject relativityObject)
		{
			return new FederatedInstanceDto
			{
				ArtifactId = relativityObject.ArtifactID,
				Name = relativityObject.FieldValues[0].Value as string,
				InstanceUrl = relativityObject.FieldValues[1].Value as string
			};
		}

		public static IEnumerable<FederatedInstanceDto> ToFederatedInstanceDTOs(this IEnumerable<RelativityObject> relativityObjects) =>
			relativityObjects.Select(ToFederatedInstanceDTO);

		/// <summary>
		/// It set <see cref="ArtifactDTO.ArtifactTypeId"/> to 0.
		/// This method works only for specific use cases
		/// and should not be used in any new code
		/// </summary>
		/// <param name="relativityObject"></param>
		/// <returns></returns>
		internal static ArtifactDTO ToArtifactDTODeprecated(this RelativityObject relativityObject)
		{
			const int artifactTypeID = 0;
			IEnumerable<ArtifactFieldDTO> fields =
				relativityObject.FieldValues.Select(x => x.ToArtifactFieldDTO());

			return new ArtifactDTO(
				relativityObject.ArtifactID,
				artifactTypeID,
				relativityObject.Name,
				fields);
		}

		/// <summary>
		/// It set <see cref="ArtifactDTO.ArtifactTypeId"/> to 0.
		/// This method works only for specific use cases
		/// and should not be used in any new code
		/// </summary>
		/// <param name="relativityObjects"></param>
		/// <returns></returns>
		internal static IEnumerable<ArtifactDTO> ToArtifactDTOsDeprecated(this IEnumerable<RelativityObject> relativityObjects) =>
			relativityObjects.Select(ToArtifactDTODeprecated);

		/// <summary>
		/// It set <see cref="ArtifactDTO.ArtifactTypeId"/> to 0.
		/// This method works only for specific use cases
		/// and should not be used in any new code
		/// </summary>
		/// <param name="relativityObjects"></param>
		/// <returns></returns>
		internal static async Task<ArtifactDTO[]> ToArtifactDTOsArrayAsyncDeprecated(this Task<List<RelativityObject>> relativityObjectsTask)
		{
			IEnumerable<RelativityObject> relativityObjects = await relativityObjectsTask.ConfigureAwait(false);

			return relativityObjects
				.ToArtifactDTOsDeprecated()
				.ToArray();
		}
	}
}
