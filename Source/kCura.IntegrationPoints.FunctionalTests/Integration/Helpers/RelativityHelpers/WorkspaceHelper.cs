using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.RelativityHelpers
{
    public class WorkspaceHelper : RelativityHelperBase
    {
        private readonly ProxyMock _proxy;
        private readonly ISerializer _serializer;

        public WorkspaceHelper(RelativityInstanceTest relativity, ProxyMock proxy, ISerializer serializer) : base(relativity)
        {
            _proxy = proxy;
            _serializer = serializer;
        }

        public WorkspaceFake CreateWorkspace(int? workspaceArtifactId = null)
        {
            WorkspaceFake workspace = new WorkspaceFake(_serializer, workspaceArtifactId);

            Relativity.Workspaces.Add(workspace);

            workspace.Folders.Add(new FolderFake
            {
                Name = workspace.Name
            });

            workspace.ObjectTypes.Add(new ObjectTypeFake
                {
                    Name = Const.Document._DOCUMENT_NAME,
                    Guid = Const.Document._DOCUMENT_GUID,
                    ObjectType = Const.Document._DOCUMENT_NAME,
                    ObjectTypeArtifactTypeId = (int)ArtifactType.ObjectType,
                    ArtifactTypeId = (int)ArtifactType.Document
                }
            );
            workspace.Fields.Add(new FieldFake
            {
                ObjectTypeId = (int)ArtifactType.Document,
                IsIdentifier = true,
                Name = "Control Number"
            });
            workspace.Fields.Add(new FieldFake(Const.OVERWRITE_FIELD_ARTIFACT_ID)
            {
                ObjectTypeId = (int)ArtifactType.Document,
                Guid = IntegrationPointProfileFieldGuids.OverwriteFieldsGuid,
                IsIdentifier = false,
                Name = "Overwrite Fields"
            });

            workspace.ObjectTypes.Add(new ObjectTypeFake
                {
                    Name = Const.Entity._ENTITY_OBJECT_NAME,
                    Guid = Const.Entity._ENTITY_OBJECT_GUID,
                    ObjectType = Const.Entity._ENTITY_OBJECT_NAME,
                    ObjectTypeArtifactTypeId = (int)ArtifactType.ObjectType,
                    ArtifactTypeId = Const.Entity._ENTITY_TYPE_ARTIFACT_ID
                }
            );

            int _artifactTypeIdEntity = workspace.ObjectTypes.First(x => x.Name == Const.Entity._ENTITY_OBJECT_NAME).ArtifactTypeId;
            workspace.Fields.Add(new FieldFake
            {
                ObjectTypeId = _artifactTypeIdEntity,
                Guid = new Guid(EntityFieldGuids.UniqueID),
                IsIdentifier = true,
                Name = "Unique ID"
            });
            workspace.Fields.Add(new FieldFake
            {
                ObjectTypeId = _artifactTypeIdEntity,
                Guid = new Guid(EntityFieldGuids.FirstName),
                IsIdentifier = false,
                Name = Const.Entity._ENTITY_OBJECT_FIRST_NAME
            });
            workspace.Fields.Add(new FieldFake
            {
                ObjectTypeId = _artifactTypeIdEntity,
                Guid = new Guid(EntityFieldGuids.LastName),
                IsIdentifier = false,
                Name = Const.Entity._ENTITY_OBJECT_LAST_NAME
            });
            workspace.Fields.Add(new FieldFake
            {
                ObjectTypeId = _artifactTypeIdEntity,
                Guid = new Guid(EntityFieldGuids.FullName),
                IsIdentifier = false,
                Name = "Full Name"
            });
            workspace.Fields.Add(new FieldFake
            {
                ObjectTypeId = _artifactTypeIdEntity,
                Guid = new Guid(EntityFieldGuids.Manager),
                IsIdentifier = false,
                Name = "Manager"
            });

            workspace.SavedSearches.Add(new SavedSearchFake
            {
                ParentObjectArtifactId = workspace.ArtifactId,
                Name = "All Documents"
            });

            workspace.Views.Add(new ViewFake
            {
                ParentObjectArtifactId = workspace.ArtifactId,
                Name = "Default View"
            });

            CreateSavedSearchAndProduction(workspace, new SearchCriteria(true, false, true));
            CreateSavedSearchAndProduction(workspace, new SearchCriteria(true, false, false));
            CreateSavedSearchAndProduction(workspace, new SearchCriteria(false, true, true));
            CreateSavedSearchAndProduction(workspace, new SearchCriteria(false, true, false));
            CreateSavedSearchAndProduction(workspace, new SearchCriteria(false, false, true));

            FolderFake folder = workspace.Folders.First();
            IList<FieldFake> fields = workspace.Fields;

            workspace.Documents.Add(new DocumentFake(fields)
            {
                ParentObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = true,
                HasNatives = true,
                ImageCount = 1,
            });

            workspace.Documents.Add(new DocumentFake(fields)
            {
                ParentObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = true,
                HasNatives = true,
                ImageCount = 1,
            });

            workspace.Documents.Add(new DocumentFake(fields)
            {
                ParentObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = true,
                HasNatives = false,
                ImageCount = 10,
            });

            workspace.Documents.Add(new DocumentFake(fields)
            {
                ParentObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = true,
                HasNatives = false,
                ImageCount = 12,
            });

            workspace.Documents.Add(new DocumentFake(fields)
            {
                ParentObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = false,
                HasNatives = false,
                ImageCount = 0,
            });

            workspace.Documents.Add(new DocumentFake(fields)
            {
                ParentObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = false,
                HasNatives = false,
                ImageCount = 0,
            });

            workspace.Documents.Add(new DocumentFake
            {
                ParentObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = true,
                HasNatives = false,
                ImageCount = 32,
            });

            workspace.Documents.Add(new DocumentFake
            {
                ParentObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = true,
                HasNatives = false,
                ImageCount = 21,
            });

            workspace.Documents.Add(new DocumentFake(fields)
            {
                ParentObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = true,
                HasNatives = false,
                ImageCount = 15,
            });

            workspace.Documents.Add(new DocumentFake(fields)
            {
                ParentObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = false,
                HasNatives = true,
            });

            workspace.Documents.Add(new DocumentFake
            {
                ParentObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = false,
                HasNatives = true,
            });

            workspace.Documents.Add(new DocumentFake
            {
                ParentObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = false,
                HasNatives = true,
            });

            workspace.Documents.Add(new DocumentFake
            {
                ParentObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = false,
                HasNatives = false,
            });

            return workspace;
        }

        public WorkspaceFake CreateWorkspaceWithIntegrationPointsApp(int? workspaceArtifactId)
        {
            WorkspaceFake workspace = CreateWorkspace(workspaceArtifactId);

            workspace.Helpers.SourceProviderHelper.CreateLDAP();
            workspace.Helpers.SourceProviderHelper.CreateFTP();
            workspace.Helpers.SourceProviderHelper.CreateLoadFile();
            workspace.Helpers.SourceProviderHelper.CreateRelativity();

            workspace.Helpers.DestinationProviderHelper.CreateRelativityProvider();

            workspace.Helpers.DestinationProviderHelper.CreateLoadFile();

            workspace.Helpers.IntegrationPointTypeHelper.CreateImportType();
            workspace.Helpers.IntegrationPointTypeHelper.CreateExportType();

            return workspace;
        }

        public void RemoveWorkspace(int workspaceId)
        {
            foreach (var workspace in Relativity.Workspaces.Where(x => x.ArtifactId == workspaceId).ToArray())
            {
                Relativity.Workspaces.Remove(workspace);
            }
        }

        private void CreateSavedSearchAndProduction(WorkspaceFake workspace, SearchCriteria searchCriteria)
        {
            SavedSearchFake savedSearch = new SavedSearchFake(searchCriteria)
            {
                ParentObjectArtifactId = workspace.ArtifactId
            };

            workspace.SavedSearches.Add(savedSearch);
            workspace.Productions.Add(new ProductionFake(savedSearch.ArtifactId));
        }
    }
}
