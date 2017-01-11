using System;
using System.Collections.Generic;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Interfaces.Private.Exceptions;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using kCura.IntegrationPoints.Services.Repositories;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services.Tests.Managers
{
	public class ProviderManagerTests : TestBase
	{
		private const int _WORKSPACE_ID = 266818;
		private ProviderManager _providerManager;
		private IPermissionRepository _permissionRepository;
		private ILog _logger;
		private IWindsorContainer _container;

		public override void SetUp()
		{
			_logger = Substitute.For<ILog>();
			_permissionRepository = Substitute.For<IPermissionRepository>();
			_container = Substitute.For<IWindsorContainer>();

			var permissionRepositoryFactory = Substitute.For<IPermissionRepositoryFactory>();
			permissionRepositoryFactory.Create(Arg.Any<IHelper>(), _WORKSPACE_ID).Returns(_permissionRepository);

			_providerManager = new ProviderManager(_logger, permissionRepositoryFactory, _container);
		}

		[Test]
		public void ItShouldGrantAccessForSourceProvider()
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.SourceProvider), ArtifactPermission.View).Returns(true);

			_providerManager.GetSourceProviderArtifactIdAsync(_WORKSPACE_ID, Guid.NewGuid().ToString()).Wait();
			_providerManager.GetSourceProviders(_WORKSPACE_ID).Wait();

			_permissionRepository.Received(2).UserHasPermissionToAccessWorkspace();
			_permissionRepository.Received(2).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.SourceProvider), ArtifactPermission.View);
		}

		[Test]
		public void ItShouldGrantAccessForDestinationProvider()
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.DestinationProvider), ArtifactPermission.View).Returns(true);

			_providerManager.GetDestinationProviderArtifactIdAsync(_WORKSPACE_ID, Guid.NewGuid().ToString()).Wait();
			_providerManager.GetDestinationProviders(_WORKSPACE_ID).Wait();

			_permissionRepository.Received(2).UserHasPermissionToAccessWorkspace();
			_permissionRepository.Received(2).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.DestinationProvider), ArtifactPermission.View);
		}

		[Test]
		[TestCase(false, false, "Workspace, Source Provider - View")]
		[TestCase(false, true, "Workspace")]
		[TestCase(true, false, "Source Provider - View")]
		public void ItShouldDenyAccessForSourceProviderAndLogIt(bool workspaceAccess, bool sourceProviderAccess, string missingPermissions)
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.SourceProvider), ArtifactPermission.View).Returns(sourceProviderAccess);

			Assert.That(() => _providerManager.GetSourceProviderArtifactIdAsync(_WORKSPACE_ID, Guid.NewGuid().ToString()).Wait(),
				Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
					.And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

			Assert.That(() => _providerManager.GetSourceProviders(_WORKSPACE_ID).Wait(),
				Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
					.And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

			_permissionRepository.Received(2).UserHasPermissionToAccessWorkspace();
			_permissionRepository.Received(2).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.SourceProvider), ArtifactPermission.View);

			_logger.Received(1)
				.LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetSourceProviderArtifactIdAsync",
					missingPermissions);
			_logger.Received(1)
				.LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetSourceProviders", missingPermissions);
		}

		[Test]
		[TestCase(false, false, "Workspace, Destination Provider - View")]
		[TestCase(false, true, "Workspace")]
		[TestCase(true, false, "Destination Provider - View")]
		public void ItShouldDenyAccessForDestinationProviderAndLogIt(bool workspaceAccess, bool destinationProviderAccess, string missingPermissions)
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.DestinationProvider), ArtifactPermission.View).Returns(destinationProviderAccess);

			Assert.That(() => _providerManager.GetDestinationProviderArtifactIdAsync(_WORKSPACE_ID, Guid.NewGuid().ToString()).Wait(),
				Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
					.And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

			Assert.That(() => _providerManager.GetDestinationProviders(_WORKSPACE_ID).Wait(),
				Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
					.And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

			_permissionRepository.Received(2).UserHasPermissionToAccessWorkspace();
			_permissionRepository.Received(2).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.DestinationProvider), ArtifactPermission.View);

			_logger.Received(1)
				.LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetDestinationProviderArtifactIdAsync",
					missingPermissions);
			_logger.Received(1)
				.LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetDestinationProviders", missingPermissions);
		}

		[Test]
		public void ItShouldHideAndLogException()
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.SourceProvider), ArtifactPermission.View).Returns(true);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.DestinationProvider), ArtifactPermission.View).Returns(true);

			var expectedException = new ArgumentException();

			var providerRepository = Substitute.For<IProviderRepository>();
			providerRepository.GetDesinationProviders(Arg.Any<int>()).Throws(expectedException);
			providerRepository.GetDestinationProviderArtifactId(Arg.Any<int>(), Arg.Any<string>()).Throws(expectedException);
			providerRepository.GetSourceProviders(Arg.Any<int>()).Throws(expectedException);
			providerRepository.GetSourceProviderArtifactId(Arg.Any<int>(), Arg.Any<string>()).Throws(expectedException);

			_container.Resolve<IProviderRepository>().Returns(providerRepository);
			
			Assert.That(() => _providerManager.GetDestinationProviderArtifactIdAsync(_WORKSPACE_ID, Guid.NewGuid().ToString()).Wait(),
				Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
					.And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

			Assert.That(() => _providerManager.GetDestinationProviders(_WORKSPACE_ID).Wait(),
				Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
					.And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

			Assert.That(() => _providerManager.GetSourceProviderArtifactIdAsync(_WORKSPACE_ID, Guid.NewGuid().ToString()).Wait(),
				Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
					.And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

			Assert.That(() => _providerManager.GetSourceProviders(_WORKSPACE_ID).Wait(),
				Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
					.And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

			_logger.Received(1)
				.LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetDestinationProviderArtifactIdAsync");
			_logger.Received(1)
				.LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetDestinationProviders");
			_logger.Received(1)
				.LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetSourceProviderArtifactIdAsync");
			_logger.Received(1)
				.LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetSourceProviders");
		}

		[Test]
		public void ItShouldReturnAllSourceProviders()
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.SourceProvider), ArtifactPermission.View).Returns(true);

			var providerRepository = Substitute.For<IProviderRepository>();
			_container.Resolve<IProviderRepository>().Returns(providerRepository);

			var expectedResult = new List<ProviderModel>();
			providerRepository.GetSourceProviders(_WORKSPACE_ID).Returns(expectedResult);

			var actualResult = _providerManager.GetSourceProviders(_WORKSPACE_ID).Result;

			providerRepository.Received(1).GetSourceProviders(_WORKSPACE_ID);

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ItShouldReturnSourceProvider()
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.SourceProvider), ArtifactPermission.View).Returns(true);

			var providerRepository = Substitute.For<IProviderRepository>();
			_container.Resolve<IProviderRepository>().Returns(providerRepository);

			var guid = Guid.NewGuid();

			var expectedResult = 892946;
			providerRepository.GetSourceProviderArtifactId(_WORKSPACE_ID, guid.ToString()).Returns(expectedResult);

			var actualResult = _providerManager.GetSourceProviderArtifactIdAsync(_WORKSPACE_ID, guid.ToString()).Result;

			providerRepository.Received(1).GetSourceProviderArtifactId(_WORKSPACE_ID, guid.ToString());

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ItShouldReturnAllDestinationProviders()
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.DestinationProvider), ArtifactPermission.View).Returns(true);

			var providerRepository = Substitute.For<IProviderRepository>();
			_container.Resolve<IProviderRepository>().Returns(providerRepository);

			var expectedResult = new List<ProviderModel>();
			providerRepository.GetDesinationProviders(_WORKSPACE_ID).Returns(expectedResult);

			var actualResult = _providerManager.GetDestinationProviders(_WORKSPACE_ID).Result;

			providerRepository.Received(1).GetDesinationProviders(_WORKSPACE_ID);

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ItShouldReturnDestinationProvider()
		{
			_permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
			_permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.DestinationProvider), ArtifactPermission.View).Returns(true);

			var providerRepository = Substitute.For<IProviderRepository>();
			_container.Resolve<IProviderRepository>().Returns(providerRepository);

			var guid = Guid.NewGuid();

			var expectedResult = 118867;
			providerRepository.GetDestinationProviderArtifactId(_WORKSPACE_ID, guid.ToString()).Returns(expectedResult);

			var actualResult = _providerManager.GetDestinationProviderArtifactIdAsync(_WORKSPACE_ID, guid.ToString()).Result;

			providerRepository.Received(1).GetDestinationProviderArtifactId(_WORKSPACE_ID, guid.ToString());

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}
	}
}