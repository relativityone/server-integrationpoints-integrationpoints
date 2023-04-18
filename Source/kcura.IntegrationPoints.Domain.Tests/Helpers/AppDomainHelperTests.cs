using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Domain.Toggles;
using kCura.IntegrationPoints.Tests.Core.Helpers;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.API.Foundation;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Domain.Tests.Helpers
{
    [TestFixture, Category("Unit")]
    [NonParallelizable]
    [Ignore("TODO: Fix Broken test later")]
    class AppDomainHelperTests
    {
        private IAppDomainHelper _sut;
        private AppDomain _appDomain;
        private FakeRelativityFeaturePathService _relativityFeaturePathService;
        private IHelper _fakeHelper;
        private IDictionary<ApplicationBinary, Stream> _fakePluginLibraries;
        private string _currentDir;
        private string _tempDir;
        private bool _appDomainReleased;

        private Mock<IPluginProvider> _pluginProviderFake;
        private Mock<IToggleProvider> _toggleProviderFake;

        private readonly Guid _applicationGuid = Guid.NewGuid();
        
        [SetUp]
        public void Setup()
        {
            _pluginProviderFake = new Mock<IPluginProvider>();
            _toggleProviderFake = new Mock<IToggleProvider>();

            Mock<IAPILog> loggerFake = new Mock<IAPILog>();
            loggerFake.Setup(x => x.ForContext<AppDomainHelper>()).Returns(loggerFake.Object);
            loggerFake.Setup(x => x.LogWarning(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback<Exception, string, object[]>((exception, messageTemplate, propertyValues) =>
                    Console.WriteLine(
                        $"Message: {messageTemplate}, Properties: {string.Join(",", propertyValues)}, Error: {exception}"));

            Mock<ILogFactory> logFactoryFake = new Mock<ILogFactory>();
            logFactoryFake.Setup(x => x.GetLogger()).Returns(loggerFake.Object);

            Mock<IHelper> helperFake = new Mock<IHelper>();
            helperFake.Setup(x => x.GetLoggerFactory()).Returns(logFactoryFake.Object);

            _fakeHelper = new FakeHelper(helperFake);
            _relativityFeaturePathService = new FakeRelativityFeaturePathService();
            _tempDir = Path.GetTempPath() + Guid.NewGuid();
            _currentDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).ToString();
            _appDomainReleased = false;

            _appDomain = CreateDomain($"AppDomainHelperTestsAppDomain-{Guid.NewGuid():D}");
            
            _sut = new AppDomainHelper(_pluginProviderFake.Object, _fakeHelper,
                _relativityFeaturePathService, _toggleProviderFake.Object);
        }

        [TearDown]
        public void CleanUp()
        {
            if (!_appDomainReleased)
            {
                _sut.ReleaseDomain(_appDomain);
            }
        }

        [Test, Order(5)]
        public void LoadClientLibrariesShallLoadAllRequiredAssemblies()
        {
            // Arrange
            _sut.CreateInstance<AssemblyDomainLoader>(_appDomain, new object[0]);
            List<string> expectedAssembliesLocations = _appDomain.GetAssemblies().Select(x => x.Location).ToList();

            foreach (KeyValuePair<ApplicationBinary, Stream> library in _fakePluginLibraries)
            {
                string assemblyLocation = library.Key.Name;
                string expectedLocation = assemblyLocation.Replace(Directory.GetParent(library.Key.Name).ToString(),
                    _currentDir);
                expectedAssembliesLocations.Add(expectedLocation);
            }

            //Act
            _sut.LoadClientLibraries(_appDomain, _applicationGuid);
            List<string> loadedAssemblies = _appDomain.GetAssemblies().Select(x => x.Location).ToList();
            
            //Assert
            loadedAssemblies.Count().Should().Be(expectedAssembliesLocations.Count());
            loadedAssemblies.ShouldAllBeEquivalentTo(expectedAssembliesLocations);
        }

        [Test, Order(1)]
        public void ReleaseDomainShallRemoveBaseDirectory()
        {
            // Act
            _sut.ReleaseDomain(_appDomain);
            _appDomainReleased = true;

            // Assert
            Directory.Exists(_tempDir).Should().Be(false);
        }

        [Test, Order(2)]
        public void CreateNewDomainShallRunInKubernetesModeWhenToggleIsTrue()
        {
            // Arrange
            _toggleProviderFake.Setup(x => x.IsEnabled<EnableKubernetesMode>()).Returns(true);
            
            // Act
            AppDomain appDomain = _sut.CreateNewDomain();

            // Assert
            FakeRelativityFeaturePathService.KubernetesModeNotExecuted.Should().BeFalse();
            appDomain.Should().NotBeNull();
        }

        [Test, Order(3)]
        public void CreateNewDomainShallNotRunInKubernetesModeAsDefault()
        {
            // Act
            AppDomain appDomain = _sut.CreateNewDomain();

            // Assert
            FakeRelativityFeaturePathService.KubernetesModeNotExecuted.Should().BeTrue();
            appDomain.Should().NotBeNull();
        }

        [Test, Order(4)]
        public void SetupDomainAndCreateManagerShallBeCreated()
        {
            // Arrange
            Bootstrappers.AppDomainBootstrapper = new Mock<IAppDomainBootstrapper>().Object;

            // Act
            IAppDomainManager manager = _sut.SetupDomainAndCreateManager(_appDomain, _applicationGuid);
            // Assert
            manager.Should().NotBeNull();
        }

        private AppDomain CreateDomain(string domainName)
        {
            
            Directory.CreateDirectory(_tempDir);

            foreach (string dirPath in Directory.GetDirectories(_currentDir, "*.*", SearchOption.AllDirectories))
            {
                string newPath = dirPath.Replace(_currentDir, _tempDir);
                Directory.CreateDirectory(newPath);
            }

            foreach (string path in Directory.GetFiles(_currentDir, "*.*", SearchOption.AllDirectories))
            {
                string newPath = path.Replace(_currentDir, _tempDir);
                File.Copy(path, newPath, true);
            }

            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = _tempDir;

            AppDomain appDomain = AppDomain.CreateDomain(domainName, null, setup);
            _fakePluginLibraries = CreateFakePluginLibraries(appDomain);
            _pluginProviderFake.Setup(x => x.GetPluginLibraries(_applicationGuid)).Returns(_fakePluginLibraries);

            return appDomain;
        }

        private IDictionary<ApplicationBinary, Stream> CreateFakePluginLibraries(AppDomain appDomain)
        {
            IDictionary<ApplicationBinary, Stream> fakePluginLibraries = new Dictionary<ApplicationBinary, Stream>();
            List<string> sourceFiles = Directory.GetFiles(appDomain.BaseDirectory).Where(x => x.Contains(".dll")).Take(3).ToList();

            for (int i = 0; i < sourceFiles.Count(); i++)
            {
                fakePluginLibraries.Add(new ApplicationBinary { ArtifactID = i, Name = sourceFiles[i] }, new MemoryStream());
            }

            return fakePluginLibraries;
        }

        private class FakeRelativityFeaturePathService : RelativityFeaturePathService
        {
            public static bool KubernetesModeNotExecuted { get; set; }

            public FakeRelativityFeaturePathService()
            {
                KubernetesModeNotExecuted = false;
            }

            protected override string GetFeaturePathsValue(string keyName)
            {
                KubernetesModeNotExecuted = true;
                string currentDir = AppDomain.CurrentDomain.BaseDirectory;

                if (keyName == "WebPath")
                {
                    return Directory.GetParent(Directory.GetParent(currentDir)?.FullName)?.FullName;
                }

                return currentDir;
            }
        }
    }
}