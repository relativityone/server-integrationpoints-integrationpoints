using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.RelativitySync.Adapters;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class ValidatorTests : IntegrationTestBase
	{
		private Validator _validator;
		private ValidationConfigurationStub _config;
		private int _workspaceId;

		[SetUp]
		public void SetUp()
		{
			_workspaceId = Workspace.CreateWorkspace(Guid.NewGuid().ToString(), SourceProviderTemplate.WorkspaceTemplates.NEW_CASE_TEMPLATE);

			IWindsorContainer container = new WindsorContainer();
			container.Register(Component.For<IHelper>().Instance(Helper));

			IExtendedJob job = new Mock<IExtendedJob>().Object;
			IValidationExecutorFactory validationExecutorFactory = new Mock<IValidationExecutorFactory>().Object;
			IAPILog logger = new Mock<IAPILog>().Object;

			_validator = new Validator(container, job, validationExecutorFactory, logger);
			_config = new ValidationConfigurationStub();
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