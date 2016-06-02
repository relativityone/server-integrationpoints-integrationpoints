using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NUnit.Framework;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using Relativity.Services.Group;
using Relativity.Services.Permission;
using Group = kCura.IntegrationPoint.Tests.Core.Group;
using Permission = kCura.IntegrationPoint.Tests.Core.Permission;
using User = kCura.IntegrationPoint.Tests.Core.User;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	[Explicit]
	[Category("Integration Tests")]
	public class PermissionRepositoryTests : WorkspaceDependentTemplate
	{
		private PermissionRepository _permissionRepo;
		private int _groupId;
		private UserModel _user;
		private GroupPermissions _groupPermission;

		public PermissionRepositoryTests()
			: base("PermissionRepositoryTests", null)
		{
		}

		[SetUp]
		public void Setup()
		{

			_permissionRepo = new PermissionRepository(Helper, SourceWorkspaceArtifactId);
			_groupId = Group.CreateGroup("krowten");
			_user = User.CreateUser("Gerron", "BadMan", "gbadman@kcura.com", new[] {_groupId});

			Helper.RelativityUserName = _user.EmailAddress;
			Helper.RelativityPassword = _user.Password;

			Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, _groupId);
			_groupPermission = Permission.GetGroupPermissions(SourceWorkspaceArtifactId, _groupId);
		}

		[TearDown]
		public void Teardown()
		{
			User.DeleteUser(_user.ArtifactId);
			Group.DeleteGroup(_groupId);
		}

		[TestCase(true, true, ExpectedResult = true)]
		[TestCase(true, false, ExpectedResult = false)]
		[TestCase(false, false, ExpectedResult = false)]
		[TestCase(false, true, ExpectedResult = true)]
		public bool UserCanExport(bool isEditable, bool isSelected)
		{
			// arrange
			GenericPermission permission = _groupPermission.AdminPermissions.FindPermission("Allow Export");
			permission.Editable = isEditable;
			permission.Selected = isSelected;
			Permission.SavePermission(SourceWorkspaceArtifactId, _groupPermission);

			return _permissionRepo.UserCanExport();
		}

		[TestCase(true, true, ExpectedResult = true)]
		[TestCase(true, false, ExpectedResult = false)]
		[TestCase(false, false, ExpectedResult = false)]
		[TestCase(false, true, ExpectedResult = true)]
		public bool UserCanImport(bool isEditable, bool isSelected)
		{
			GenericPermission permission = _groupPermission.AdminPermissions.FindPermission("Allow Import");
			permission.Editable = isEditable;
			permission.Selected = isSelected;
			Permission.SavePermission(SourceWorkspaceArtifactId, _groupPermission);

			return _permissionRepo.UserCanImport();
		}

		// NOTE : ObjectPermissions Document must have View permissions when adding Add permissions.
		// NOTE : ObjectPermissions Document must have Edit permissions when adding Delete permissions.
		// NOTE : Document must have Edit permissions when adding Delete permissions.

		[TestCase(true, false, false, true, ExpectedResult = false)]
		[TestCase(false, false, true, true, ExpectedResult = true)]
		[TestCase(false, false, false, true, ExpectedResult = false)]
		[TestCase(true, false, true, true, ExpectedResult = true)]
		[TestCase(false, true, true, true, ExpectedResult = true)]
		[TestCase(true, true, true, true, ExpectedResult = true)]
		public bool UserCanEditDocuments(bool addSelected, bool deleteSelected, bool editSelected, bool viewSelected)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Document");
			permission.AddSelected = addSelected;
			permission.DeleteSelected = deleteSelected;
			permission.EditSelected = editSelected;
			permission.ViewSelected = viewSelected;
			Permission.SavePermission(SourceWorkspaceArtifactId, _groupPermission);

			return _permissionRepo.UserCanEditDocuments();
		}

		[Test]
		public void UserHasPermissionToAccessWorkspace_DoHavePermission()
		{
			IISReset();
			Assert.IsTrue(_permissionRepo.UserHasPermissionToAccessWorkspace());
		}

		[Test]
		public void UserHasPermissionToAccessWorkspace_DoNotHavePermission()
		{
			// arrange
			IISReset();
			GroupSelector selector = new GroupSelector()
			{
				EnabledGroups = new List<GroupRef>(),
				DisabledGroups = new List<GroupRef>()
				{
					new GroupRef(_groupId)
				}
			};
			Permission.RemoveAddWorkspaceGroup(SourceWorkspaceArtifactId, selector); 

			Assert.IsFalse(_permissionRepo.UserHasPermissionToAccessWorkspace());
		}

		[TestCase(true, ExpectedResult = true)]
		[TestCase(false, ExpectedResult = false)]
		public bool UserHasArtifactTypePermission_Add(bool addSelected)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Integration Point");
			permission.AddSelected = addSelected;
			permission.ViewSelected = true;
			Permission.SavePermission(SourceWorkspaceArtifactId, _groupPermission);
			return _permissionRepo.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Create);
		}

		[TestCase(true, ExpectedResult = true)]
		[TestCase(false, ExpectedResult = false)]
		public bool UserHasArtifactTypePermission_Edit(bool editSelected)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Job History");
			permission.EditSelected = editSelected;
			permission.ViewSelected = true;
			Permission.SavePermission(SourceWorkspaceArtifactId, _groupPermission);
			return _permissionRepo.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.Edit);
		}

		[TestCase(true, ExpectedResult = true)]
		[TestCase(false, ExpectedResult = false)]
		public bool UserHasArtifactTypePermission_View(bool viewSelected)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Job History");
			permission.ViewSelected = viewSelected;
			Permission.SavePermission(SourceWorkspaceArtifactId, _groupPermission);
			return _permissionRepo.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.View);
		}



	}
}