using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using kCura.IntegrationPoints.Domain.Toggles;
using Moq;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.API.Foundation;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Domain.Tests.Helpers
{
    [TestFixture, Category("Unit")]
    class AppDomainHelperTests
    {
        private IAppDomainHelper _sut;
        private AppDomain _appDomain;
        private FakeRelativityFeaturePathService _relativityFeaturePathService;
        private IHelper _helper;
        private IDictionary<ApplicationBinary, Stream> _fakePluginLibraries;

        private Mock<IPluginProvider> _pluginProviderMock;
        private Mock<IToggleProvider> _toggleProviderMock;
        private Mock<IAPILog> _loggerMock;

        private readonly Guid _applicationGuid = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            _pluginProviderMock = new Mock<IPluginProvider>();
            _helper = Substitute.For<IHelper>();
            _toggleProviderMock = new Mock<IToggleProvider>();
            _loggerMock = new Mock<IAPILog>();
            
            _relativityFeaturePathService = new FakeRelativityFeaturePathService();
            _loggerMock.Object.ForContext<AppDomainHelper>();

            _appDomain = CreateDomain("AppDomainHelperTestsAppDomain", Directory.GetCurrentDirectory());
            
            _sut = new AppDomainHelper(_pluginProviderMock.Object, _helper,
                _relativityFeaturePathService, _toggleProviderMock.Object);
        }

        [Test]
        public void LoadClientLibrariesShallLoadAllRequiredAssemblies()
        {
            // Arrange
            CreateInstance<AssemblyDomainLoader>(new object[0]);
            List<string> expectedAssembliesLocations = GetAssembliesLocations(_appDomain.GetAssemblies().ToList());

            foreach (KeyValuePair<ApplicationBinary, Stream> library in _fakePluginLibraries)
            {
                expectedAssembliesLocations.Add(library.Key.Name);
            }
            expectedAssembliesLocations.Sort();

            //Act
            _sut.LoadClientLibraries(_appDomain, _applicationGuid);
            List<string> loadedAssemblies = GetAssembliesLocations(_appDomain.GetAssemblies().ToList());
            loadedAssemblies.Sort();

            //Assert
            loadedAssemblies.Count().Should().Be(expectedAssembliesLocations.Count());
            loadedAssemblies.ShouldAllBeEquivalentTo(expectedAssembliesLocations);
        }

        [Test]
        public void ReleaseDomainShallRemoveBaseDirectory()
        {
            // Arrange
            string tempDir = Path.GetTempPath() + Guid.NewGuid();
            string currentDir = Directory.GetCurrentDirectory();
            Directory.CreateDirectory(tempDir);

            foreach (string newPath in Directory.GetFiles(currentDir, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(currentDir, tempDir), true);
            }

            AppDomain appDomain = CreateDomain("TestDomain", tempDir);

            // Act
            _sut.ReleaseDomain(appDomain);

            // Assert
            Directory.Exists(tempDir).Should().Be(false);
        }

        [Test]
        public void CreateNewDomainShallRunInKubernetesModeWhenToggleIsTrue()
        {
            // Arrange
            _toggleProviderMock.Setup(x => x.IsEnabled<LoadRequiredAssembliesInKubernetesMode>()).Returns(true);
            
            // Act
            AppDomain appDomain = _sut.CreateNewDomain();

            // Assert
            FakeRelativityFeaturePathService.KubernetesModeExecuted.Should().BeTrue();
            appDomain.Should().NotBeNull();
        }

        [Test]
        public void CreateNewDomainShallNotRunInKubernetesModeAsDefault()
        {
            // Act
            AppDomain appDomain = _sut.CreateNewDomain();

            // Assert
            FakeRelativityFeaturePathService.KubernetesModeExecuted.Should().BeFalse();
            appDomain.Should().NotBeNull();
        }

        [Test]
        public void SetupDomainAndCreateManagerTest()
        {
            // Arrange
            _sut = new AppDomainHelper(_pluginProviderMock.Object, new FakeHelper(),
                _relativityFeaturePathService, _toggleProviderMock.Object);
            Bootstrappers.AppDomainBootstrapper = new Mock<IAppDomainBootstrapper>().Object;

            // Act
            IAppDomainManager manager = _sut.SetupDomainAndCreateManager(_appDomain, _applicationGuid);

            // Assert
            manager.Should().NotBeNull();
        }

        private AppDomain CreateDomain(string domainName, string baseDirectory)
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = baseDirectory;

            AppDomain appDomain = AppDomain.CreateDomain(domainName, null, setup);
            _fakePluginLibraries = CreateFakePluginLibraries(appDomain);
            _pluginProviderMock.Setup(x => x.GetPluginLibraries(_applicationGuid)).Returns(_fakePluginLibraries);

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

        private T CreateInstance<T>(params object[] constructorArgs) where T : class
        {
            Type type = typeof(T);
            T instance = (T) _appDomain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName,
                false, BindingFlags.Default, null, constructorArgs, null,
                null);
            return instance;
        }

        private List<string> GetAssembliesLocations(List<Assembly> assemblies)
        {
            List<string> assembliesLocations = new List<string>();
            foreach (Assembly assembly in assemblies)
            {
                assembliesLocations.Add(assembly.Location);
            }

            return assembliesLocations;
        }

        private class FakeRelativityFeaturePathService : RelativityFeaturePathService
        {
            public static bool KubernetesModeExecuted { get; set; }

            protected override string GetFeaturePathsValue(string keyName)
            {
                KubernetesModeExecuted = true;
                string currentDir = Directory.GetCurrentDirectory();

                if (keyName == "WebPath")
                {
                    return Directory.GetParent(currentDir)?.FullName;
                }

                return currentDir;
            }
        }

        [Serializable]
        private class FakeHelper : IHelper
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public IDBContext GetDBContext(int caseID)
            {
                throw new NotImplementedException();
            }

            public IServicesMgr GetServicesManager()
            {
                throw new NotImplementedException();
            }

            public IUrlHelper GetUrlHelper()
            {
                throw new NotImplementedException();
            }

            public ILogFactory GetLoggerFactory()
            {
                return new FakeLogFactory();
            }

            public string ResourceDBPrepend()
            {
                throw new NotImplementedException();
            }

            public string ResourceDBPrepend(IDBContext context)
            {
                throw new NotImplementedException();
            }

            public string GetSchemalessResourceDataBasePrepend(IDBContext context)
            {
                throw new NotImplementedException();
            }

            public Guid GetGuid(int workspaceID, int artifactID)
            {
                throw new NotImplementedException();
            }

            public ISecretStore GetSecretStore()
            {
                throw new NotImplementedException();
            }

            public IInstanceSettingsBundle GetInstanceSettingBundle()
            {
                throw new NotImplementedException();
            }

            public IStringSanitizer GetStringSanitizer(int workspaceID)
            {
                throw new NotImplementedException();
            }
        }

        private class FakeLogFactory : ILogFactory
        {
            public IAPILog GetLogger()
            {
                return new FakeIAPILog();
            }
        }

        private class FakeIAPILog : IAPILog
        {
            public void LogVerbose(string messageTemplate, params object[] propertyValues)
            {
                
            }

            public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
            {
                
            }

            public void LogDebug(string messageTemplate, params object[] propertyValues)
            {
                
            }

            public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
            {
                
            }

            public void LogInformation(string messageTemplate, params object[] propertyValues)
            {
                
            }

            public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
            {
                
            }

            public void LogWarning(string messageTemplate, params object[] propertyValues)
            {
                
            }

            public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
            {
                
            }

            public void LogError(string messageTemplate, params object[] propertyValues)
            {
                
            }

            public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
            {
                
            }

            public void LogFatal(string messageTemplate, params object[] propertyValues)
            {
                
            }

            public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
            {
                
            }

            public IAPILog ForContext<T>()
            {
                return new FakeIAPILog();
            }

            public IAPILog ForContext(Type source)
            {
                return new FakeIAPILog();
            }

            public IAPILog ForContext(string propertyName, object value, bool destructureObjects)
            {
                return new FakeIAPILog();
            }

            public IDisposable LogContextPushProperty(string propertyName, object obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}