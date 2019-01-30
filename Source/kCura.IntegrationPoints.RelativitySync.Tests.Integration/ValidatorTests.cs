using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class ValidatorTests : IntegrationTestBase
	{
		private int _workspaceId;

		[SetUp]
		public void SetUp()
		{
			_workspaceId = Workspace.CreateWorkspace(Guid.NewGuid().ToString(), SourceProviderTemplate.WorkspaceTemplates.NEW_CASE_TEMPLATE);

			IWindsorContainer container = new WindsorContainer();
			container.Register(Component.For<IHelper>().Instance(Helper));
			_instance = new DestinationWorkspaceObjectTypesCreation(container);

			_configuration = new DestinationWorkspaceObjectTypesCreationConfigurationStub
			{
				DestinationWorkspaceArtifactId = _workspaceId
			};
		}

		[TearDown]
		public void TearDown()
		{
			if (_workspaceId != 0)
			{
				Workspace.DeleteWorkspace(_workspaceId);
			}
		}
	}
}