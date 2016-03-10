using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Permission;

namespace kCura.IntegrationPoints.Data.Tests.Unit
{
	[TestFixture]
	public class PermissionServiceTest
	{
		private IPermissionManager _permissionManager;
		private IServicesMgr _servicesMgr;
		private PermissionService _instance;
		private const int WORKSPACE_ID = 930293; [SetUp] public void SetUp() {
			_servicesMgr = NSubstitute.Substitute.For<IServicesMgr>();
			_permissionManager = NSubstitute.Substitute.For<IPermissionManager>();

			_servicesMgr.CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser)).Returns(_permissionManager);

			_instance = new PermissionService(_servicesMgr);
		}

		[Test]
		public void UserCanImport_UserHasPermissionToImport()
		{
			//ARRANGE
			_permissionManager.GetPermissionSelectedAsync(
				Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(x => this.PermissionValuesMatch(new List<PermissionRef>() {new PermissionRef() {PermissionID = 158} }, x)))
				.Returns(Task.FromResult(new List<PermissionValue>()
				{
					new PermissionValue()
					{
						PermissionID = 158,
						Selected = true
					}
				}));

			//ACT
			bool userCanImport = _instance.UserCanImport(WORKSPACE_ID);

			//ASSERT 
			Assert.IsTrue(userCanImport, "The user should have correct permissions");
		}

		[Test]
		public void UserCanImport_UserDoesNotHavePermissionToImport()
		{
			//ARRANGE
			_permissionManager.GetPermissionSelectedAsync(
				Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = 158 } }, x)))
				.Returns(Task.FromResult(new List<PermissionValue>()
				{
					new PermissionValue()
					{
						PermissionID = 158,
						Selected = false
					}
				}));

			//ACT
			bool userCanImport = _instance.UserCanImport(WORKSPACE_ID);

			//ASSERT 
			Assert.IsFalse(userCanImport, "The user should not have correct permissions");
		}

		[Test]
		public void UserCanImport_ServiceReturnsIncorrectPermissionIdWithSuccess()
		{
			//ARRANGE
			_permissionManager.GetPermissionSelectedAsync(
				Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = 158 } }, x)))
				.Returns(Task.FromResult(new List<PermissionValue>()
				{
					new PermissionValue()
					{
						PermissionID = 123,
						Selected = true
					}
				}));

			//ACT
			bool userCanImport = _instance.UserCanImport(WORKSPACE_ID);

			//ASSERT 
			Assert.IsFalse(userCanImport, "The user should not have correct permissions");
		}

		[Test]
		public void UserCanImport_ServiceReturnsIncorrectPermissionIdWithNoSuccess()
		{
			//ARRANGE
			_permissionManager.GetPermissionSelectedAsync(
				Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = 158 } }, x)))
				.Returns(Task.FromResult(new List<PermissionValue>()
				{
					new PermissionValue()
					{
						PermissionID = 123,
						Selected = false
					}
				}));

			//ACT
			bool userCanImport = _instance.UserCanImport(WORKSPACE_ID);

			//ASSERT 
			Assert.IsFalse(userCanImport, "The user should not have correct permissions");
		}

		[Test]
		public void UserCanImport_ServiceReturnsNoPermissions()
		{
			//ARRANGE
			_permissionManager.GetPermissionSelectedAsync(
				Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = 158 } }, x)))
				.Returns(Task.FromResult(new List<PermissionValue>() {}));

			//ACT
			bool userCanImport = _instance.UserCanImport(WORKSPACE_ID);

			//ASSERT 
			Assert.IsFalse(userCanImport, "The user should not have correct permissions");
		}

		private bool PermissionValuesMatch(List<PermissionRef> expected, List<PermissionRef> actual)
		{
			if (expected.Count != actual.Count)
			{
				return false;
			}

			if (expected.First().PermissionID != actual.First().PermissionID)
			{
				return false;
			}

			return true;
		}
	}
}
