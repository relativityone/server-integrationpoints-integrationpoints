using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Relativity.Kepler.Transport;
using Relativity.Productions.Services;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.LibraryApplication.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.ServiceProxy;
using Relativity.Services.Workspace;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Stubs;
using FieldRef = Relativity.Services.Field.FieldRef;

namespace Relativity.Sync.Tests.System.Core
{
    public sealed class TestEnvironment : IDisposable
    {
        private int _templateWorkspaceArtifactId = -1;
        private const string _RELATIVITY_SYNC_TEST_HELPER_RAP = "Relativity_Sync_Test_Helper.xml";
        private const string _CUSTOM_RELATIVITY_SYNC_TEST_HELPER_RAP = "Custom_Relativity_Sync_Test_Helper.xml";
        private readonly List<WorkspaceRef> _workspaces = new List<WorkspaceRef>();
        private readonly SemaphoreSlim _templateWorkspaceSemaphore = new SemaphoreSlim(1);
        private readonly ServiceFactory _serviceFactory;
        private static readonly Guid _HELPER_APP_GUID = new Guid("e08fd0d9-c3a1-4654-87ad-104f08980b84");
        private static readonly Guid _CUSTOM_HELPER_APP_GUID = new Guid("fdd69e45-880a-40bb-aae1-784271974d49");

        public TestEnvironment()
        {
            _serviceFactory = new ServiceFactoryFromAppConfig().CreateServiceFactory();
        }

        public async Task<WorkspaceRef> CreateWorkspaceAsync(string name = null, string templateWorkspaceName = "Relativity Starter Template")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "SyncTests-" + Guid.NewGuid();
            }

            int templateWorkspaceArtifactId = await GetTemplateWorkspaceArtifactIdAsync(templateWorkspaceName).ConfigureAwait(false);
            using (var workspaceManager = _serviceFactory.CreateProxy<IWorkspaceManager>())
            {
                WorkspaceSetttings settings = new WorkspaceSetttings { Name = name, TemplateArtifactId = templateWorkspaceArtifactId };
                WorkspaceRef newWorkspace = await workspaceManager.CreateWorkspaceAsync(settings).ConfigureAwait(false);
                if (newWorkspace == null)
                {
                    throw new InvalidOperationException("Workspace creation failed. WorkspaceManager kepler service returned null");
                }
                _workspaces.Add(newWorkspace);
                return newWorkspace;
            }
        }

        public async Task<WorkspaceRef> GetWorkspaceAsync(int workspaceId)
        {
            using (var workspaceManager = _serviceFactory.CreateProxy<IWorkspaceManager>())
            {
                IEnumerable<WorkspaceRef> workspaces = await workspaceManager.RetrieveAllActive().ConfigureAwait(false);
                return workspaces.FirstOrDefault(x => x.ArtifactID == workspaceId);
            }
        }

        public async Task<WorkspaceRef> GetWorkspaceAsync(string workspaceName)
        {
            using (var workspaceManager = _serviceFactory.CreateProxy<IWorkspaceManager>())
            {
                IEnumerable<WorkspaceRef> workspaces = await workspaceManager.RetrieveAllActive().ConfigureAwait(false);
                return workspaces.FirstOrDefault(x => x.Name == workspaceName);
            }
        }

        public async Task<WorkspaceRef> CreateWorkspaceWithFieldsAsync(string name = null, string templateWorkspaceName = "Relativity Starter Template")
        {
            WorkspaceRef workspace = await CreateWorkspaceAsync(name, templateWorkspaceName).ConfigureAwait(false);
            await CreateFieldsInWorkspaceAsync(workspace.ArtifactID).ConfigureAwait(false);
            return workspace;
        }

        public async Task DeleteAllDocumentsInWorkspaceAsync(WorkspaceRef workspace)
        {
            var request = new MassDeleteByCriteriaRequest()
            {
                ObjectIdentificationCriteria = new ObjectIdentificationCriteria
                {
                    ObjectType = new ObjectTypeRef
                    {
                        ArtifactTypeID = (int)ArtifactType.Document

                    }
                }
            };

            using (var objectManager = _serviceFactory.CreateProxy<IObjectManager>())
            {
                await objectManager.DeleteAsync(workspace.ArtifactID, request).ConfigureAwait(false);
            }
        }

        private async Task<int> GetTemplateWorkspaceArtifactIdAsync(string templateWorkspaceName)
        {
            try
            {
                await _templateWorkspaceSemaphore.WaitAsync().ConfigureAwait(false);
                if (_templateWorkspaceArtifactId == -1)
                {
                    _templateWorkspaceArtifactId = await GetWorkspaceArtifactIdByNameAsync(templateWorkspaceName).ConfigureAwait(false);
                }
                return _templateWorkspaceArtifactId;
            }
            finally
            {
                _templateWorkspaceSemaphore.Release();
            }
        }

        public async Task<int> GetWorkspaceArtifactIdByNameAsync(string workspaceName)
        {
            using (var objectManager = _serviceFactory.CreateProxy<IObjectManager>())
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { Name = "Workspace" },
                    Condition = $"'Name' == '{workspaceName}'"
                };
                QueryResultSlim result = await objectManager.QuerySlimAsync(-1, request, 0, 1).ConfigureAwait(false);

                if (result.ResultCount == 0)
                {
                    throw new NotFoundException($"Template workspace named: '{workspaceName}' not found.");
                }

                return result.Objects.FirstOrDefault()?.ArtifactID ?? 0;
            }
        }

        public async Task DoCleanupAsync()
        {
            using (var manager = _serviceFactory.CreateProxy<IWorkspaceManager>())
            {
                // ReSharper disable once AccessToDisposedClosure - False positive. We're awaiting all tasks, so we can be sure dispose will be done after each call is handled 
                await Task.WhenAll(_workspaces.Select(w => manager.DeleteAsync(new WorkspaceRef(w.ArtifactID)))).ConfigureAwait(false);
            }
            _workspaces.Clear();
        }

        public async Task<int> CreateProductionAsync(int workspaceID, string productionName)
        {
            var production = new Productions.Services.Production
            {
                Name = productionName,
                Details = new ProductionDetails
                {
                    BrandingFontSize = 10,
                    ScaleBrandingFont = false
                },
                Numbering = new DocumentFieldNumbering
                {
                    NumberingType = NumberingType.DocumentField,
                    NumberingField = new FieldRef
                    {
                        ArtifactID = 1003667,
                        ViewFieldID = 0,
                        Name = "Control Number"
                    },
                    AttachmentRelationalField = new FieldRef
                    {
                        ArtifactID = 0,
                        ViewFieldID = 0,
                        Name = ""
                    },
                    BatesPrefix = "PRE",
                    BatesSuffix = "SUF",
                    IncludePageNumbers = false,
                    DocumentNumberPageNumberSeparator = "",
                    NumberOfDigitsForPageNumbering = 0,
                    StartNumberingOnSecondPage = false
                }
            };

            using (var productionManager = _serviceFactory.CreateProxy<IProductionManager>())
            {
                return await productionManager.CreateSingleAsync(workspaceID, production).ConfigureAwait(false);
            }
        }

        public async Task CreateFieldsInWorkspaceAsync(int workspaceArtifactId)
        {
            await InstallHelperAppIfNeededAsync(_RELATIVITY_SYNC_TEST_HELPER_RAP, _HELPER_APP_GUID).ConfigureAwait(false);
            await InstallApplicationFromLibraryToWorkspaceAsync(workspaceArtifactId, _HELPER_APP_GUID);

            await EnsureRdosExistsAsync(workspaceArtifactId).ConfigureAwait(false);
        }

        public async Task InstallCustomHelperAppAsync(int workspaceArtifactId)
        {
            await InstallHelperAppIfNeededAsync(_CUSTOM_RELATIVITY_SYNC_TEST_HELPER_RAP, _CUSTOM_HELPER_APP_GUID).ConfigureAwait(false);
            await InstallApplicationFromLibraryToWorkspaceAsync(workspaceArtifactId, _CUSTOM_HELPER_APP_GUID);

            await EnsureRdosExistsAsync(workspaceArtifactId).ConfigureAwait(false);
        }

        public async Task InstallLegalHoldToWorkspaceAsync(int workspaceArtifactId)
        {
            Guid legalHoldGuid = new Guid("98f31698-90a0-4ead-87e3-dac723fed2a6");
            await InstallApplicationFromLibraryToWorkspaceAsync(workspaceArtifactId, legalHoldGuid).ConfigureAwait(false);
        }

        private async Task InstallApplicationFromLibraryToWorkspaceAsync(int workspaceArtifactId, Guid appGuid)
        {
            using (var applicationInstallManager =
                _serviceFactory.CreateProxy<Services.Interfaces.LibraryApplication.IApplicationInstallManager>())
            {
                var installApplicationRequest = new InstallApplicationRequest
                {
                    WorkspaceIDs = new List<int> { workspaceArtifactId }
                };
                InstallApplicationResponse install = await applicationInstallManager
                    .InstallApplicationAsync(-1, appGuid, installApplicationRequest).ConfigureAwait(false);
                int applicationInstallId = install.Results.First().ApplicationInstallID;
                InstallStatusCode installStatusCode;
                do
                {
                    await Task.Yield();
                    GetInstallStatusResponse status = await applicationInstallManager
                        .GetStatusAsync(-1, appGuid, applicationInstallId).ConfigureAwait(false);
                    installStatusCode = status.InstallStatus.Code;
                } while (installStatusCode == InstallStatusCode.Pending || installStatusCode == InstallStatusCode.InProgress);
            }
        }

        private static async Task EnsureRdosExistsAsync(int workspaceArtifactId)
        {
            var rdoManager = new RdoManager(TestLogHelper.GetLogger(), new SourceServiceFactoryStub(), new RdoGuidProvider());
            await rdoManager.EnsureTypeExistsAsync<SyncConfigurationRdo>(workspaceArtifactId).ConfigureAwait(false);
            await rdoManager.EnsureTypeExistsAsync<SyncProgressRdo>(workspaceArtifactId).ConfigureAwait(false);
            await rdoManager.EnsureTypeExistsAsync<SyncBatchRdo>(workspaceArtifactId).ConfigureAwait(false);
            await rdoManager.EnsureTypeExistsAsync<SyncStatisticsRdo>(workspaceArtifactId).ConfigureAwait(false);
        }

        private async Task InstallHelperAppIfNeededAsync(string appFileName, Guid appGuid)
        {
            using (var fileStream = GetHelperApplicationXml(appFileName))
            using (var applicationLibraryManager = _serviceFactory.CreateProxy<Services.Interfaces.LibraryApplication.ILibraryApplicationManager>())
            {
                List<LibraryApplicationResponse> apps = await applicationLibraryManager.ReadAllAsync(-1).ConfigureAwait(false);
                LibraryApplicationResponse libraryApp = apps.FirstOrDefault(app => app.Guids.Contains(appGuid));
                var libraryAppVersion = new Version(libraryApp?.Version ?? "0.0.0.0");
                Version appXmlAppVersion = GetVersionFromApplicationXmlStream(fileStream);

                if (libraryAppVersion < appXmlAppVersion)
                {
                    // Rewinding stream as it will be reused. 
                    fileStream.Seek(0, SeekOrigin.Begin);
                    using (var outStream = await CreateRapFileInMemoryAsync(fileStream).ConfigureAwait(false))
                    using (var keplerStream = new KeplerStream(outStream))
                    {
                        var updateLibraryApplicationRequest = new UpdateLibraryApplicationRequest
                        {
                            CreateIfMissing = true,
                            FileName = Path.ChangeExtension(appFileName, "rap"),
                            IgnoreVersion = true,
                            RefreshCustomPages = false
                        };
                        await applicationLibraryManager.UpdateAsync(-1, keplerStream, updateLibraryApplicationRequest).ConfigureAwait(false);
                    }
                }
            }
        }

        private static async Task<MemoryStream> CreateRapFileInMemoryAsync(Stream applicationXmlFileStream)
        {
            MemoryStream outStream = new MemoryStream();
            using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
            {
                ZipArchiveEntry fileEntry = archive.CreateEntry("application.xml", CompressionLevel.Fastest);
                using (var entryStream = fileEntry.Open())
                {
                    await applicationXmlFileStream.CopyToAsync(entryStream).ConfigureAwait(false);
                }
            }

            // Rewinding stream as it is meant to be reused. 
            outStream.Seek(0, SeekOrigin.Begin);
            return outStream;
        }

        private static Version GetVersionFromApplicationXmlStream(Stream fileStream)
        {
            XmlDocument appXml = SafeLoadXml(fileStream);
            string versionStringFromXml = appXml?.SelectSingleNode("//Version")?.InnerText;
            if (versionStringFromXml == null)
            {
                throw new InvalidOperationException("Application XML could not be parsed. Cannot find Version node.");
            }

            Version appXmlAppVersion = new Version(versionStringFromXml);
            return appXmlAppVersion;
        }

        /// <summary> 
        /// To prevent insecure DTD processing we have to load the XML in a specific way. 
        /// See https://docs.microsoft.com/en-us/visualstudio/code-quality/ca3075-insecure-dtd-processing?view=vs-2017 for more info 
        /// </summary> 
        /// <param name="fileStream">Stream to be read</param> 
        /// <returns>XML document loaded from given stream</returns> 
        private static XmlDocument SafeLoadXml(Stream fileStream)
        {
            var xmlReaderSettings = new XmlReaderSettings { XmlResolver = null };
            XmlReader reader = XmlReader.Create(fileStream, xmlReaderSettings);
            XmlDocument appXml = new XmlDocument { XmlResolver = null };
            appXml.Load(reader);
            return appXml;
        }

        private static Stream GetHelperApplicationXml(string appResourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string resource = $"Relativity.Sync.Tests.System.Core.Resources.{appResourceName}";
            return asm.GetManifestResourceStream(resource);
        }

        public void Dispose()
        {
            _templateWorkspaceSemaphore.Dispose();
        }
    }
}
