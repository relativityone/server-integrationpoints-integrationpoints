using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.Services.Group;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Permission;
using Permission = kCura.IntegrationPoint.Tests.Core.Permission;
using User = kCura.IntegrationPoint.Tests.Core.User;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	public class PermissionRepositoryTests : RelativityProviderTemplate
	{
		private PermissionRepository _userPermissionRepository;
		private PermissionRepository _adminPermissionRepository;
		private int _groupId;
		private UserModel _user;
		private GroupPermissions _groupPermission;
		private IObjectTypeRepository _objectTypeRepository;

		public PermissionRepositoryTests() : base("PermissionRepositoryTests", null)
		{ }

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			InstanceSetting.UpsertAndReturnOldValueIfExists("Relativity.Authentication", "AdminsCanSetPasswords", "True");
		}

		[SetUp]
		public async Task SetupAsync()
		{
			await CreateTestUserAndGroupAsync().ConfigureAwait(false);

			_objectTypeRepository = Container.Resolve<IObjectTypeRepository>();
			_adminPermissionRepository = new PermissionRepository(Helper, SourceWorkspaceArtifactId);
			ITestHelper helperForUser = Helper.CreateHelperForUser(_user.EmailAddress, _user.Password);
			_userPermissionRepository = new PermissionRepository(helperForUser, SourceWorkspaceArtifactId);
		}

		public override void TestTeardown()
		{
			User.DeleteUser(_user.ArtifactID);
			Group.DeleteGroup(_groupId);
		}

		[TestCase(true, true, true)]
		[TestCase(true, false, false)]
		[TestCase(false, false, false)]
		[TestCase(false, true, true)]
		public async Task UserCanExport(bool isEditable, bool isSelected, bool expectedResult)
		{
			// arrange
			GenericPermission permission = _groupPermission.AdminPermissions.FindPermission("Allow Export");
			permission.Editable = isEditable;
			permission.Selected = isSelected;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactId, _groupPermission).ConfigureAwait(false);

			// act
			bool result = _userPermissionRepository.UserCanExport();

			// assert
			Assert.AreEqual(expectedResult, result);
		}

		[TestCase(true, true, true)]
		[TestCase(true, false, false)]
		[TestCase(false, false, false)]
		[TestCase(false, true, true)]
		public async Task UserCanImport(bool isEditable, bool isSelected, bool expectedResult)
		{
			// arrange
			GenericPermission permission = _groupPermission.AdminPermissions.FindPermission("Allow Import");
			permission.Editable = isEditable;
			permission.Selected = isSelected;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactId, _groupPermission).ConfigureAwait(false);

			// act
			bool result = _userPermissionRepository.UserCanImport();

			// assert
			result.Should().Be(expectedResult);
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
		public async Task UserCanEditDocuments(bool addSelected, bool deleteSelected, bool editSelected, bool viewSelected, bool expectedResult)
		{
			// arrange
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission(ObjectTypes.Document);
			permission.AddSelected = addSelected;
			permission.DeleteSelected = deleteSelected;
			permission.EditSelected = editSelected;
			permission.ViewSelected = viewSelected;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactId, _groupPermission).ConfigureAwait(false);

			// act
			bool result = _userPermissionRepository.UserCanEditDocuments();

			// assert
			result.Should().Be(expectedResult);
		}

		[Test]
		public void UserHasPermissionToAccessWorkspace_DoHavePermission()
		{
			// act
			bool result = _userPermissionRepository.UserHasPermissionToAccessWorkspace();

			// assert
			result.Should().BeTrue();
		}

		[Test]
		public async Task UserHasPermissionToAccessWorkspace_DoNotHavePermission()
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
			await Permission.RemoveAddWorkspaceGroupAsync(SourceWorkspaceArtifactId, selector).ConfigureAwait(false);

			// act
			bool result = _userPermissionRepository.UserHasPermissionToAccessWorkspace();

			// assert
			result.Should().BeFalse();
		}

		[TestCase(true, true)]
		[TestCase(false, false)]
		public async Task UserHasArtifactTypePermission_Add(bool addSelected, bool expectedResult)
		{
			// arrange
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPoint);
			permission.AddSelected = addSelected;
			permission.ViewSelected = true;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactId, _groupPermission).ConfigureAwait(false);

			// act
			bool result = _userPermissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Create);

			// assert
			result.Should().Be(expectedResult);
		}

		[TestCase(true, true)]
		[TestCase(false, false)]
		public async Task UserHasArtifactTypePermission_Edit(bool editSelected, bool expectedResult)
		{
			// arrange
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Job History");
			permission.EditSelected = editSelected;
			permission.ViewSelected = true;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactId, _groupPermission).ConfigureAwait(false);

			// act
			bool result = _userPermissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.Edit);

			// assert
			result.Should().Be(expectedResult);
		}

		[TestCase(true, true)]
		[TestCase(false, false)]
		public async Task UserHasArtifactTypePermission_View(bool viewSelected, bool expectedResult)
		{
			// arrange
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission(ObjectTypes.JobHistory);
			permission.ViewSelected = viewSelected;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactId, _groupPermission).ConfigureAwait(false);

			// act
			bool result = _userPermissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.JobHistory), ArtifactPermission.View);

			// assert
			result.Should().Be(expectedResult);
		}

		[TestCase(true, true)]
		[TestCase(false, false)]
		public async Task UserHasArtifactTypePermissions_ArtifactId_OnePermission(bool viewSelected, bool expectedResult)
		{
			// arrange
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission(ObjectTypes.JobHistory);
			permission.ViewSelected = viewSelected;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactId, _groupPermission).ConfigureAwait(false);
			int jobHistoryErrorTypeId = _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistory));

			// act
			bool result = _userPermissionRepository.UserHasArtifactTypePermissions(jobHistoryErrorTypeId, new[] { ArtifactPermission.View });

			// assert
			result.Should().Be(expectedResult);
		}

		[TestCase(true, false, false)]
		[TestCase(true, true, true)]
		[TestCase(false, false, false)]
		public async Task UserHasArtifactTypePermissions_ArtifactId_MultiplePermission(bool viewSelected, bool editSelected, bool expectedResult)
		{
			// arrange
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission(ObjectTypes.JobHistory);
			permission.ViewSelected = viewSelected;
			permission.EditSelected = editSelected;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactId, _groupPermission).ConfigureAwait(false);
			int jobHistoryErrorTypeId = _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistory));

			// act
			bool result = _userPermissionRepository.UserHasArtifactTypePermissions(jobHistoryErrorTypeId, new[] { ArtifactPermission.View, ArtifactPermission.Edit });

			// assert
			result.Should().Be(expectedResult);
		}

		[TestCase(true, false, false)]
		[TestCase(false, true, false)]
		[TestCase(true, true, true)]
		[TestCase(false, false, false)]
		public async Task UserHasArtifactTypePermissions_ArtifactId_CheckSubSetOfThePermissions(bool hasAddPermission, bool hasEditPermission, bool expectedResult)
		{
			// arrange
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission(ObjectTypes.JobHistory);
			permission.AddSelected = hasAddPermission;
			permission.EditSelected = hasEditPermission;
			permission.ViewSelected = true;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactId, _groupPermission).ConfigureAwait(false);
			int jobHistoryErrorTypeId = _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistory));

			// act
			bool result = _userPermissionRepository.UserHasArtifactTypePermissions(jobHistoryErrorTypeId, new[] { ArtifactPermission.Edit, ArtifactPermission.Create });

			// assert
			result.Should().Be(expectedResult);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void UserHasArtifactInstancePermission_UsingAdmin(bool useAdmin)
		{
			// arrange
			IntegrationPointModel model = CreateNewIntegrationPoint();

			if (useAdmin == false)
			{
				Group.RemoveGroupFromWorkspace(SourceWorkspaceArtifactId, _groupId);
			}
			IPermissionRepository sut = useAdmin ? _adminPermissionRepository : _userPermissionRepository;

			// act
			bool result = sut.UserHasArtifactInstancePermission(
				Core.Constants.ObjectTypeArtifactTypesGuid.IntegrationPoint,
				model.ArtifactID,
				ArtifactPermission.View);

			// assert
			result.Should().Be(useAdmin);
		}

		private IntegrationPointModel CreateNewIntegrationPoint()
		{
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
			return model;
		}

		private async Task CreateTestUserAndGroupAsync()
		{
			await CreateTestGroupAsync();
			CreateTestUser();
		}

		private async Task CreateTestGroupAsync()
		{
			_groupId = Group.CreateGroup("krowten");
			Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, _groupId);
			_groupPermission = await Permission.GetGroupPermissionsAsync(SourceWorkspaceArtifactId, _groupId);
		}

		private void CreateTestUser()
		{
			var randomNumbersGenerator = new Random();
			string firstName = "Gerron";
			string lastName = "BadMan";
			string emailAddress = $"gbadman{randomNumbersGenerator.Next(int.MaxValue)}@relativity.com";

			_user = User.CreateUser(firstName, lastName, emailAddress, new[] { _groupId });
		}
	}
}