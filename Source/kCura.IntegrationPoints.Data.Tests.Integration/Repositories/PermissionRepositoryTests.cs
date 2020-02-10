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
using Relativity.Testing.Identification;
using Permission = kCura.IntegrationPoint.Tests.Core.Permission;
using User = kCura.IntegrationPoint.Tests.Core.User;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
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

		public async override void SuiteSetup()
		{
			base.SuiteSetup();
			await InstanceSetting.CreateOrUpdateAsync("Relativity.Authentication", "AdminsCanSetPasswords", "True")
				.ConfigureAwait(false);
		}

		[SetUp]
		public async Task SetupAsync()
		{
			await CreateTestUserAndGroupAsync().ConfigureAwait(false);

			_objectTypeRepository = Container.Resolve<IObjectTypeRepository>();
			_adminPermissionRepository = new PermissionRepository(Helper, SourceWorkspaceArtifactID);
			ITestHelper helperForUser = Helper.CreateHelperForUser(_user.EmailAddress, _user.Password);
			_userPermissionRepository = new PermissionRepository(helperForUser, SourceWorkspaceArtifactID);
		}

		public override void TestTeardown()
		{
			User.DeleteUser(_user.ArtifactID);
			Group.DeleteGroup(_groupId);
		}

		[IdentifiedTestCase("e2919997-add5-4bd7-940f-2f5d549b5d14", true, true, true)]
		[IdentifiedTestCase("b70bccff-37ac-499a-b830-f9781af4d0c9", true, false, false)]
		[IdentifiedTestCase("08d41a90-eddd-41fe-b4b8-7d3e50a30120", false, false, false)]
		[IdentifiedTestCase("63d64a62-b3ab-40f3-8118-3d81f0a826d1", false, true, true)]
		public async Task UserCanExport(bool isEditable, bool isSelected, bool expectedResult)
		{
			// arrange
			GenericPermission permission = _groupPermission.AdminPermissions.FindPermission("Allow Export");
			permission.Editable = isEditable;
			permission.Selected = isSelected;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactID, _groupPermission).ConfigureAwait(false);

			// act
			bool result = _userPermissionRepository.UserCanExport();

			// assert
			Assert.AreEqual(expectedResult, result);
		}

		[IdentifiedTestCase("4c393691-1ecc-4ef0-ac6a-5828446beedb", true, true, true)]
		[IdentifiedTestCase("69eb081f-49bc-400d-b1a6-364f51ee4767", true, false, false)]
		[IdentifiedTestCase("71f97dd9-5131-420c-ab49-dcf495caaadb", false, false, false)]
		[IdentifiedTestCase("b31bbbcd-c1b4-4e81-9a5e-1bc3f14d6e99", false, true, true)]
		public async Task UserCanImport(bool isEditable, bool isSelected, bool expectedResult)
		{
			// arrange
			GenericPermission permission = _groupPermission.AdminPermissions.FindPermission("Allow Import");
			permission.Editable = isEditable;
			permission.Selected = isSelected;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactID, _groupPermission).ConfigureAwait(false);

			// act
			bool result = _userPermissionRepository.UserCanImport();

			// assert
			result.Should().Be(expectedResult);
		}

		// NOTE : ObjectPermissions Document must have View permissions when adding Add permissions.
		// NOTE : ObjectPermissions Document must have Edit permissions when adding Delete permissions.
		// NOTE : Document must have Edit permissions when adding Delete permissions.

		[IdentifiedTestCase("108e4798-b0fd-46a6-9ab7-012f4eb3872a", true, false, false, true, false)]
		[IdentifiedTestCase("bcde17a7-30a0-4e53-95e8-0a1d89d9ddda", false, false, true, true, true)]
		[IdentifiedTestCase("f8531865-e78f-4eae-bec7-88b1994a873f", false, false, false, true, false)]
		[IdentifiedTestCase("32c34e81-48f4-4be2-859f-b2a1fcf9213c", true, false, true, true, true)]
		[IdentifiedTestCase("1217892b-b802-4044-ba8d-b350798c979d", false, true, true, true, true)]
		[IdentifiedTestCase("3914795c-cd14-4026-8a71-d6218f7c9a55", true, true, true, true, true)]
		public async Task UserCanEditDocuments(bool addSelected, bool deleteSelected, bool editSelected, bool viewSelected, bool expectedResult)
		{
			// arrange
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission(ObjectTypes.Document);
			permission.AddSelected = addSelected;
			permission.DeleteSelected = deleteSelected;
			permission.EditSelected = editSelected;
			permission.ViewSelected = viewSelected;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactID, _groupPermission).ConfigureAwait(false);

			// act
			bool result = _userPermissionRepository.UserCanEditDocuments();

			// assert
			result.Should().Be(expectedResult);
		}

		[IdentifiedTest("2122633d-1541-4f23-b74d-16c78bc955a7")]
		public void UserHasPermissionToAccessWorkspace_DoHavePermission()
		{
			// act
			bool result = _userPermissionRepository.UserHasPermissionToAccessWorkspace();

			// assert
			result.Should().BeTrue();
		}

		[IdentifiedTest("55c432f6-85f0-42be-9c03-a9a230d284d9")]
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
			await Permission.RemoveAddWorkspaceGroupAsync(SourceWorkspaceArtifactID, selector).ConfigureAwait(false);

			// act
			bool result = _userPermissionRepository.UserHasPermissionToAccessWorkspace();

			// assert
			result.Should().BeFalse();
		}

		[IdentifiedTestCase("f2325773-fb6a-4a54-ae03-4ac9f40b30d3", true, true)]
		[IdentifiedTestCase("ffbafdd6-61c0-4a92-93cc-8b50089b7122", false, false)]
		public async Task UserHasArtifactTypePermission_Add(bool addSelected, bool expectedResult)
		{
			// arrange
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPoint);
			permission.AddSelected = addSelected;
			permission.ViewSelected = true;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactID, _groupPermission).ConfigureAwait(false);

			// act
			bool result = _userPermissionRepository.UserHasArtifactTypePermission(
				new Guid(ObjectTypeGuids.IntegrationPoint),
				ArtifactPermission.Create);

			// assert
			result.Should().Be(expectedResult);
		}

		[IdentifiedTestCase("d5e7edd1-d801-4db6-80f5-c07a74c44387", true, true)]
		[IdentifiedTestCase("2b3953d2-2482-4d78-a575-6a9d4b980de9", false, false)]
		public async Task UserHasArtifactTypePermission_Edit(bool editSelected, bool expectedResult)
		{
			// arrange
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission("Job History");
			permission.EditSelected = editSelected;
			permission.ViewSelected = true;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactID, _groupPermission).ConfigureAwait(false);

			// act
			bool result = _userPermissionRepository.UserHasArtifactTypePermission(
				new Guid(ObjectTypeGuids.JobHistory),
				ArtifactPermission.Edit);

			// assert
			result.Should().Be(expectedResult);
		}

		[IdentifiedTestCase("034ac850-02eb-447a-90b5-c2b841dcd0d7", true, true)]
		[IdentifiedTestCase("d2730cd5-fc65-4fe0-b5ab-b466a3b48585", false, false)]
		public async Task UserHasArtifactTypePermission_View(bool viewSelected, bool expectedResult)
		{
			// arrange
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission(ObjectTypes.JobHistory);
			permission.ViewSelected = viewSelected;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactID, _groupPermission).ConfigureAwait(false);

			// act
			bool result = _userPermissionRepository.UserHasArtifactTypePermission(
				new Guid(ObjectTypeGuids.JobHistory),
				ArtifactPermission.View);

			// assert
			result.Should().Be(expectedResult);
		}

		[IdentifiedTestCase("257a5499-6b11-4874-8ae7-8a917b06ccd4", true, true)]
		[IdentifiedTestCase("283419ac-77ce-41a0-a121-2c5e482a848c", false, false)]
		public async Task UserHasArtifactTypePermissions_ArtifactId_OnePermission(bool viewSelected, bool expectedResult)
		{
			// arrange
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission(ObjectTypes.JobHistory);
			permission.ViewSelected = viewSelected;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactID, _groupPermission).ConfigureAwait(false);
			int jobHistoryErrorTypeId = _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistory));

			// act
			bool result = _userPermissionRepository.UserHasArtifactTypePermissions(
				jobHistoryErrorTypeId,
				new[] { ArtifactPermission.View });

			// assert
			result.Should().Be(expectedResult);
		}

		[IdentifiedTestCase("912c12ed-9503-418a-8eab-627786b3da54", true, false, false)]
		[IdentifiedTestCase("94901b75-55d9-4b47-979d-1769705c2cfa", true, true, true)]
		[IdentifiedTestCase("e2ee4387-6673-489b-8d0d-ddad42d60f67", false, false, false)]
		public async Task UserHasArtifactTypePermissions_ArtifactId_MultiplePermission(bool viewSelected, bool editSelected, bool expectedResult)
		{
			// arrange
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission(ObjectTypes.JobHistory);
			permission.ViewSelected = viewSelected;
			permission.EditSelected = editSelected;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactID, _groupPermission).ConfigureAwait(false);
			int jobHistoryErrorTypeId = _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistory));

			// act
			bool result = _userPermissionRepository.UserHasArtifactTypePermissions(
				jobHistoryErrorTypeId,
				new[] { ArtifactPermission.View, ArtifactPermission.Edit });

			// assert
			result.Should().Be(expectedResult);
		}

		[IdentifiedTestCase("c389a2a0-8560-4365-b459-8044c2709d74", true, false, false)]
		[IdentifiedTestCase("3a611677-0772-4f95-b266-c9cb6f324831", false, true, false)]
		[IdentifiedTestCase("1d39237e-cf28-435f-a1b8-4aa43396ba2b", true, true, true)]
		[IdentifiedTestCase("dea8d9d7-23a6-4ece-ad89-49ecda9c9182", false, false, false)]
		public async Task UserHasArtifactTypePermissions_ArtifactId_CheckSubSetOfThePermissions(bool hasAddPermission, bool hasEditPermission, bool expectedResult)
		{
			// arrange
			ObjectPermission permission = _groupPermission.ObjectPermissions.FindPermission(ObjectTypes.JobHistory);
			permission.AddSelected = hasAddPermission;
			permission.EditSelected = hasEditPermission;
			permission.ViewSelected = true;

			await Permission.SavePermissionAsync(SourceWorkspaceArtifactID, _groupPermission).ConfigureAwait(false);
			int jobHistoryErrorTypeId = _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.JobHistory));

			// act
			bool result = _userPermissionRepository.UserHasArtifactTypePermissions(
				jobHistoryErrorTypeId,
				new[] { ArtifactPermission.Edit, ArtifactPermission.Create });

			// assert
			result.Should().Be(expectedResult);
		}

		[IdentifiedTestCase("38d70b28-8696-49a6-b45f-48c6934a6525", true)]
		[IdentifiedTestCase("9d93216e-0993-43eb-87e3-9e8485d253ef", false)]
		public void UserHasArtifactInstancePermission_UsingAdmin(bool useAdmin)
		{
			// arrange
			IntegrationPointModel model = CreateNewIntegrationPoint();

			if (!useAdmin)
			{
				Group.RemoveGroupFromWorkspace(SourceWorkspaceArtifactID, _groupId);
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

		[IdentifiedTestCase("ff6326fe-cba7-441a-bb75-084e245b68dd")]
		public void UserBelongsToGroup_ShouldReturnsTrue_WhenUserBelongsToGroup()
		{
			// act
			bool result = _adminPermissionRepository.UserBelongsToGroup(_user.ArtifactID, _groupId);

			// assert
			result.Should().BeTrue();
		}

		[IdentifiedTestCase("edf48796-895d-43e6-96b1-c81544cc96be")]
		public void UserBelongsToGroup_ShouldReturnsFalse_WhenUserDoesNotBelongToGroup()
		{
			// arrange
			UserModel userWithoutGroup = User.CreateUser("Test", "User", "test.user@relativity.com");

			// act
			bool result = _adminPermissionRepository.UserBelongsToGroup(userWithoutGroup.ArtifactID, _groupId);

			// assert
			result.Should().BeFalse();
		}

		private IntegrationPointModel CreateNewIntegrationPoint()
		{
			IIntegrationPointTypeService integrationPointTypeService = Container.Resolve<IIntegrationPointTypeService>();
			IntegrationPointType integrationPointType = integrationPointTypeService.GetIntegrationPointType(
				Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid);

			var model = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = ObjectManager.Query<DestinationProvider>(new QueryRequest()).First().ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"UserHasArtifactInstancePermission - {DateTime.Today:yy-MM-dd HH-mm-ss}",
				Map = CreateDefaultFieldMap(),
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler(),
				Type = integrationPointType.ArtifactId
			};
			model = CreateOrUpdateIntegrationPoint(model);
			return model;
		}

		private async Task CreateTestUserAndGroupAsync()
		{
			await CreateTestGroupAsync().ConfigureAwait(false);
			CreateTestUser();
		}

		private async Task CreateTestGroupAsync()
		{
			_groupId = Group.CreateGroup("krowten");
			Group.AddGroupToWorkspace(SourceWorkspaceArtifactID, _groupId);
			_groupPermission = await Permission
				.GetGroupPermissionsAsync(SourceWorkspaceArtifactID, _groupId)
				.ConfigureAwait(false);
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
