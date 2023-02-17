using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Tests.Services.Synchronizer
{
    [TestFixture, Category("Unit")]
    public class RdoSynchronizerProviderTests
    {
        private Mock<IDestinationProviderRepository> _destinationProviderRepositoryMock;
        private Mock<IAPILog> _loggerMock;
        private RdoSynchronizerProvider _sut;
        private const string _RELATIVITY_PROVIDER_NAME = "Relativity";
        private const string _LOAD_FILE_PROVIDER_NAME = "Load File";
        private readonly IDictionary<string, string> _destinationProviders = new Dictionary<string, string>
        {
            [_RELATIVITY_PROVIDER_NAME] = "74A863B9-00EC-4BB7-9B3E-1E22323010C6",
            [_LOAD_FILE_PROVIDER_NAME] = "1D3AD995-32C5-48FE-BAA5-5D97089C8F18"
        };

        [SetUp]
        public void SetUp()
        {
            _destinationProviderRepositoryMock = new Mock<IDestinationProviderRepository>();
            _loggerMock = new Mock<IAPILog>
            {
                DefaultValue = DefaultValue.Mock
            };

            _sut = new RdoSynchronizerProvider(_destinationProviderRepositoryMock.Object, _loggerMock.Object);
        }

        [Test]
        public void ShouldCreateDestinationProvidersIfTheyNotExist()
        {
            // arrange
            _destinationProviderRepositoryMock
                .Setup(x =>
                    x.ReadByProviderGuid(It.IsAny<string>())
                )
                .Returns((DestinationProvider)null);

            // act
            _sut.CreateOrUpdateDestinationProviders();

            // assert
            VerifyDestinationProviderCreated(_RELATIVITY_PROVIDER_NAME);
            VerifyDestinationProviderCreated(_LOAD_FILE_PROVIDER_NAME);
        }

        [Test]
        public void ShouldUpdateDestinationProviderNameIfTheyExist()
        {
            // arrange
            _destinationProviderRepositoryMock
                .Setup(x =>
                    x.ReadByProviderGuid(It.IsAny<string>())
                )
                .Returns((string guid) => CreateNewDestinationProvider(guid));

            // act
            _sut.CreateOrUpdateDestinationProviders();

            // assert
            VerifyDestinationProviderUpdated(_RELATIVITY_PROVIDER_NAME);
            VerifyDestinationProviderUpdated(_LOAD_FILE_PROVIDER_NAME);
        }

        [Test]
        public void ShouldCreateAndUpdateDestinationProvidersIfTheyPartiallyExist()
        {
            // arrange
            _destinationProviderRepositoryMock
                .Setup(x =>
                    x.ReadByProviderGuid(_destinationProviders[_RELATIVITY_PROVIDER_NAME])
                )
                .Returns((string guid) => CreateNewDestinationProvider(guid));
            _destinationProviderRepositoryMock
                .Setup(x =>
                    x.ReadByProviderGuid(_destinationProviders[_LOAD_FILE_PROVIDER_NAME])
                )
                .Returns((DestinationProvider)null);

            // act
            _sut.CreateOrUpdateDestinationProviders();

            // assert
            VerifyDestinationProviderUpdated(_RELATIVITY_PROVIDER_NAME);
            VerifyDestinationProviderCreated(_LOAD_FILE_PROVIDER_NAME);
        }

        private void VerifyDestinationProviderCreated(string destinationProviderName)
        {
            _destinationProviderRepositoryMock.Verify(x =>
                x.Create(
                    It.Is<DestinationProvider>(provider => VerifyDestinationProvider(provider, destinationProviderName))
                )
            );
        }

        private void VerifyDestinationProviderUpdated(string destinationProviderName)
        {
            _destinationProviderRepositoryMock.Verify(x =>
                x.Update(
                    It.Is<DestinationProvider>(provider => VerifyDestinationProvider(provider, destinationProviderName))
                )
            );
        }

        private bool VerifyDestinationProvider(DestinationProvider provider, string destinationProviderName)
        {
            string destinationProviderGuid = _destinationProviders[destinationProviderName];

            bool isValid = true;
            isValid &= provider.Name == destinationProviderName;
            isValid &= provider.Identifier == destinationProviderGuid;

            return isValid;
        }

        private DestinationProvider CreateNewDestinationProvider(string identifier)
        {
            return new DestinationProvider
            {
                Name = string.Empty,
                Identifier = identifier
            };
        }
    }
}
