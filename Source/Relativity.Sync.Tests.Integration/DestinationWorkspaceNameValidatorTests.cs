using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class DestinationWorkspaceNameValidatorTests
	{
		private ConfigurationStub _configuration;
		private Mock<IDestinationServiceFactoryForUser> _serviceFactory;
		private DestinationWorkspaceNameValidator _sut;
		private Mock<IObjectManager> _objectManagerMock;
		private const int _WORKSPACE_ARTIFACT_ID = 123;
		private const int _ADMIN_WORKSPACE_ARTIFACT_ID = -1;

		[SetUp]
		public void SetUp()
		{
			_objectManagerMock = new Mock<IObjectManager>();

			_serviceFactory = new Mock<IDestinationServiceFactoryForUser>();
			_serviceFactory.Setup(sf => sf.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManagerMock.Object);

			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			containerBuilder.RegisterInstance(_serviceFactory.Object).As<IDestinationServiceFactoryForUser>();
			containerBuilder.RegisterType<DestinationWorkspaceNameValidator>();
			IContainer container = containerBuilder.Build();

			_configuration = new ConfigurationStub();

			_sut = container.Resolve<DestinationWorkspaceNameValidator>();
		}

		[Test]
		public async Task ItShouldHandleValidDestinationWorkspaceName()
		{
			string validWorkspaceName = "So much valid";

			_objectManagerMock.Setup(x =>
					x.QueryAsync(
						_ADMIN_WORKSPACE_ARTIFACT_ID,
						It.Is<QueryRequest>(y => y.Condition.Contains(_WORKSPACE_ARTIFACT_ID.ToString(CultureInfo.InvariantCulture))),
						It.IsAny<int>(),
						It.IsAny<int>(),
						CancellationToken.None,
						It.IsAny<IProgress<ProgressReport>>()))
						.Returns(Task.FromResult(new QueryResult
					{
						Objects = new List<RelativityObject> {new RelativityObject() {Name = validWorkspaceName}}
					}));

			_configuration.DestinationWorkspaceArtifactId = _WORKSPACE_ARTIFACT_ID;

			ValidationResult result = await _sut.ValidateAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		[Test]
		public async Task ItShouldHandleInvalidDestinationWorkspaceName()
		{
			string invalidWorkspaceName = "So ; much ; invalid";

			_objectManagerMock.Setup(x => x.QueryAsync(
				-1,
				It.Is<QueryRequest>(y => y.Condition.Contains(_WORKSPACE_ARTIFACT_ID.ToString(CultureInfo.InvariantCulture))),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(new QueryResult
			{
				Objects = new List<RelativityObject> { new RelativityObject() { Name = invalidWorkspaceName } }
			}));

			_configuration.DestinationWorkspaceArtifactId = _WORKSPACE_ARTIFACT_ID;

			ValidationResult result = await _sut.ValidateAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			result.IsValid.Should().BeFalse();
			result.Messages.Should().NotBeEmpty();
		}
	}
}
