using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.JobHistory
{
    [TestFixture, Category("Unit")]
    public class CachedIntegrationPointProviderTypeServiceTests
    {
        private const int _DESTINATION_PROVIDER_ID = 21;
        private const int _SOURCE_PROVIDER_ID = 12;
        private const int _DEFAULT_CACHE_REFRESH_DELAY = 30;

        [TestFixture, Category("Unit")]
        public class GetProviderTypeByIntegrationPointMethod
        {
            private CachedIntegrationPointProviderTypeService _service;
            private DateTime _currentTimeValue = DateTime.UtcNow;
            private IIntegrationPointService _integrationPointServiceMock;
            private IProviderTypeService _providerTypeServiceMock;
            private IDateTimeHelper _currentTimeProvider;

            private readonly IntegrationPointDto _firstIntegrationPoint = new IntegrationPointDto
            {
                ArtifactId = 1,
                SourceProvider = _SOURCE_PROVIDER_ID,
                DestinationProvider = _DESTINATION_PROVIDER_ID
            };

            private readonly IntegrationPointDto _secondIntegrationPoint = new IntegrationPointDto
            {
                ArtifactId = 2,
                SourceProvider = _SOURCE_PROVIDER_ID + 1,
                DestinationProvider = _DESTINATION_PROVIDER_ID + 1
            };

            [SetUp]
            public void SetUp()
            {
                _providerTypeServiceMock = Substitute.For<IProviderTypeService>();

                _integrationPointServiceMock = Substitute.For<IIntegrationPointService>();

                _currentTimeProvider = Substitute.For<IDateTimeHelper>();
                _currentTimeProvider.Now().Returns(callInfo => _currentTimeValue);

                _service = new CachedIntegrationPointProviderTypeService(_providerTypeServiceMock,
                    _integrationPointServiceMock, _currentTimeProvider,
                    TimeSpan.FromSeconds(_DEFAULT_CACHE_REFRESH_DELAY));
            }


            [Test]
            public void ItShouldRetrieveDataFromServiceForTheFirstTime()
            {
                _service.GetProviderType(_firstIntegrationPoint);

                VerifyOnlyTypeServiceIsCalled();
            }

            [Test]
            public void ItShouldCallServiceOnceWithinRefreshDelay()
            {
                _service.GetProviderType(_firstIntegrationPoint);
                _service.GetProviderType(_firstIntegrationPoint);

                VerifyOnlyTypeServiceIsCalled();
            }

            [Test]
            public void ItShouldReturnSameProviderTypeWithinRefreshDelay()
            {
                const ProviderType expectedFirstProviderType = ProviderType.FTP;
                _providerTypeServiceMock.GetProviderType(0, 0).ReturnsForAnyArgs(expectedFirstProviderType);

                ProviderType firstCallProviderType = _service.GetProviderType(_firstIntegrationPoint);

                _providerTypeServiceMock.GetProviderType(0, 0).ReturnsForAnyArgs(ProviderType.LDAP);

                ProviderType secondCallProviderType = _service.GetProviderType(_firstIntegrationPoint);

                Assert.That(firstCallProviderType, Is.EqualTo(expectedFirstProviderType));
                Assert.That(secondCallProviderType, Is.EqualTo(expectedFirstProviderType));
            }

            [Test]
            public void ItShouldUpdateProviderTypeAfterRefreshDelay()
            {
                const ProviderType expectedFirstProviderType = ProviderType.FTP;
                const ProviderType expectedSecondProviderType = ProviderType.LDAP;

                _providerTypeServiceMock.GetProviderType(0, 0).ReturnsForAnyArgs(expectedFirstProviderType);

                ProviderType firstCallProviderType = _service.GetProviderType(_firstIntegrationPoint);

                _currentTimeValue = _currentTimeValue.AddSeconds(_DEFAULT_CACHE_REFRESH_DELAY + 1);
                _providerTypeServiceMock.GetProviderType(0, 0).ReturnsForAnyArgs(expectedSecondProviderType);

                ProviderType secondCallProviderType = _service.GetProviderType(_firstIntegrationPoint);

                Assert.That(firstCallProviderType, Is.EqualTo(expectedFirstProviderType));
                Assert.That(secondCallProviderType, Is.EqualTo(expectedSecondProviderType));
            }

            [Test]
            public void ItShouldCallServiceAgainWhenRefreshDelayPasses()
            {
                _service.GetProviderType(_firstIntegrationPoint);
                _service.GetProviderType(_firstIntegrationPoint);

                VerifyOnlyTypeServiceIsCalled();

                _currentTimeValue = _currentTimeValue.AddSeconds(_DEFAULT_CACHE_REFRESH_DELAY + 1);

                _service.GetProviderType(_firstIntegrationPoint);

                VerifyOnlyTypeServiceIsCalled(2);
            }

            [Test]
            public void ItShouldCallServiceForEachIntegrationPointWithinRefreshDelay()
            {
                _service.GetProviderType(_firstIntegrationPoint);
                _service.GetProviderType(_secondIntegrationPoint);

                _providerTypeServiceMock.Received(1).GetProviderType(_SOURCE_PROVIDER_ID, _DESTINATION_PROVIDER_ID);
                _providerTypeServiceMock.Received(1).GetProviderType(_SOURCE_PROVIDER_ID + 1, _DESTINATION_PROVIDER_ID + 1);
            }

            private void VerifyOnlyTypeServiceIsCalled(int times = 1)
            {
                _integrationPointServiceMock.DidNotReceive();
                _providerTypeServiceMock.Received(times).GetProviderType(_SOURCE_PROVIDER_ID, _DESTINATION_PROVIDER_ID);
            }
        }

        [TestFixture, Category("Unit")]
        public class GetProviderTypeByIdMethod
        {
            private CachedIntegrationPointProviderTypeService _service;
            private DateTime _currentTimeValue = DateTime.UtcNow;
            private IIntegrationPointService _integrationPointServiceMock;
            private IProviderTypeService _providerTypeServiceMock;
            private IDateTimeHelper _currentTimeProvider;

            [SetUp]
            public void SetUp()
            {
                _providerTypeServiceMock = Substitute.For<IProviderTypeService>();

                _integrationPointServiceMock = Substitute.For<IIntegrationPointService>();
                _integrationPointServiceMock.Read(0).ReturnsForAnyArgs(new IntegrationPointDto
                {
                    SourceProvider = _SOURCE_PROVIDER_ID,
                    DestinationProvider = _DESTINATION_PROVIDER_ID
                });

                _currentTimeProvider = Substitute.For<IDateTimeHelper>();
                _currentTimeProvider.Now().Returns(callInfo => _currentTimeValue);

                _service = new CachedIntegrationPointProviderTypeService(_providerTypeServiceMock,
                    _integrationPointServiceMock, _currentTimeProvider,
                    TimeSpan.FromSeconds(_DEFAULT_CACHE_REFRESH_DELAY));
            }


            [Test]
            public void ItShouldRetrieveDataFromServiceForTheFirstTime()
            {
                _service.GetProviderType(1);

                VerifyBothServicesCalled();
            }

            [Test]
            public void ItShouldCallServiceOnceWithinRefreshDelay()
            {
                _service.GetProviderType(1);
                _service.GetProviderType(1);

                VerifyBothServicesCalled();
            }

            [Test]
            public void ItShouldReturnSameProviderTypeWithinRefreshDelay()
            {
                const ProviderType expectedFirstProviderType = ProviderType.FTP;
                _providerTypeServiceMock.GetProviderType(0, 0).ReturnsForAnyArgs(expectedFirstProviderType);

                ProviderType firstCallProviderType = _service.GetProviderType(1);

                _providerTypeServiceMock.GetProviderType(0, 0).ReturnsForAnyArgs(ProviderType.LDAP);

                ProviderType secondCallProviderType = _service.GetProviderType(1);

                Assert.That(firstCallProviderType, Is.EqualTo(expectedFirstProviderType));
                Assert.That(secondCallProviderType, Is.EqualTo(expectedFirstProviderType));
            }

            [Test]
            public void ItShouldUpdateProviderTypeAfterRefreshDelay()
            {
                const ProviderType expectedFirstProviderType = ProviderType.FTP;
                const ProviderType expectedSecondProviderType = ProviderType.LDAP;

                _providerTypeServiceMock.GetProviderType(0, 0).ReturnsForAnyArgs(expectedFirstProviderType);

                ProviderType firstCallProviderType = _service.GetProviderType(1);

                _currentTimeValue = _currentTimeValue.AddSeconds(_DEFAULT_CACHE_REFRESH_DELAY + 1);
                _providerTypeServiceMock.GetProviderType(0, 0).ReturnsForAnyArgs(expectedSecondProviderType);

                ProviderType secondCallProviderType = _service.GetProviderType(1);

                Assert.That(firstCallProviderType, Is.EqualTo(expectedFirstProviderType));
                Assert.That(secondCallProviderType, Is.EqualTo(expectedSecondProviderType));
            }

            [Test]
            public void ItShouldCallServiceAgainWhenRefreshDelayPasses()
            {
                _service.GetProviderType(1);
                _service.GetProviderType(1);

                VerifyBothServicesCalled();

                _currentTimeValue = _currentTimeValue.AddSeconds(_DEFAULT_CACHE_REFRESH_DELAY + 1);

                _service.GetProviderType(1);

                VerifyBothServicesCalled(2);
            }

            [Test]
            public void ItShouldCallServiceForEachIntegrationPointWithinRefreshDelay()
            {
                _service.GetProviderType(1);
                _service.GetProviderType(2);

                _integrationPointServiceMock.Received(1).Read(1);
                _integrationPointServiceMock.Received(1).Read(2);
            }

            private void VerifyBothServicesCalled(int times = 1, int integrationPointId = 1)
            {
                _integrationPointServiceMock.Received(times).Read(integrationPointId);
                _providerTypeServiceMock.Received(times).GetProviderType(_SOURCE_PROVIDER_ID, _DESTINATION_PROVIDER_ID);
            }
        }
    }
}
