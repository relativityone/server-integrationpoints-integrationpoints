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
		

		public WorkspaceTest CreateWorkspace(int? workspaceArtifactId = null)
		{
			WorkspaceTest workspace = new WorkspaceTest(_serializer, workspaceArtifactId);

			Relativity.Workspaces.Add(workspace);
			
			workspace.Folders.Add(new FolderTest
			{
				Name = workspace.Name
			});

			workspace.Fields.Add(new FieldTest
			{
				ObjectTypeId = (int)ArtifactType.Document,
				IsIdentifier = true,
				Name = "Control Number"
			});
			workspace.Fields.Add(new FieldTest
			{
				ObjectTypeId = Const.LDAP._ENTITY_TYPE_ARTIFACT_ID,
				Guid = new Guid(EntityFieldGuids.UniqueID),
				IsIdentifier = true,
				Name = "Unique ID"
			});
			workspace.Fields.Add(new FieldTest
			{
				ObjectTypeId = Const.LDAP._ENTITY_TYPE_ARTIFACT_ID,
				Guid = new Guid(EntityFieldGuids.FirstName),
				IsIdentifier = false,
				Name = "First Name"
			});
			workspace.Fields.Add(new FieldTest
			{
				ObjectTypeId = Const.LDAP._ENTITY_TYPE_ARTIFACT_ID,
				Guid = new Guid(EntityFieldGuids.LastName),
				IsIdentifier = false,
				Name = "Last Name"
			});
			workspace.Fields.Add(new FieldTest
			{
				ObjectTypeId = Const.LDAP._ENTITY_TYPE_ARTIFACT_ID,
				Guid = new Guid(EntityFieldGuids.FullName),
				IsIdentifier = false,
				Name = "Full Name"
			});
			workspace.Fields.Add(new FieldTest(Const.OVERWRITE_FIELD_ARTIFACT_ID)
			{
				ObjectTypeId = (int)ArtifactType.Document,
				Guid = IntegrationPointProfileFieldGuids.OverwriteFieldsGuid,
				IsIdentifier = false,
				Name = "Overwrite Fields"
			});
            workspace.Fields.Add(new FieldTest
			{
				ObjectTypeId = Const.LDAP._ENTITY_TYPE_ARTIFACT_ID,
				Guid = new Guid(EntityFieldGuids.Manager),
				IsIdentifier = false,
				Name = "Manager"
			});

			workspace.SavedSearches.Add(new SavedSearchTest
			{
				ParenObjectArtifactId = workspace.ArtifactId,
				Name = "All Documents"
			});

            FolderTest folder = workspace.Folders.First();
            IList<FieldTest> fields = workspace.Fields;

            workspace.Documents.Add(new DocumentTest(fields)
            {
                ParenObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = true,
                HasNatives = true,
                ImageCount = 1,
            });

			workspace.Documents.Add(new DocumentTest(fields)
            {
                ParenObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = true,
                HasNatives = true,
                ImageCount = 1,
            });

            workspace.Documents.Add(new DocumentTest(fields)
            {
                ParenObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = true,
                HasNatives = false,
                ImageCount = 10,
            });

			workspace.Documents.Add(new DocumentTest(fields)
			{
				ParenObjectArtifactId = folder.ArtifactId,
				FolderName = folder.Name,
				HasImages = true,
				HasNatives = false,
				ImageCount = 12,
			});

            workspace.Documents.Add(new DocumentTest(fields)
            {
                ParenObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = true,
                HasNatives = false,
                ImageCount = 15,
            });

			workspace.Documents.Add(new DocumentTest(fields)
            {
                ParenObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = false,
                HasNatives = true,
            });

            workspace.Documents.Add(new DocumentTest
            {
                ParenObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = false,
                HasNatives = true,
            });

            workspace.Documents.Add(new DocumentTest
            {
                ParenObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = false,
                HasNatives = true,
            });

workspace.Documents.Add(new DocumentTest()
            {
                ParenObjectArtifactId = folder.ArtifactId,
                FolderName = folder.Name,
                HasImages = false,
                HasNatives = false,
            });

            workspace.Productions.Add(new ProductionTest()
            {
                ParenObjectArtifactId = folder.ArtifactId,
                HasImages = false,
                HasNatives = false,
            });

			return workspace;
		}

		public WorkspaceTest CreateWorkspaceWithIntegrationPointsApp(int? workspaceArtifactId)
		{
			WorkspaceTest workspace = CreateWorkspace(workspaceArtifactId);

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
	}
}
