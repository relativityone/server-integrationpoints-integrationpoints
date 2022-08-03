using System;
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
            if (relativityObject == null)
            {
                return null;
            }

            return new WorkspaceDTO
            {
                ArtifactId = relativityObject.ArtifactID,
                Name = GetFieldValue<string, WorkspaceDTO>(relativityObject, WorkspaceFieldsConstants.NAME_FIELD)
            };
        }

        public static IEnumerable<WorkspaceDTO> ToWorkspaceDTOs(this IEnumerable<RelativityObject> relativityObjects) =>
            relativityObjects?.Select(ToWorkspaceDTO);

        public static SavedSearchDTO ToSavedSearchDTO(this RelativityObject relativityObject)
        {
            if (relativityObject == null)
            {
                return null;
            }

            if (relativityObject.ParentObject == null)
            {
                throw new ArgumentException($"{GetErrorMessageHeader<SavedSearchDTO>()} - missing '{nameof(RelativityObject.ParentObject)}' value");
            }

            return new SavedSearchDTO
            {
                ArtifactId = relativityObject.ArtifactID,
                ParentContainerId = relativityObject.ParentObject.ArtifactID,
                Name = GetFieldValue<string, SavedSearchDTO>(relativityObject, SavedSearchFieldsConstants.NAME_FIELD),
                Owner = GetFieldValue<string, SavedSearchDTO>(relativityObject, SavedSearchFieldsConstants.OWNER_FIELD)
            };
        }

        public static IEnumerable<SavedSearchDTO> ToSavedSearchDTOs(this IEnumerable<RelativityObject> relativityObjects) =>
            relativityObjects?.Select(ToSavedSearchDTO);

        public static FederatedInstanceDto ToFederatedInstanceDTO(this RelativityObject relativityObject)
        {
            if (relativityObject == null)
            {
                return null;
            }

            return new FederatedInstanceDto
            {
                ArtifactId = relativityObject.ArtifactID,
                Name = GetFieldValue<string, FederatedInstanceDto>(relativityObject, FederatedInstanceFieldsConstants.NAME_FIELD),
                InstanceUrl = GetFieldValue<string, FederatedInstanceDto>(relativityObject, FederatedInstanceFieldsConstants.INSTANCE_URL_FIELD)
            };
        }

        public static IEnumerable<FederatedInstanceDto> ToFederatedInstanceDTOs(this IEnumerable<RelativityObject> relativityObjects) =>
            relativityObjects?.Select(ToFederatedInstanceDTO);

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
            relativityObjects?.Select(ToArtifactDTODeprecated);

        /// <summary>
        /// It set <see cref="ArtifactDTO.ArtifactTypeId"/> to 0.
        /// This method works only for specific use cases
        /// and should not be used in any new code
        /// </summary>
        /// <param name="relativityObjectsTask"></param>
        /// <returns></returns>
        internal static async Task<ArtifactDTO[]> ToArtifactDTOsArrayAsyncDeprecated(this Task<List<RelativityObject>> relativityObjectsTask)
        {
            IEnumerable<RelativityObject> relativityObjects = await relativityObjectsTask.ConfigureAwait(false);

            return relativityObjects
                .ToArtifactDTOsDeprecated()
                ?.ToArray();
        }

        private static TValue GetFieldValue<TValue, TDestinationType>(RelativityObject relativityObject, string fieldName) where TValue : class
        {
            ValidateFieldValueWithingRelativityObject<TValue, TDestinationType>(relativityObject, fieldName);

            FieldValuePair fieldValuePair = relativityObject.FieldValues.Single(x => x.Field.Name == fieldName);
            return fieldValuePair.Value as TValue;
        }

        private static void ValidateFieldValueWithingRelativityObject<TValue, TDestinationType>(RelativityObject relativityObject, string fieldName) where TValue : class
        {
            if (relativityObject.FieldValues == null)
            {
                throw new ArgumentException($"{GetErrorMessageHeader<TDestinationType>()} - missing fields values");
            }

            IEnumerable<FieldValuePair> fieldsWithMatchingName = relativityObject.FieldValues.Where(x => x.Field.Name == fieldName);
            List<FieldValuePair> twoFirstMatchingFields = fieldsWithMatchingName.Take(2).ToList();
            if (!twoFirstMatchingFields.Any())
            {
                throw new ArgumentException($"{GetErrorMessageHeader<TDestinationType>()} - missing '{fieldName}' value");
            }

            if (twoFirstMatchingFields.Count > 1)
            {
                throw new ArgumentException($"{GetErrorMessageHeader<TDestinationType>()} - duplicated '{fieldName}' field");
            }

            FieldValuePair fieldValuePair = twoFirstMatchingFields.Single();

            if (fieldValuePair.Value != null && !(fieldValuePair.Value is TValue))
            {
                throw new ArgumentException($"{GetErrorMessageHeader<TDestinationType>()} - wrong '{fieldName}' type");
            }
        }

        private static string GetErrorMessageHeader<TDestinationType>()
        {
            return $"{nameof(RelativityObject)} does not represent valid {typeof(TDestinationType).Name}";
        }
    }
}
