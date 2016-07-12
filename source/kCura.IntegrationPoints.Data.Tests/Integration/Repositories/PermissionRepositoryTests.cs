using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NUnit.Framework;
using Relativity.Services.Group;
using Relativity.Services.Permission;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	[Category("Integration Tests")]
	public class PermissionRepositoryTests : RelativityProviderTemplate
	{
		private PermissionRepository _permissionRepo;
		private int _groupId;
		private UserModel _user;
		private GroupPermissions _groupPermission;
		private IObjectTypeRepository _typeRepo;
		private Random _rand;

		private string _oldInstanceSettingValue;

		public PermissionRepositoryTests()
			: base("PermissionRepositoryTests", null)
		{
			_rand = new Random();
		}

		[TestFixtureSetUp]
		public new void SuiteSetup()
		{
			_oldInstanceSettingValue = InstanceSetting.Update("Relativity.Authentication", "AdminsCanSetPasswords", "True");
		}

		[TestFixtureTearDown]
		public new void SuiteTeardown()
		{
			if (_oldInstanceSettingValue != InstanceSetting.INSTANCE_SETTING_VALUE_UNCHANGED)
			{
				InstanceSetting.Update("Relativity.Authentication", "AdminsCanSetPasswords", _oldInstanceSettingValue);
			}
		}

		[SetUp]
		public void Setup()
		{
			_typeRepo = Container.Resolve<IObjectTypeRepository>();
			_permissionRepo = new PermissionRepository(Helper, SourceWorkspaceArtifactId);
			_groupId = Group.CreateGroup("krowten");
			_user = User.CreateUser("Gerron", "BadMan", $"gbadman{_rand.Next(int.MaxValue)}@kcura.com", new[] { _groupId });

			Helper.RelativityUserName = _user.EmailAddress;
			Helper.RelativityPassword = _user.Password;

			Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, _groupId);
			_groupPermission = kCura.IntegrationPoint.Tests.Core.Permission.GetGroupPermissions(SourceWorkspaceArtifactId, _groupId);
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
			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactId, _groupPermission);

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
			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactId, _groupPermission);

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
			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactId, _groupPermission);

			return _permissionRepo.UserCanEditDocuments();
		}

		[Test]
		public void UserHasPermissionToAccessWorkspace_DoHavePermission()
		{
			Assert.IsTrue(_permissionRepo.UserHasPermissionToAccessWorkspace());
		}

		[Test]
		[Ignore("Keep getting exception when trying to remove group from the workspace. Need to investigate.")]
		public void UserHasPermissionToAccessWorkspace_DoNotHavePermission()
		{
			// arrange
			GroupSelector selector = new GroupSelector()
			{
				EnabledGroups = new List<GroupRef>(),
				DisabledGroups = new List<GroupRef>()
				{
					new GroupRef(_groupId)
				}
			};
			kCura.IntegrationPoint.Tests.Core.Permission.RemoveAddWorkspaceGroup(SourceWorkspaceArtifactId, selector);

			Assert.IsFalse(_permissionRepo.UserHasPermissionToAccessWorkspace());
		}

		[TestCase(true, ExpectedResult = true)]
		[TestCase(false, ExpectedResult = false)]
		public bool UserHasArtifactTypePermission_Add(bool addSelected)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Integration Point");
			permission.AddSelected = addSelected;
			permission.ViewSelected = true;
			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactId, _groupPermission);
			return _permissionRepo.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Create);
		}

		[TestCase(true, ExpectedResult = true)]
		[TestCase(false, ExpectedResult = false)]
		public bool UserHasArtifactTypePermission_Edit(bool editSelected)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Job History");
			permission.EditSelected = editSelected;
			permission.ViewSelected = true;
			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactId, _groupPermission);
			return _permissionRepo.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.Edit);
		}

		[TestCase(true, ExpectedResult = true)]
		[TestCase(false, ExpectedResult = false)]
		public bool UserHasArtifactTypePermission_View(bool viewSelected)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Job History");
			permission.ViewSelected = viewSelected;
			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactId, _groupPermission);
			return _permissionRepo.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.View);
		}

		[TestCase(true, ExpectedResult = true)]
		[TestCase(false, ExpectedResult = false)]
		public bool UserHasArtifactTypePermissions_ArtifactId_OnePermission(bool viewSelected)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Job History");
			permission.ViewSelected = viewSelected;
			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactId, _groupPermission);
			int jobHistoryErrorTypeId = _typeRepo.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistory));

			return _permissionRepo.UserHasArtifactTypePermissions(jobHistoryErrorTypeId, new ArtifactPermission[] { ArtifactPermission.View });
		}

		[TestCase(true, false, ExpectedResult = false)]
		[TestCase(true, true, ExpectedResult = true)]
		[TestCase(false, false, ExpectedResult = false)]
		public bool UserHasArtifactTypePermissions_ArtifactId_MultiplePermission(bool viewSelected, bool editSelected)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Job History");
			permission.ViewSelected = viewSelected;
			permission.EditSelected = editSelected;

			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactId, _groupPermission);
			int jobHistoryErrorTypeId = _typeRepo.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistory));

			return _permissionRepo.UserHasArtifactTypePermissions(jobHistoryErrorTypeId, new ArtifactPermission[] { ArtifactPermission.View, ArtifactPermission.Edit });
		}

		[TestCase(true, false, ExpectedResult = false)]
		[TestCase(false, true, ExpectedResult = false)]
		[TestCase(true, true, ExpectedResult = true)]
		[TestCase(false, false, ExpectedResult = false)]
		public bool UserHasArtifactTypePermissions_ArtifactId_CheckSubSetOfThePermissions(bool addSelected, bool editSelected)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Job History");
			permission.AddSelected = addSelected;
			permission.EditSelected = editSelected;
			permission.ViewSelected = true;

			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactId, _groupPermission);
			int jobHistoryErrorTypeId = _typeRepo.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistory));

			return _permissionRepo.UserHasArtifactTypePermissions(jobHistoryErrorTypeId, new ArtifactPermission[] { ArtifactPermission.Edit, ArtifactPermission.Create });
		}

		[TestCase(true)]
		[TestCase(false)]
		public void UserHasArtifactInstancePermission_UsingAdmin(bool useAdmin)
		{
			// arrange
			Helper.RelativityUserName = SharedVariables.RelativityUserName;
			Helper.RelativityPassword = SharedVariables.RelativityPassword;

			IntegrationModel model = new IntegrationModel()
			{
				Destination = $"{{\"artifactTypeID\":10,\"CaseArtifactId\":{TargetWorkspaceArtifactId},\"Provider\":\"relativity\",\"DoNotUseFieldsMapCache\":true,\"ImportOverwriteMode\":\"AppendOnly\",\"importNativeFile\":\"false\",\"UseFolderPathInformation\":\"false\",\"ExtractedTextFieldContainsFilePath\":\"false\",\"ExtractedTextFileEncoding\":\"utf - 16\",\"CustodianManagerFieldContainsLink\":\"true\",\"FieldOverlayBehavior\":\"Use Field Settings\"}}",
				DestinationProvider = CaseContext.RsapiService.DestinationProviderLibrary.ReadAll().First().ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"UserHasArtifactInstancePermission - {DateTime.Today}",
				Map = "[]",
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler(),
			};
			model = CreateOrUpdateIntegrationPoint(model);

			if (useAdmin == false)
			{
				Group.RemoveGroupFromWorkspace(SourceWorkspaceArtifactId, _groupId);
				Helper.RelativityUserName = _user.EmailAddress;
				Helper.RelativityPassword = _user.Password;
			}
			Assert.AreEqual(useAdmin, _permissionRepo.UserHasArtifactInstancePermission(Core.Constants.IntegrationPoints.IntegrationPoint.ObjectTypeGuid, model.ArtifactID, ArtifactPermission.View));
		}
	}
}