using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Exceptions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.Services.ResourceServer;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class CaseRepositoryTests
    {
        private CaseRepository _sut;
        private IResourceServerManager _resourceServerManager;
        private IExternalServiceSimpleInstrumentation _simpleInstrumentation;
        private const int _CASE_ARTIFACT_ID = 3535;

        [SetUp]
        public void SetUp()
        {
            _resourceServerManager = Substitute.For<IResourceServerManager>();
            IExternalServiceInstrumentationProvider instrumentationProvider = MockInstrumentation();

            _sut = new CaseRepository(_resourceServerManager, instrumentationProvider);
        }

        [Test]
        public void ItShouldDisposeResorceServerManagerWhenDisposeIsCalled()
        {
            // act
            _sut.Dispose();

            // assert
            _resourceServerManager.Received().Dispose();
        }

        [Test]
        public void ItShouldReturnValueFromResourceServerManager()
        {
            // arrange
            CaseInfo caseInfo = GetCaseInfo();

            Task<CaseInfo> result = Task.FromResult(caseInfo);
            _resourceServerManager.ReadCaseInfo(_CASE_ARTIFACT_ID).Returns(result);

            // act
            ICaseInfoDto actualResult = _sut.Read(_CASE_ARTIFACT_ID);

            // assert
            AssertAreEqual(caseInfo, actualResult);
        }

        [Test]
        public void ItShouldThrowsExceptionInCaseOfExceptionInResourceServerManager()
        {
            // arrange
            var exceptionToThrow = new Exception();
            _resourceServerManager.ReadCaseInfo(_CASE_ARTIFACT_ID).Throws(exceptionToThrow);

            // act
            try
            {
                _sut.Read(_CASE_ARTIFACT_ID);
            }
            catch (IntegrationPointsException ex)
            {
                // assert
                Assert.AreEqual(IntegrationPointsExceptionSource.KEPLER, ex.ExceptionSource);
                Assert.IsTrue(ex.ShouldAddToErrorsTab);
            }
        }

        [Test]
        public void ItShouldUseInstrumentation()
        {
            // arrange
            CaseInfo caseInfo = GetCaseInfo();

            Task<CaseInfo> result = Task.FromResult(caseInfo);
            _resourceServerManager.ReadCaseInfo(_CASE_ARTIFACT_ID).Returns(result);

            // act
            _sut.Read(_CASE_ARTIFACT_ID);

            // assert
            _simpleInstrumentation.Received().Execute(Arg.Any<Func<CaseInfo>>());
        }

        private void AssertAreEqual(CaseInfo expected, ICaseInfoDto actual)
        {
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.ArtifactID, actual.ArtifactID);
            Assert.AreEqual(expected.AsImportAllowed, actual.AsImportAllowed);
            Assert.AreEqual(expected.DocumentPath, actual.DocumentPath);
            Assert.AreEqual(expected.DownloadHandlerURL, actual.DownloadHandlerURL);
            Assert.AreEqual(expected.RootArtifactID, actual.RootArtifactID);
            Assert.AreEqual(expected.MatterArtifactID, actual.MatterArtifactID);
            Assert.AreEqual(expected.ExportAllowed, actual.ExportAllowed);
            Assert.AreEqual(expected.RootFolderID, actual.RootFolderID);
            Assert.AreEqual(expected.EnableDataGrid, actual.EnableDataGrid);
            Assert.AreEqual(expected.StatusCodeArtifactID, actual.StatusCodeArtifactID);
        }

        private static CaseInfo GetCaseInfo()
        {
            return new CaseInfo
            {
                Name = "name",
                ArtifactID = 5454,
                AsImportAllowed = true,
                DocumentPath = "// fileshare/path",
                DownloadHandlerURL = "https://relativity.com",
                RootArtifactID = 34343,
                MatterArtifactID = 6576,
                ExportAllowed = true,
                RootFolderID = 434,
                EnableDataGrid = false,
                StatusCodeArtifactID = 76756
            };
        }

        private IExternalServiceInstrumentationProvider MockInstrumentation()
        {
            _simpleInstrumentation = Substitute.For<IExternalServiceSimpleInstrumentation>();
            _simpleInstrumentation.Execute(Arg.Any<Func<CaseInfo>>()).Returns(x => x.Arg<Func<CaseInfo>>().Invoke());
            IExternalServiceInstrumentationProvider instrumentationProvider = Substitute.For<IExternalServiceInstrumentationProvider>();
            instrumentationProvider.CreateSimple(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(_simpleInstrumentation);
            return instrumentationProvider;
        }
    }
}
