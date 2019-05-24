﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.Services.Group;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Permission;
using User = kCura.IntegrationPoint.Tests.Core.User;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	public class PermissionRepositoryTests : RelativityProviderTemplate
	{
		private PermissionRepository _permissionRepo;
		private int _groupId;
		private UserModel _user;
		private GroupPermissions _groupPermission;
		private IObjectTypeRepository _typeRepo;
		private string _originalHelperUsername;
		private string _originalHelperPassword;
		private readonly Random _rand;

		public PermissionRepositoryTests() : base("PermissionRepositoryTests", null)
		{
			_rand = new Random();
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			InstanceSetting.UpsertAndReturnOldValueIfExists("Relativity.Authentication", "AdminsCanSetPasswords", "True");
		}

		public override void TestSetup()
		{
			_typeRepo = Container.Resolve<IObjectTypeRepository>();
			_permissionRepo = new PermissionRepository(Helper, SourceWorkspaceArtifactID);
			_groupId = Group.CreateGroup("krowten");
			_user = User.CreateUser("Gerron", "BadMan", $"gbadman{_rand.Next(int.MaxValue)}@relativity.com", new[] { _groupId });

			_originalHelperUsername = Helper.RelativityUserName;
			_originalHelperPassword = Helper.RelativityPassword;

			Helper.RelativityUserName = _user.EmailAddress;
			Helper.RelativityPassword = _user.Password;

			Group.AddGroupToWorkspace(SourceWorkspaceArtifactID, _groupId);
			_groupPermission = kCura.IntegrationPoint.Tests.Core.Permission.GetGroupPermissions(SourceWorkspaceArtifactID, _groupId);
		}

		public override void TestTeardown()
		{
			User.DeleteUser(_user.ArtifactID);
			Group.DeleteGroup(_groupId);

			Helper.RelativityUserName = _originalHelperUsername;
			Helper.RelativityPassword = _originalHelperPassword;
		}

		[TestCase(true, true, true)]
		[TestCase(true, false, false)]
		[TestCase(false, false, false)]
		[TestCase(false, true, true)]
		public void UserCanExport(bool isEditable, bool isSelected, bool expectedResult)
		{
			// arrange
			GenericPermission permission = _groupPermission.AdminPermissions.FindPermission("Allow Export");
			permission.Editable = isEditable;
			permission.Selected = isSelected;
			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactID, _groupPermission);

			bool result = _permissionRepo.UserCanExport();
			Assert.AreEqual(expectedResult, result);
		}

		[TestCase(true, true, true)]
		[TestCase(true, false, false)]
		[TestCase(false, false, false)]
		[TestCase(false, true, true)]
		public void UserCanImport(bool isEditable, bool isSelected, bool expectedResult)
		{
			GenericPermission permission = _groupPermission.AdminPermissions.FindPermission("Allow Import");
			permission.Editable = isEditable;
			permission.Selected = isSelected;
			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactID, _groupPermission);

			bool result = _permissionRepo.UserCanImport();
			Assert.AreEqual(expectedResult, result);
		}

		// NOTE : ObjectPermissions Document must have View permissions when adding Add permissions.
		// NOTE : ObjectPermissions Document must have Edit permissions when adding Delete permissions.
		// NOTE : Document must have Edit permissions when adding Delete permissions.

		[TestCase(true, false, false, true, false)]
		[TestCase(false, false, true, true, true)]
		[TestCase(false, false, false, true, false)]
		[TestCase(true, false, true, true, true)]
		[TestCase(false, true, true, true, true)]
		[TestCase(true, true, true, true, true)]
		public void UserCanEditDocuments(bool addSelected, bool deleteSelected, bool editSelected, bool viewSelected, bool expectedResult)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Document");
			permission.AddSelected = addSelected;
			permission.DeleteSelected = deleteSelected;
			permission.EditSelected = editSelected;
			permission.ViewSelected = viewSelected;
			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactID, _groupPermission);

			bool result = _permissionRepo.UserCanEditDocuments();
			Assert.AreEqual(expectedResult, result);
		}

		[Test]
		public void UserHasPermissionToAccessWorkspace_DoHavePermission()
		{
			Assert.IsTrue(_permissionRepo.UserHasPermissionToAccessWorkspace());
		}

		[Test]
		public void UserHasPermissionToAccessWorkspace_DoNotHavePermission()
		{
			// arrange
			var selector = new GroupSelector()
			{
				EnabledGroups = new List<GroupRef>(),
				DisabledGroups = new List<GroupRef>()
				{
					new GroupRef(_groupId)
				}
			};
			kCura.IntegrationPoint.Tests.Core.Permission.RemoveAddWorkspaceGroup(SourceWorkspaceArtifactID, selector);

			Assert.IsFalse(_permissionRepo.UserHasPermissionToAccessWorkspace());
		}

		[TestCase(true, true)]
		[TestCase(false, false)]
		public void UserHasArtifactTypePermission_Add(bool addSelected, bool expectedResult)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Integration Point");
			permission.AddSelected = addSelected;
			permission.ViewSelected = true;
			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactID, _groupPermission);

			bool result = _permissionRepo.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Create);
			Assert.AreEqual(expectedResult, result);
		}

		[TestCase(true, true)]
		[TestCase(false, false)]
		public void UserHasArtifactTypePermission_Edit(bool editSelected, bool expectedResult)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Job History");
			permission.EditSelected = editSelected;
			permission.ViewSelected = true;
			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactID, _groupPermission);

			bool result = _permissionRepo.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.Edit);
			Assert.AreEqual(expectedResult, result);
		}

		[TestCase(true, true)]
		[TestCase(false, false)]
		public void UserHasArtifactTypePermission_View(bool viewSelected, bool expectedResult)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Job History");
			permission.ViewSelected = viewSelected;
			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactID, _groupPermission);
			bool result = _permissionRepo.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.View);

			Assert.AreEqual(result, expectedResult);
		}

		[TestCase(true, true)]
		[TestCase(false, false)]
		public void UserHasArtifactTypePermissions_ArtifactId_OnePermission(bool viewSelected, bool expectedResult)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Job History");
			permission.ViewSelected = viewSelected;
			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactID, _groupPermission);
			int jobHistoryErrorTypeId = _typeRepo.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistory));

			bool result = _permissionRepo.UserHasArtifactTypePermissions(jobHistoryErrorTypeId, new[] { ArtifactPermission.View });
			Assert.AreEqual(expectedResult, result);
		}

		[TestCase(true, false, false)]
		[TestCase(true, true, true)]
		[TestCase(false, false, false)]
		public void UserHasArtifactTypePermissions_ArtifactId_MultiplePermission(bool viewSelected, bool editSelected, bool expectedResult)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Job History");
			permission.ViewSelected = viewSelected;
			permission.EditSelected = editSelected;

			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactID, _groupPermission);
			int jobHistoryErrorTypeId = _typeRepo.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistory));

			bool result = _permissionRepo.UserHasArtifactTypePermissions(jobHistoryErrorTypeId, new[] { ArtifactPermission.View, ArtifactPermission.Edit });
			Assert.AreEqual(expectedResult, result);
		}

		[TestCase(true, false, false)]
		[TestCase(false, true, false)]
		[TestCase(true, true, true)]
		[TestCase(false, false, false)]
		public void UserHasArtifactTypePermissions_ArtifactId_CheckSubSetOfThePermissions(bool addSelected, bool editSelected, bool expectedResult)
		{
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Job History");
			permission.AddSelected = addSelected;
			permission.EditSelected = editSelected;
			permission.ViewSelected = true;

			kCura.IntegrationPoint.Tests.Core.Permission.SavePermission(SourceWorkspaceArtifactID, _groupPermission);
			int jobHistoryErrorTypeId = _typeRepo.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistory));

			bool result = _permissionRepo.UserHasArtifactTypePermissions(jobHistoryErrorTypeId, new[] { ArtifactPermission.Edit, ArtifactPermission.Create });
			Assert.AreEqual(expectedResult, result);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void UserHasArtifactInstancePermission_UsingAdmin(bool useAdmin)
		{
			// arrange
			Helper.RelativityUserName = SharedVariables.RelativityUserName;
			Helper.RelativityPassword = SharedVariables.RelativityPassword;

			var model = new IntegrationPointModel()
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = CaseContext.RsapiService.RelativityObjectManager.Query<DestinationProvider>(new QueryRequest()).First().ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"UserHasArtifactInstancePermission - {DateTime.Today:yy-MM-dd HH-mm-ss}",
				Map = CreateDefaultFieldMap(),
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};
			model = CreateOrUpdateIntegrationPoint(model);

			if (useAdmin == false)
			{
				Group.RemoveGroupFromWorkspace(SourceWorkspaceArtifactID, _groupId);
				Helper.RelativityUserName = _user.EmailAddress;
				Helper.RelativityPassword = _user.Password;
			}
			Assert.AreEqual(useAdmin, _permissionRepo.UserHasArtifactInstancePermission(Core.Constants.ObjectTypeArtifactTypesGuid.IntegrationPoint, model.ArtifactID, ArtifactPermission.View));
		}
	}
}