using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs.Folder;
using Relativity.Services.Folder;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public sealed class DestinationFolderValidatorTests
	{
		private DestinationFolderValidator _sut;

		private FolderStatus _getAccessStatusAsyncResult;

		private Mock<IDestinationServiceFactoryForUser> _serviceFactoryMock;
		private Mock<IFolderManager> _folderManagerMock;
		private Mock<IValidationConfiguration> _configurationMock;

		private const int _FOLDER_ARTIFACT_ID = 456;
		private const int _WORKSPACE_ARTIFACT_ID = 123;

		[SetUp]
		public void SetUp()
		{
			_getAccessStatusAsyncResult = new FolderStatus();

			_folderManagerMock = new Mock<IFolderManager>();
			_folderManagerMock.Setup(fm => fm.GetAccessStatusAsync(_WORKSPACE_ARTIFACT_ID, _FOLDER_ARTIFACT_ID)).ReturnsAsync(_getAccessStatusAsyncResult);

			_serviceFactoryMock = new Mock<IDestinationServiceFactoryForUser>();
			_serviceFactoryMock.Setup(sf => sf.CreateProxyAsync<IFolderManager>()).ReturnsAsync(_folderManagerMock.Object);

			_configurationMock = new Mock<IValidationConfiguration>();
			_configurationMock.Setup(c => c.DestinationFolderArtifactId).Returns(_FOLDER_ARTIFACT_ID);
			_configurationMock.Setup(c => c.DestinationWorkspaceArtifactId).Returns(_WORKSPACE_ARTIFACT_ID);

			_sut = new DestinationFolderValidator(_serviceFactoryMock.Object, new EmptyLogger());
		}
		
		[Test]
		public async Task ItShouldHandleValidConfiguration()
		{
			_getAccessStatusAsyncResult.Exists = true;

			ValidationResult result = await _sut.ValidateAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}
		
		[Test]
		public async Task ItShouldHandleInvalidConfiguration()
		{
			_getAccessStatusAsyncResult.Exists = false;

			ValidationResult result = await _sut.ValidateAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			result.IsValid.Should().BeFalse();
			result.Messages.Should().NotBeEmpty();
		}
	}
}
