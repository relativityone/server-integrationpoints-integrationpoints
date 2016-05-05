﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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
		private const int WORKSPACE_ID = 930293;
		private const int _editDocPermission = 45;

		[SetUp]
		public void SetUp() {
			_servicesMgr = NSubstitute.Substitute.For<IServicesMgr>();
			_permissionManager = NSubstitute.Substitute.For<IPermissionManager>();

			_servicesMgr.CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser)).Returns(_permissionManager);

			_instance = new PermissionService(_servicesMgr);
		}

		[Test]
		public void UserCanImport_UserHasPermissionToImport_UserPermissionIsTrue()
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
			_servicesMgr.Received().CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser));
			_permissionManager.Received().GetPermissionSelectedAsync(Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(
					x => this.PermissionValuesMatch(new List<PermissionRef>() {new PermissionRef() {PermissionID = 158}}, x)));
		}

		[Test]
		public void UserCanImport_UserDoesNotHavePermissionToImport_UserPermissionIsFalse()
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
			_servicesMgr.Received().CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser));
			_permissionManager.Received().GetPermissionSelectedAsync(Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(
					x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = 158 } }, x)));
		}

		[Test]
		public void UserCanImport_ServiceReturnsIncorrectPermissionIdWithSuccess_UserPermissionIsFalse()
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
			_servicesMgr.Received().CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser));
			_permissionManager.Received().GetPermissionSelectedAsync(Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(
					x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = 158 } }, x)));
		}

		[Test]
		public void UserCanImport_ServiceReturnsIncorrectPermissionIdWithNoSuccess_UserPermissionIsFalse()
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
			_servicesMgr.Received().CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser));
			_permissionManager.Received().GetPermissionSelectedAsync(Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(
					x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = 158 } }, x)));
		}

		[Test]
		public void UserCanImport_ServiceReturnsNoPermissions_UserPermissionsIsFalse()
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
			_servicesMgr.Received().CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser));
			_permissionManager.Received().GetPermissionSelectedAsync(Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(
					x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = 158 } }, x)));
		}

		[Test]
		public void UserCanImport_ServiceThrowsException_UserPermissionIsFalse()
		{
			//ARRANGE
			_permissionManager.GetPermissionSelectedAsync(
				Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(
					x => this.PermissionValuesMatch(new List<PermissionRef>() {new PermissionRef() {PermissionID = 158}}, x)))
				.Throws(new Exception());

			//ACT
			bool userCanImport = _instance.UserCanImport(WORKSPACE_ID);

			//ASSERT 
			Assert.IsFalse(userCanImport, "The user should not have correct permissions");
			_servicesMgr.Received().CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser));
			_permissionManager.Received().GetPermissionSelectedAsync(Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(
					x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = 158 } }, x)));
		}

		[Test]
		public void UserCanEditDocuments_UserHasPermission_UserPermissionIsTrue()
		{
			//ARRANGE
			_permissionManager.GetPermissionSelectedAsync(
				Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = _editDocPermission } }, x)))
				.Returns(Task.FromResult(new List<PermissionValue>()
				{
					new PermissionValue()
					{
						PermissionID = _editDocPermission,
						Selected = true
					}
				}));

			//ACT
			bool userCanEditDocuments = _instance.UserCanEditDocuments(WORKSPACE_ID);

			//ASSERT 
			Assert.IsTrue(userCanEditDocuments, "The user should have correct permissions");
			_servicesMgr.Received().CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser));
			_permissionManager.Received().GetPermissionSelectedAsync(Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(
					x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = _editDocPermission } }, x)));
		}

		[Test]
		public void UserCanEditDocuments_UserDoesNotHavePermission_UserPermissionIsFalse()
		{
			//ARRANGE
			_permissionManager.GetPermissionSelectedAsync(
				Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = _editDocPermission } }, x)))
				.Returns(Task.FromResult(new List<PermissionValue>()
				{
					new PermissionValue()
					{
						PermissionID = _editDocPermission,
						Selected = false
					}
				}));

			//ACT
			bool userCanEditDocuments = _instance.UserCanEditDocuments(WORKSPACE_ID);

			//ASSERT 
			Assert.IsFalse(userCanEditDocuments, "The user should not have correct permissions");
			_servicesMgr.Received().CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser));
			_permissionManager.Received().GetPermissionSelectedAsync(Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(
					x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = _editDocPermission } }, x)));
		}

		[Test]
		public void UserCanEditDocuments_ServiceReturnsIncorrectPermissionIdWithSuccess_UserPermissionIsFalse()
		{
			//ARRANGE
			_permissionManager.GetPermissionSelectedAsync(
				Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = _editDocPermission } }, x)))
				.Returns(Task.FromResult(new List<PermissionValue>()
				{
					new PermissionValue()
					{
						PermissionID = 123,
						Selected = true
					}
				}));

			//ACT
			bool userCanEditDocuments = _instance.UserCanEditDocuments(WORKSPACE_ID);

			//ASSERT 
			Assert.IsFalse(userCanEditDocuments, "The user should not have correct permissions");
			_servicesMgr.Received().CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser));
			_permissionManager.Received().GetPermissionSelectedAsync(Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(
					x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = _editDocPermission } }, x)));
		}

		[Test]
		public void UserCanEditDocuments_ServiceReturnsIncorrectPermissionIdWithNoSuccess_UserPermissionIsFalse()
		{
			//ARRANGE
			_permissionManager.GetPermissionSelectedAsync(
				Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = _editDocPermission } }, x)))
				.Returns(Task.FromResult(new List<PermissionValue>()
				{
					new PermissionValue()
					{
						PermissionID = 123,
						Selected = false
					}
				}));

			//ACT
			bool userCanEditDocuments = _instance.UserCanEditDocuments(WORKSPACE_ID);

			//ASSERT 
			Assert.IsFalse(userCanEditDocuments, "The user should not have correct permissions");
			_servicesMgr.Received().CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser));
			_permissionManager.Received().GetPermissionSelectedAsync(Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(
					x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = _editDocPermission } }, x)));
		}

		[Test]
		public void UserCanEditDocuments_ServiceReturnsNoPermissions_UserPermissionIsFalse()
		{
			//ARRANGE
			_permissionManager.GetPermissionSelectedAsync(
				Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = _editDocPermission } }, x)))
				.Returns(Task.FromResult(new List<PermissionValue>() { }));

			//ACT
			bool userCanEditDocuments = _instance.UserCanEditDocuments(WORKSPACE_ID);

			//ASSERT 
			Assert.IsFalse(userCanEditDocuments, "The user should not have correct permissions");
			_servicesMgr.Received().CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser));
			_permissionManager.Received().GetPermissionSelectedAsync(Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(
					x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = _editDocPermission } }, x)));
		}

		[Test]
		public void UserCanEditDocuments_ServiceThrowsException_UserPermissionIsFalse()
		{
			//ARRANGE
			_permissionManager.GetPermissionSelectedAsync(
				Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(
					x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = _editDocPermission } }, x)))
				.Throws(new Exception());

			//ACT
			bool userCanEditDocuments = _instance.UserCanEditDocuments(WORKSPACE_ID);

			//ASSERT 
			Assert.IsFalse(userCanEditDocuments, "The user should not have correct permissions");
			_servicesMgr.Received().CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser));
			_permissionManager.Received().GetPermissionSelectedAsync(Arg.Is(WORKSPACE_ID),
				Arg.Is<List<PermissionRef>>(
					x => this.PermissionValuesMatch(new List<PermissionRef>() { new PermissionRef() { PermissionID = _editDocPermission } }, x)));
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
