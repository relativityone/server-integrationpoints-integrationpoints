using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers
{
	public abstract class DestinationWorkspaceFieldManagerBase
	{
		private string ErrorMassage { get; }
		protected readonly IRepositoryFactory RepositoryFactory;
		protected readonly string FieldName;
		protected readonly Guid FieldGuid;

		protected DestinationWorkspaceFieldManagerBase(IRepositoryFactory repositoryFactory, string fieldName, Guid fieldGuid, string errorMassage)
		{
			ErrorMassage = errorMassage;
			RepositoryFactory = repositoryFactory;
			FieldName = fieldName;
			FieldGuid = fieldGuid;
		}

		protected int CreateObjectType(int workspaceArtifactId,
			IRelativityProviderObjectRepository relativityObjectRepository,
			IArtifactGuidRepository artifactGuidRepository,
			int parentArtifactTypeId)
		{
			IObjectTypeRepository objectTypeRepository = RepositoryFactory.GetObjectTypeRepository(workspaceArtifactId);

			// Check workspace for instance of the object type GUID
			int descriptorArtifactTypeId;
			try
			{
				descriptorArtifactTypeId = objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(FieldGuid);
			}
			catch (TypeLoadException)
			{
				// GUID doesn't exist in the workspace, so we try to see if the field name exists and assign a GUID to the field
				int? objectTypeArtifactId =
					objectTypeRepository.RetrieveObjectTypeArtifactId(FieldName);

				objectTypeArtifactId = objectTypeArtifactId ?? relativityObjectRepository.CreateObjectType(parentArtifactTypeId);

				// Associate a GUID with the newly created or existing object type
				try
				{
					artifactGuidRepository.InsertArtifactGuidForArtifactId(objectTypeArtifactId.Value, FieldGuid);
				}
				catch (Exception e)
				{
					throw new Exception(ErrorMassage, e);
				}

				// Get descriptor artifact type id of the now existing object type
				descriptorArtifactTypeId = objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(FieldGuid);

				// Delete the tab if it exists (it should always exist since we're creating the object type one line above)
				DeleteObjectTypeTab(workspaceArtifactId, descriptorArtifactTypeId);
			}
			return descriptorArtifactTypeId;
		}

		private void DeleteObjectTypeTab(int workspaceArtifactId, int objectDescriptorArtifactTypeId)
		{
			ITabRepository tabRepository = RepositoryFactory.GetTabRepository(workspaceArtifactId);
			int? sourceWorkspaceTabId = tabRepository.RetrieveTabArtifactId(objectDescriptorArtifactTypeId, FieldName);
			if (sourceWorkspaceTabId.HasValue)
			{
				tabRepository.Delete(sourceWorkspaceTabId.Value);
			}
		}

		protected void CreateObjectFields(List<Guid> fieldGuids,
			IArtifactGuidRepository artifactGuidRepository,
			IRelativityProviderObjectRepository relativityObjectRepository,
			IExtendedFieldRepository extendedFieldRepository,
			int descriptorArtifactTypeId)
		{
			IDictionary<Guid, bool> objectTypeFields = artifactGuidRepository.GuidsExist(fieldGuids);
			IList<Guid> missingFieldGuids = objectTypeFields.Where(x => x.Value == false).Select(y => y.Key).ToList();
			if (missingFieldGuids.Any())
			{
				IDictionary<Guid, int> guidToArtifactId = new Dictionary<Guid, int>();
				IDictionary<Guid, FieldDefinition> fieldDefinitions = GetObjectFieldDefinitions();

				for (int index = 0; index < missingFieldGuids.Count; index++)
				{
					Guid missingGuid = missingFieldGuids[index];
					FieldDefinition definition = fieldDefinitions[missingGuid];
					int? artifactId = extendedFieldRepository.RetrieveField(definition.FieldName, descriptorArtifactTypeId, (int)definition.FieldType);
					if (artifactId.HasValue)
					{
						guidToArtifactId.Add(missingGuid, artifactId.Value);
						missingFieldGuids.Remove(missingGuid);
						index--;
					}
				}

				if (missingFieldGuids.Any())
				{
					IDictionary<Guid, int> missingGuidToArtifactId = relativityObjectRepository.CreateObjectTypeFields(descriptorArtifactTypeId, missingFieldGuids);
					guidToArtifactId = guidToArtifactId.Union(missingGuidToArtifactId).ToDictionary(k => k.Key, v => v.Value);
				}

				try
				{
					artifactGuidRepository.InsertArtifactGuidsForArtifactIds(guidToArtifactId);
				}
				catch (Exception e)
				{
					throw new Exception(ErrorMassage, e);
				}
			}
		}

		protected void CreateDocumentsFields(int sourceWorkspaceDescriptorArtifactTypeId,
			Guid documentFieldGuid,
			IArtifactGuidRepository artifactGuidRepository,
			IRelativityProviderObjectRepository relativityObjectRepository,
			IExtendedFieldRepository extendedFieldRepository)
		{
			bool sourceWorkspaceFieldOnDocumentExists = artifactGuidRepository.GuidExists(documentFieldGuid);
			if (!sourceWorkspaceFieldOnDocumentExists)
			{
				int? fieldArtifactId = extendedFieldRepository.RetrieveField(FieldName, (int)Relativity.Client.ArtifactType.Document, (int)Relativity.Client.FieldType.MultipleObject);

				int sourceWorkspaceFieldArtifactId = fieldArtifactId ?? relativityObjectRepository.CreateFieldOnDocument(sourceWorkspaceDescriptorArtifactTypeId);

				try
				{
					int? retrieveArtifactViewFieldId = extendedFieldRepository.RetrieveArtifactViewFieldId(sourceWorkspaceFieldArtifactId);
					if (!retrieveArtifactViewFieldId.HasValue)
					{
						throw new Exception(ErrorMassage);
					}

					// Set the filter type
					extendedFieldRepository.UpdateFilterType(retrieveArtifactViewFieldId.Value, "Popup");
					// Set the overlay behavior
					extendedFieldRepository.SetOverlayBehavior(sourceWorkspaceFieldArtifactId, true);
					// Set the field artifact guid
					artifactGuidRepository.InsertArtifactGuidForArtifactId(sourceWorkspaceFieldArtifactId, documentFieldGuid);
				}
				catch (Exception e)
				{
					throw new Exception(ErrorMassage, e);
				}
			}
		}

		protected abstract IDictionary<Guid, FieldDefinition> GetObjectFieldDefinitions();

		protected sealed class FieldDefinition
		{
			public string FieldName;
			public Relativity.Client.FieldType FieldType;
		}
	}
}