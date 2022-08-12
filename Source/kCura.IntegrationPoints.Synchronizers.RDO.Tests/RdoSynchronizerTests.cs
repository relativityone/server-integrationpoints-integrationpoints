using kCura.Apps.Common.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.Relativity.ImportAPI;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Domain.Logging;
using Moq;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Internals;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Objects.DataContracts;
using Assert = NUnit.Framework.Assert;
using Constants = kCura.IntegrationPoints.Domain.Constants;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
    [TestFixture, Category("Unit")]
    public class RdoSynchronizerTests : TestBase
    {
        private Mock<IHelper> _helper;
        private Mock<IImportJobFactory> _importJobFactory;
        private Mock<IObjectTypeRepository> _objectTypeRepository;
        private Mock<IRelativityFieldQuery> _relativityFieldQuery;
        private Mock<IDiagnosticLog> _diagnosticLogMock;

        public static RdoSynchronizer ChangeWebAPIPath(RdoSynchronizer synchronizer)
        {
            PropertyInfo prop = synchronizer.GetType().GetProperty(Constants.WEB_API_PATH);
            prop.SetValue(synchronizer, "Mock value");
            return synchronizer;
        }

        public static Mock<IHelper> MockHelper()
        {
            Mock<IAPILog> logger = new Mock<IAPILog>();
            logger.Setup(x => x.ForContext<RdoEntitySynchronizer>()).Returns(logger.Object);
            logger.Setup(x => x.ForContext<RdoSynchronizer>()).Returns(logger.Object);
            logger.Setup(x => x.ForContext<ImportService>()).Returns(logger.Object);
            Mock<ILogFactory> logFactory = new Mock<ILogFactory>();
            logFactory.Setup(x => x.GetLogger()).Returns(logger.Object);
            var helper = new Mock<IHelper>();
            helper.Setup(x => x.GetLoggerFactory()).Returns(logFactory.Object);
            return helper;
        }

        [SetUp]
        public override void SetUp()
        {
            _objectTypeRepository = new Mock<IObjectTypeRepository>();
            _relativityFieldQuery = new Mock<IRelativityFieldQuery>();
            _importJobFactory = new Mock<IImportJobFactory>();
            _helper = MockHelper();
            _diagnosticLogMock = new Mock<IDiagnosticLog>();
        }

        private RdoSynchronizer PrepareSut()
        {
            return ChangeWebAPIPath(
                new RdoSynchronizer(
                    _relativityFieldQuery.Object,
                    RdoEntitySynchronizerTests.GetMockAPI(_relativityFieldQuery.Object),
                    _importJobFactory.Object,
                    _helper.Object,
                    _diagnosticLogMock.Object));
        }

        [Test]
        public void GetRightCountOfFieldsWithSystemAndArtifactFieldsRemoved()
        {
            //ARRANGE

            var options = new ImportSettings
            {
                ArtifactTypeId = 1268820
            };

            var fields = new List<RelativityObject>
            {
                new RelativityObject {Name = "Name", ArtifactID = 1},
                new RelativityObject {Name = "System Created On", ArtifactID = 2},
                new RelativityObject {Name = "Date Modified On", ArtifactID = 3},
                new RelativityObject {Name = "User", ArtifactID = 4},
                new RelativityObject {Name = "Artifact ID", ArtifactID = 5}
            };

            _relativityFieldQuery.Setup(x => x.GetFieldsForRdo(It.IsAny<int>())).Returns(fields);

            RdoSynchronizer sut = PrepareSut();
            string optionsStr = JsonConvert.SerializeObject(options);

            //ACT
            int numberOfFields = sut.GetFields(new DataSourceProviderConfiguration(optionsStr)).Count();

            //ASSERT
            Assert.AreEqual(3, numberOfFields);
        }

        [Test]
        public void GetRightDataInFieldsWithSystemAndArtifactFieldsRemoved()
        {
            //ARRANGE

            _objectTypeRepository.Setup(x => x.GetObjectType(It.IsAny<int>())).Returns(new ObjectTypeDTO
            {
                DescriptorArtifactTypeId = 1,
                Name = "Document"
            });

            var options = new ImportSettings
            {
                ArtifactTypeId = 1268820
            };

            _relativityFieldQuery.Setup(x => x.GetFieldsForRdo(It.IsAny<int>())).Returns(new List<RelativityObject>
                {
                    new RelativityObject {Name = "Name", ArtifactID = 1},
                    new RelativityObject {Name = "System Created On", ArtifactID = 2},
                    new RelativityObject {Name = "Date Modified On", ArtifactID = 3},
                    new RelativityObject {Name = "User", ArtifactID = 4},
                    new RelativityObject {Name = "Artifact ID", ArtifactID = 5}
                });

            var expectedFieldEntry = new List<FieldEntry>
                {
                    new FieldEntry {DisplayName = "Name", FieldIdentifier = "1"},
                    new FieldEntry {DisplayName = "Date Modified On", FieldIdentifier = "3"},
                    new FieldEntry {DisplayName = "User", FieldIdentifier = "4"},
                };

            string optionsStr = JsonConvert.SerializeObject(options);
            RdoSynchronizer sut = PrepareSut();

            //ACT
            List<FieldEntry> listOfFieldEntry = sut.GetFields(new DataSourceProviderConfiguration(optionsStr)).ToList();

            //ASSERT
            Assert.AreEqual(expectedFieldEntry.Count, listOfFieldEntry.Count);
            for (var i = 0; i < listOfFieldEntry.Count; i++)
            {
                Assert.AreEqual(listOfFieldEntry[i].DisplayName, expectedFieldEntry[i].DisplayName);
                Assert.AreEqual(listOfFieldEntry[i].FieldIdentifier, expectedFieldEntry[i].FieldIdentifier);
            }
        }

        [Test]
        public void GetRightCountOfFields()
        {
            //ARRANGE
            _objectTypeRepository.Setup(x => x.GetObjectType(It.IsAny<int>())).Returns(new ObjectTypeDTO
            {
                DescriptorArtifactTypeId = 1,
                Name = "Document"
            });

            var options = new ImportSettings
            {
                ArtifactTypeId = 1268820
            };

            _relativityFieldQuery.Setup(x => x.GetFieldsForRdo(It.IsAny<int>())).Returns(new List<RelativityObject>
                {
                    new RelativityObject {Name = "Name", ArtifactID = 1},
                    new RelativityObject {Name = "Value", ArtifactID = 2},
                    new RelativityObject {Name = "Date Modified On", ArtifactID = 3},
                    new RelativityObject {Name = "User", ArtifactID = 4},
                    new RelativityObject {Name = "FirstName", ArtifactID = 5}
                });

            string optionsStr = JsonConvert.SerializeObject(options);
            RdoSynchronizer sut = PrepareSut();

            //ACT
            int numberOfFields = sut.GetFields(new DataSourceProviderConfiguration(optionsStr)).Count();

            //ASSERT
            Assert.AreEqual(5, numberOfFields);
        }

        [Test]
        public void GetRightDataInFields()
        {
            //ARRANGE
            _objectTypeRepository.Setup(x => x.GetObjectType(It.IsAny<int>())).Returns(new ObjectTypeDTO
            {
                DescriptorArtifactTypeId = 1,
                Name = "Document"
            });

            var options = new ImportSettings
            {
                ArtifactTypeId = 1268820
            };

            _relativityFieldQuery.Setup(x => x.GetFieldsForRdo(It.IsAny<int>())).Returns(new List<RelativityObject>
                {
                    new RelativityObject {Name = "Name", ArtifactID = 1},
                    new RelativityObject {Name = "Value", ArtifactID = 2},
                    new RelativityObject {Name = "Date Modified On", ArtifactID = 3},
                    new RelativityObject {Name = "User", ArtifactID = 4},
                    new RelativityObject {Name = "FirstName", ArtifactID = 5}
                });

            var expectedFieldEntry = new List<FieldEntry>
                {
                    new FieldEntry {DisplayName = "Name", FieldIdentifier = "1"},
                    new FieldEntry {DisplayName = "Value", FieldIdentifier = "2"},
                    new FieldEntry {DisplayName = "Date Modified On", FieldIdentifier = "3"},
                    new FieldEntry {DisplayName = "User", FieldIdentifier = "4"},
                    new FieldEntry {DisplayName = "FirstName", FieldIdentifier = "5"}
                };

            string optionsStr = JsonConvert.SerializeObject(options);
            RdoSynchronizer sut = PrepareSut();

            //ACT
            List<FieldEntry> listOfFieldEntry = sut.GetFields(new DataSourceProviderConfiguration(optionsStr)).ToList();

            //ASSERT
            Assert.AreEqual(expectedFieldEntry.Count, listOfFieldEntry.Count);
            for (var i = 0; i < listOfFieldEntry.Count; i++)
            {
                Assert.AreEqual(listOfFieldEntry[i].DisplayName, expectedFieldEntry[i].DisplayName);
                Assert.AreEqual(listOfFieldEntry[i].FieldIdentifier, expectedFieldEntry[i].FieldIdentifier);
            }
        }

        [Test]
        public void GetSyncDataImportSettings_NoNativeFileImport_CorrectResult()
        {
            //ARRANGE
            IEnumerable<FieldMap> fieldMap = new List<FieldMap>()
                {
                    new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000001"}, FieldMapType = FieldMapTypeEnum.Identifier, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld1"}},
                    new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000002"}, FieldMapType = FieldMapTypeEnum.Parent, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld2"}},
                    new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000003"}, FieldMapType = FieldMapTypeEnum.None, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld3"}},
                };

            NativeFileImportService nativeFileImportService = new NativeFileImportService();
            string options = JsonConvert.SerializeObject(new ImportSettings
            {
                ArtifactTypeId = 1111111,
                CaseArtifactId = 2222222,
                ImportNativeFile = false,
                ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.DoNotImportNativeFiles
            });
            TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

            //ACT
            ImportSettings result = rdoSynchronizer.GetSyncDataImportSettings(fieldMap, options, nativeFileImportService);

            //ASSERT
            Assert.AreEqual(1111111, result.ArtifactTypeId);
            Assert.AreEqual(2222222, result.CaseArtifactId);
            Assert.AreEqual(4000001, result.IdentityFieldId);
            Assert.AreEqual("SourceFld2", result.ParentObjectIdSourceFieldName);
            Assert.AreEqual(ImportNativeFileCopyModeEnum.DoNotImportNativeFiles, result.ImportNativeFileCopyMode);
            Assert.AreEqual(false, result.CopyFilesToDocumentRepository);
            Assert.AreEqual(result.NativeFilePathSourceFieldName, string.Empty);
            Assert.IsFalse(result.DisableNativeLocationValidation.HasValue);
            Assert.IsFalse(result.DisableNativeValidation.HasValue);
            Assert.AreEqual("NATIVE_FILE_PATH_001", nativeFileImportService.DestinationFieldName);
            Assert.IsNull(nativeFileImportService.SourceFieldName);
            Assert.IsFalse(nativeFileImportService.ImportNativeFiles);
        }

        [Test]
        public void GetSyncDataImportSettings_SetFileLinks_CorrectResult()
        {
            //ARRANGE
            IEnumerable<FieldMap> fieldMap = new List<FieldMap>()
                {
                    new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000001"}, FieldMapType = FieldMapTypeEnum.Identifier, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld1"}},
                    new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000002"}, FieldMapType = FieldMapTypeEnum.Parent, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld2"}},
                    new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000003"}, FieldMapType = FieldMapTypeEnum.None, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld3"}},
                };

            NativeFileImportService nativeFileImportService = new NativeFileImportService();
            var importSettings = new ImportSettings
            {
                ArtifactTypeId = 1111111,
                CaseArtifactId = 2222222,
                ImportNativeFile = false,
                ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.SetFileLinks
            };
            string options = JsonConvert.SerializeObject(importSettings);
            TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();
            rdoSynchronizer.SourceProvider = new Data.SourceProvider()
            {
                Config = new SourceProviderConfiguration()
                {
                    AlwaysImportNativeFiles = true
                }
            };

            //ACT
            ImportSettings result = rdoSynchronizer.GetSyncDataImportSettings(fieldMap, options, nativeFileImportService);

            //ASSERT
            Assert.AreEqual(1111111, result.ArtifactTypeId);
            Assert.AreEqual(2222222, result.CaseArtifactId);
            Assert.AreEqual(4000001, result.IdentityFieldId);
            Assert.AreEqual("SourceFld2", result.ParentObjectIdSourceFieldName);
            Assert.AreEqual(ImportNativeFileCopyModeEnum.SetFileLinks, result.ImportNativeFileCopyMode);
            Assert.AreEqual(false, result.CopyFilesToDocumentRepository);
            Assert.IsFalse(result.DisableNativeLocationValidation.Value);
            Assert.IsFalse(result.DisableNativeValidation.Value);
            Assert.AreEqual("NATIVE_FILE_PATH_001", result.NativeFilePathSourceFieldName);
            Assert.AreEqual("NATIVE_FILE_PATH_001", nativeFileImportService.DestinationFieldName);
            Assert.AreEqual(Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD, nativeFileImportService.SourceFieldName);
            Assert.IsTrue(nativeFileImportService.ImportNativeFiles);
        }

        [Test]
        public void GetSyncDataImportSettings_NativeFileImport_CorrectResult()
        {
            //ARRANGE
            IEnumerable<FieldMap> fieldMap = new List<FieldMap>()
                {
                    new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000001"}, FieldMapType = FieldMapTypeEnum.Identifier, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld1"}},
                    new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000002"}, FieldMapType = FieldMapTypeEnum.Parent, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld2"}},
                    new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000003"}, FieldMapType = FieldMapTypeEnum.None, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld3"}},
                    new FieldMap() {DestinationField = null, FieldMapType = FieldMapTypeEnum.NativeFilePath, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld4"}},
                };

            var nativeFileImportService = new NativeFileImportService();
            var importSettings = new ImportSettings
            {
                ArtifactTypeId = 1111111,
                CaseArtifactId = 2222222,
                ImportNativeFile = true,
                ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.CopyFiles
            };
            string options = JsonConvert.SerializeObject(importSettings);
            TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

            //ACT
            ImportSettings result = rdoSynchronizer.GetSyncDataImportSettings(fieldMap, options, nativeFileImportService);

            //ASSERT
            Assert.AreEqual(1111111, result.ArtifactTypeId);
            Assert.AreEqual(2222222, result.CaseArtifactId);
            Assert.AreEqual(4000001, result.IdentityFieldId);
            Assert.AreEqual("SourceFld2", result.ParentObjectIdSourceFieldName);
            Assert.AreEqual(ImportNativeFileCopyModeEnum.CopyFiles, result.ImportNativeFileCopyMode);
            Assert.AreEqual(true, result.CopyFilesToDocumentRepository);
            Assert.IsFalse(result.DisableNativeLocationValidation.Value);
            Assert.IsFalse(result.DisableNativeValidation.Value);
            Assert.AreEqual("NATIVE_FILE_PATH_001", result.NativeFilePathSourceFieldName);
            Assert.AreEqual("NATIVE_FILE_PATH_001", nativeFileImportService.DestinationFieldName);
            Assert.AreEqual("SourceFld4", nativeFileImportService.SourceFieldName);
            Assert.IsTrue(nativeFileImportService.ImportNativeFiles);
        }

        [Test]
        public void GetSyncDataImportSettings_NoFolderInformationPath_CorrectResult()
        {
            //ARRANGE
            IEnumerable<FieldMap> fieldMap = new List<FieldMap>()
                {
                    new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000001"}, FieldMapType = FieldMapTypeEnum.Identifier, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld1"}},
                    new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000002"}, FieldMapType = FieldMapTypeEnum.Parent, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld2"}},
                    new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000003"}, FieldMapType = FieldMapTypeEnum.None, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld3"}},
                };

            NativeFileImportService nativeFileImportService = new NativeFileImportService();
            string options = JsonConvert.SerializeObject(new ImportSettings { ArtifactTypeId = 1111111, CaseArtifactId = 2222222 });
            TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

            //ACT
            ImportSettings result = rdoSynchronizer.GetSyncDataImportSettings(fieldMap, options, nativeFileImportService);

            //ASSERT
            Assert.IsNull(result.FolderPathSourceFieldName);
            Assert.AreEqual(0, result.DestinationFolderArtifactId);
        }

        [Test]
        public void GetSyncDataImportSettings_FolderInformationPath_CorrectResult()
        {
            //ARRANGE
            IEnumerable<FieldMap> fieldMap = new List<FieldMap>()
                {
                    new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000001"}, FieldMapType = FieldMapTypeEnum.Identifier, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld1"}},
                    new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000002"}, FieldMapType = FieldMapTypeEnum.Parent, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld2"}},
                    new FieldMap() {DestinationField = new FieldEntry(){FieldIdentifier = "4000003"}, FieldMapType = FieldMapTypeEnum.None, SourceField = new FieldEntry() {FieldIdentifier = "SourceFld3"}},
                    new FieldMap() {DestinationField = null, FieldMapType = FieldMapTypeEnum.FolderPathInformation, SourceField = new FieldEntry() {DisplayName = "SourceFld4"}},
                };

            NativeFileImportService nativeFileImportService = new NativeFileImportService();
            string options = JsonConvert.SerializeObject(new ImportSettings { ArtifactTypeId = 1111111, CaseArtifactId = 2222222 });
            TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

            //ACT
            ImportSettings result = rdoSynchronizer.GetSyncDataImportSettings(fieldMap, options, nativeFileImportService);

            //ASSERT
            Assert.AreEqual(kCura.IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD_NAME, result.FolderPathSourceFieldName);
            Assert.AreEqual(0, result.DestinationFolderArtifactId);
        }

        [Test]
        public void IncludeFieldInImport_FieldMapTypeIsNone_True()
        {
            //ARRANGE
            FieldMap fieldMap = new FieldMap()
            {
                DestinationField = new FieldEntry() { FieldIdentifier = "4000001" },
                FieldMapType = FieldMapTypeEnum.None,
                SourceField = new FieldEntry() { FieldIdentifier = "SourceFld1" }
            };
            TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

            //ACT
            bool result = rdoSynchronizer.IncludeFieldInImport(fieldMap);

            //ASSERT
            Assert.IsTrue(result);
        }

        [Test]
        public void IncludeFieldInImport_FieldMapTypeIsParent_False()
        {
            //ARRANGE
            FieldMap fieldMap = new FieldMap()
            {
                DestinationField = new FieldEntry() { FieldIdentifier = "4000001" },
                FieldMapType = FieldMapTypeEnum.Parent,
                SourceField = new FieldEntry() { FieldIdentifier = "SourceFld1" }
            };
            TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

            //ACT
            bool result = rdoSynchronizer.IncludeFieldInImport(fieldMap);

            //ASSERT
            Assert.IsFalse(result);
        }

        [Test]
        public void IncludeFieldInImport_FieldMapTypeIsNativeFilePath_False()
        {
            //ARRANGE
            FieldMap fieldMap = new FieldMap()
            {
                DestinationField = new FieldEntry() { FieldIdentifier = "4000001" },
                FieldMapType = FieldMapTypeEnum.NativeFilePath,
                SourceField = new FieldEntry() { FieldIdentifier = "SourceFld1" }
            };
            TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

            //ACT
            bool result = rdoSynchronizer.IncludeFieldInImport(fieldMap);

            //ASSERT
            Assert.IsFalse(result);
        }

        [Test]
        public void IncludeFieldInImport_FieldMapTypeIsIdentifier_True()
        {
            //ARRANGE
            FieldMap fieldMap = new FieldMap()
            {
                DestinationField = new FieldEntry() { FieldIdentifier = "4000001" },
                FieldMapType = FieldMapTypeEnum.Identifier,
                SourceField = new FieldEntry() { FieldIdentifier = "SourceFld1" }
            };
            TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

            //ACT
            bool result = rdoSynchronizer.IncludeFieldInImport(fieldMap);

            //ASSERT
            Assert.IsTrue(result);
        }

        [Test]
        public void IncludeFieldInImport_FieldMapTypeIsFolderPathInformationWhenThereIsADestination_True()
        {
            //ARRANGE
            FieldMap fieldMap = new FieldMap()
            {
                DestinationField = new FieldEntry() { FieldIdentifier = "4000001" },
                FieldMapType = FieldMapTypeEnum.FolderPathInformation,
                SourceField = new FieldEntry() { FieldIdentifier = "SourceFld1" }
            };
            TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

            //ACT
            bool result = rdoSynchronizer.IncludeFieldInImport(fieldMap);

            //ASSERT
            Assert.IsTrue(result);
        }

        [Test]
        public void IncludeFieldInImport_FieldMapTypeIsFolderPathInformationWhenThereIsNoDestinationSet_False()
        {
            //ARRANGE
            FieldMap fieldMap = new FieldMap()
            {
                DestinationField = null,
                FieldMapType = FieldMapTypeEnum.FolderPathInformation,
                SourceField = new FieldEntry() { FieldIdentifier = "SourceFld1" }
            };
            TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

            //ACT
            bool result = rdoSynchronizer.IncludeFieldInImport(fieldMap);

            //ASSERT
            Assert.IsFalse(result);
        }

        [Test]
        public void IncludeFieldInImport_FieldMapTypeIsFolderPathInformationWhenThereIsDestinationHasNullProperties_False()
        {
            //ARRANGE
            FieldMap fieldMap = new FieldMap()
            {
                DestinationField = new FieldEntry(),
                FieldMapType = FieldMapTypeEnum.FolderPathInformation,
                SourceField = new FieldEntry() { FieldIdentifier = "SourceFld1" }
            };
            TestRdoSynchronizer rdoSynchronizer = new TestRdoSynchronizer();

            //ACT
            bool result = rdoSynchronizer.IncludeFieldInImport(fieldMap);

            //ASSERT
            Assert.IsFalse(result);
        }

        [Test]
        public void GetEmailBodyData_HasWorkspace_CorrectlyFormattedOutput()
        {
            //ARRANGE
            int workspaceId = 1111111;
            WorkspaceRef workspaceRef = new WorkspaceRef() { Id = workspaceId, Name = "My Test workspace" };
            IEmailBodyData rdoSynchronizer = new MockSynchronizer(workspaceRef);
            var settings = new ImportSettings { CaseArtifactId = workspaceId };
            string options = JsonConvert.SerializeObject(settings);

            //ACT
            string returnedString = rdoSynchronizer.GetEmailBodyData(null, options);

            //ASSERT
            Assert.AreEqual("\r\nDestination Workspace: My Test workspace - 1111111", returnedString);
        }

        [Test]
        public void GetEmailBodyData_NoWorkspace_CorrectlyFormattedOutput()
        {
            //ARRANGE
            int workspaceId = 1111111;
            WorkspaceRef workspaceRef = null;
            IEmailBodyData rdoSynchronizer = new MockSynchronizer(workspaceRef);
            var settings = new ImportSettings { CaseArtifactId = workspaceId };
            string options = JsonConvert.SerializeObject(settings);

            //ACT
            string returnedString = rdoSynchronizer.GetEmailBodyData(null, options);

            //ASSERT
            Assert.AreEqual("", returnedString);
        }

        /// <summary>
        /// Test whether options are parsed correctly when getting the mappable fields
        /// </summary>
        [Test]
        public void GetFields_CorrectOptionsPassed()
        {
            // Arrange
            const int artifactTypeId = 123;
            const int caseArtifactId = 456;

            var relativityFieldQuery = new Mock<IRelativityFieldQuery>();
            var importApiFactory = new Mock<IImportApiFactory>();
            var importApi = new Mock<IImportAPI>();
            var jobFactory = new Mock<IImportJobFactory>();

            string options = string.Format("{{Provider:'relativity', WebServiceUrl:'WebServiceUrl', ArtifactTypeId:{0}, CaseArtifactId:{1}}}", artifactTypeId, caseArtifactId);

            relativityFieldQuery.Setup(x => x.GetFieldsForRdo(artifactTypeId)).Returns(new List<RelativityObject>());
            importApi.Setup(x => x.GetWorkspaceFields(caseArtifactId, artifactTypeId)).Returns(new List<kCura.Relativity.ImportAPI.Data.Field>());
            importApiFactory.Setup(x => x.GetImportAPI(It.IsAny<ImportSettings>())).Returns(importApi.Object);

            importApiFactory.Setup(x => x.GetImportApiFacade(It.IsAny<ImportSettings>()))
                .Returns(new ImportApiFacade(importApiFactory.Object, new ImportSettings(), new Mock<IAPILog>().Object));

            var sut = new RdoSynchronizer(relativityFieldQuery.Object, importApiFactory.Object, jobFactory.Object, _helper.Object, _diagnosticLogMock.Object);

            // Act
            IEnumerable<FieldEntry> results = sut.GetFields(new DataSourceProviderConfiguration(options));

            // Assert
            relativityFieldQuery.Verify(x => x.GetFieldsForRdo(artifactTypeId), Times.Once);
            importApiFactory.Verify(x => x.GetImportAPI(It.IsAny<ImportSettings>()), Times.Once);
            importApi.Verify(x => x.GetWorkspaceFields(caseArtifactId, artifactTypeId), Times.Once);
        }
    }

    public class TestRdoSynchronizer : RdoSynchronizer
    {
        public TestRdoSynchronizer()
          : base(null, null, Mock.Of<IImportJobFactory>(), RdoSynchronizerTests.MockHelper().Object, null)
        {
            WebAPIPath = kCura.IntegrationPoints.Domain.Constants.WEB_API_PATH;
            DisableNativeLocationValidation = false;
            DisableNativeValidation = false;
        }

        public new ImportSettings GetSyncDataImportSettings(IEnumerable<FieldMap> fieldMap, string options, NativeFileImportService nativeFileImportService)
        {
            return base.GetSyncDataImportSettings(fieldMap, options, nativeFileImportService);
        }

        public new bool IncludeFieldInImport(FieldMap fieldMap)
        {
            return base.IncludeFieldInImport(fieldMap);
        }
    }

    public class MockSynchronizer : RdoSynchronizer
    {
        private readonly WorkspaceRef _workspaceRef;

        public MockSynchronizer(WorkspaceRef workspaceRef)
          : base(null, null, Mock.Of<IImportJobFactory>(), RdoSynchronizerTests.MockHelper().Object, null)
        {
            WebAPIPath = "WebAPIPath";
            DisableNativeLocationValidation = false;
            DisableNativeValidation = false;
            _workspaceRef = workspaceRef;
        }

        protected override WorkspaceRef GetWorkspace(ImportSettings settings)
        {
            return _workspaceRef;
        }
    }
}
