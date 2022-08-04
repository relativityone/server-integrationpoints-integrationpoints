using System;
using System.IO;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.Domain
{
    [TestFixture, Category("Unit")]
    public class RelativityFeaturePathServiceTests : TestBase
    {
        [SetUp]
        public override void SetUp()
        {
            
        }

        [Test]
        public void GetFeaturePathsValue_Relativity91AndPrior_CorrectValues()
        {
            //ARRANGE
            //ACT
            MockService mockService = getDefaultMockService();
            mockService.NewRegistryStructure = false;

            //ASSERT
            Assert.AreEqual("AgentPath", mockService.AgentPath);
            Assert.AreEqual("WebPath", mockService.EddsPath);
            Assert.AreEqual("LibraryPath", mockService.LibraryPath);
            Assert.AreEqual("WebProcessingPath", mockService.WebProcessingPath);
        }

        [Test]
        public void GetFeaturePathsValue_Relativity91AndPriorOnDevEnvironment_ExpectedException()
        {
            //ARRANGE
            //ACT
            MockService mockService = NSubstitute.Substitute.For<MockService>();
            mockService.NewRegistryStructure = false;
            mockService.LibraryPath = null;
            mockService.GetFeaturePathsValueOverride(Arg.Any<string>()).Returns(string.Empty);

            //ASSERT
            Assert.Throws<Exception>(() => { string path = mockService.LibraryPath; }, "Could not retrieve LibraryPath.");
        }

        [Test]
        public void GetFeaturePathsValue_Relativity92AndUpRunningOnWebServer_CorrectValues()
        {
            //ARRANGE
            //ACT
            MockService2 mockService =  new MockService2();
            mockService.NewRegistryStructure = true;
            mockService.BaseInstallDirOverride = "XXX";
            mockService.DirectoryExistOverride = true;

            //ASSERT
            Assert.AreEqual("XXX\\Agents", mockService.AgentPath);
            Assert.AreEqual("XXX\\EDDS", mockService.EddsPath);
            Assert.AreEqual("XXX\\Library", mockService.LibraryPath);
            Assert.AreEqual("XXX\\WebProcessing", mockService.WebProcessingPath);
        }

        [Test]
        public void GetFeaturePathsValue_Relativity92AndUpRunningOnAgent_CorrectValues()
        {
            //ARRANGE
            //ACT
            MockService2 mockService = new MockService2();
            mockService.NewRegistryStructure = true;
            mockService.BaseInstallDirOverride = "XXX";

            //ASSERT
            mockService.DirectoryExistOverride = true;
            Assert.AreEqual("XXX\\Agents", mockService.AgentPath);
            mockService.DirectoryExistOverride = false;
            Assert.AreEqual("", mockService.EddsPath);
            mockService.DirectoryExistOverride = true;
            Assert.AreEqual("XXX\\Library", mockService.LibraryPath);
            mockService.DirectoryExistOverride = false;
            Assert.AreEqual("", mockService.WebProcessingPath);
        }

        [Test]
        public void GetFeaturePathsValue_CallNewRegistryStructureTwice_ValueRetrievedOnce()
        {
        }

        [Test]
        public void GetFeaturePathsValue_CallWebProcessingPathTwice_ValueRetrievedOnce()
        {
        }

        [Test]
        public void GetFeaturePathsValue_CallEddsPathTwice_ValueRetrievedOnce()
        {
        }

        [Test]
        public void GetFeaturePathsValue_CallAgentPathTwice_ValueRetrievedOnce()
        {
        }

        [Test]
        public void GetFeaturePathsValue_CallLibraryPathTwice_ValueRetrievedOnce()
        {
        }

        private MockService getDefaultMockService()
        {
            return new MockService()
            {
                DevEnvironmentLibPath = Path.Combine("c:\\", Guid.NewGuid().ToString())
            };
        }
    }

    public class MockService : RelativityFeaturePathService
    {
        public MockService()
        {

        }

        public string BaseInstallDirOverride
        {
            set { base._baseInstallDir = value; }
        }

        public string DevEnvironmentLibPath { get; set; }

        public virtual bool DirectoryExistOverride(string path)
        {
            return base.DirectoryExists(path);
        }

        public virtual string GetFeaturePathsValueOverride(string keyName)
        {
            return keyName;
        }

        protected override bool DirectoryExists(string path)
        {
            return this.DirectoryExistOverride(path);
        }

        protected override string GetFeaturePathsValue(string keyName)
        {
            return GetFeaturePathsValueOverride(keyName);
        }

        protected override string GetDevEnvironmentLibPath()
        {
            return DevEnvironmentLibPath;
        }
    }

    public class MockService2 : RelativityFeaturePathService
    {
        public MockService2()
        {

        }

        public string BaseInstallDirOverride
        {
            set { base._baseInstallDir = value; }
        }

        public string DevEnvironmentLibPath { get; set; }

        public bool DirectoryExistOverride { get; set; }

        protected override bool DirectoryExists(string path)
        {
            return this.DirectoryExistOverride;
        }

        protected override string GetDevEnvironmentLibPath()
        {
            return DevEnvironmentLibPath;
        }

        protected override string GetFeaturePathsValue(string keyName)
        {
            return base.GetFeaturePathsValue(keyName);
        }
    }
}
