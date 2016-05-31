using System.Collections.Generic;
using System.Linq;
using System.Net;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using ViewFieldInfo = kCura.WinEDDS.ViewFieldInfo;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
    public class ExportProcessBuilderTests
    {
        #region Fields

        private ICaseManagerFactory _caseManagerFactory;
        private ICredentialProvider _credentialProvider;
        private IExporterFactory _exporterFactory;

        private ExportFile _exportFile;
        private IExportFileHelper _exportFileHelper;

        private ExportProcessBuilder _exportProcessBuilder;
        private ILoggingMediator _loggingMediator;
        private ISearchManagerFactory _searchManagerFactory;
        private IUserMessageNotification _userMessageNotification;
        private IUserNotification _userNotification;

        #endregion

        #region SetUp

        [SetUp]
        public void SetUp()
        {
            _caseManagerFactory = Substitute.For<ICaseManagerFactory>();
            _credentialProvider = Substitute.For<ICredentialProvider>();
            _exporterFactory = Substitute.For<IExporterFactory>();
            _exportFileHelper = Substitute.For<IExportFileHelper>();
            _loggingMediator = Substitute.For<ILoggingMediator>();
            _searchManagerFactory = Substitute.For<ISearchManagerFactory>();
            _userMessageNotification = Substitute.For<IUserMessageNotification>();
            _userNotification = Substitute.For<IUserNotification>();

            MockExportFile();

            _exportProcessBuilder = new ExportProcessBuilder(_loggingMediator, _userMessageNotification, _userNotification, _credentialProvider, _caseManagerFactory,
                _searchManagerFactory, _exporterFactory, _exportFileHelper);
        }

        private void MockExportFile()
        {
            _exportFile = new ExportFile(1)
            {
                CaseInfo = new CaseInfo
                {
                    DocumentPath = "document_path",
                    ArtifactID = 2
                }
            };
            _exportFileHelper.CreateDefaultSetup(new ExportSettings()).ReturnsForAnyArgs(_exportFile);
        }

        #endregion

        #region Tests

        [Test]
        public void ItShouldPerformLogin()
        {
            var credential = new NetworkCredential();
            _credentialProvider.Authenticate(new CookieContainer()).ReturnsForAnyArgs(credential);

            _exportProcessBuilder.Create(new ExportSettings());

            Assert.IsNotNull(_exportFile.CookieContainer);
            Assert.AreEqual(credential, _exportFile.Credential);
        }

        [Test]
        public void ItShouldCreateAndDisposeSearchManager()
        {
            var searchManager = Substitute.For<ISearchManager>();
            _searchManagerFactory.Create(null, null).ReturnsForAnyArgs(searchManager);

            _exportProcessBuilder.Create(new ExportSettings());

            _searchManagerFactory.ReceivedWithAnyArgs().Create(null, null);
            searchManager.Received().Dispose();
        }

        [Test]
        public void ItShouldCreateAndDisposeCaseManager()
        {
            var caseManager = Substitute.For<ICaseManager>();
            _caseManagerFactory.Create(null, null).ReturnsForAnyArgs(caseManager);

            _exportProcessBuilder.Create(new ExportSettings());

            _caseManagerFactory.ReceivedWithAnyArgs().Create(null, null);
            caseManager.Received().Dispose();
        }

        [Test]
        public void ItShouldPopulateCaseInfoForEmptyDocumentPath()
        {
            _exportFile.CaseInfo.DocumentPath = string.Empty;
            var expectedCaseInfoArtifactId = _exportFile.CaseInfo.ArtifactID;

            var caseManager = Substitute.For<ICaseManager>();
            caseManager.Read(1).ReturnsForAnyArgs(new CaseInfo());
            _caseManagerFactory.Create(null, null).ReturnsForAnyArgs(caseManager);

            _exportProcessBuilder.Create(new ExportSettings());

            caseManager.Received().Read(expectedCaseInfoArtifactId);
        }

        [Test]
        public void ItShouldNotPopulateCaseInfoForNotEmptyDocumentPath()
        {
            _exportFile.CaseInfo.DocumentPath = "document_path";

            var caseManager = Substitute.For<ICaseManager>();
            _caseManagerFactory.Create(null, null).ReturnsForAnyArgs(caseManager);

            _exportProcessBuilder.Create(new ExportSettings());

            caseManager.DidNotReceiveWithAnyArgs().Read(1);
        }

        [Test]
        public void ItShouldAssignAllExportableFields()
        {
            var expectedExportableFields = new ViewFieldInfo[0];

            MockSearchManagerReturnValue(expectedExportableFields);

            _exportProcessBuilder.Create(new ExportSettings());

            Assert.AreSame(expectedExportableFields, _exportFile.AllExportableFields);
        }

        [Test]
        public void ItShouldFilterSelectedViewFields()
        {
            var expectedFilteredFields = new List<int>
            {
                1,
                2,
                3
            };
            var notExpectedFilteredFields = new List<int>
            {
                4,
                5,
                6
            };
            var settings = new ExportSettings
            {
                SelViewFieldIds = expectedFilteredFields
            };
            var expected = ViewFieldInfoMockFactory.CreateMockedViewFieldInfoArray(expectedFilteredFields.Concat(notExpectedFilteredFields).ToList());

            MockSearchManagerReturnValue(expected);

            _exportProcessBuilder.Create(settings);

            CollectionAssert.AreEquivalent(expectedFilteredFields, _exportFile.SelectedViewFields.Select(x => x.FieldArtifactId));
        }

        [Test]
        public void ItShouldCreateExporterUsingFactory()
        {
            _exportProcessBuilder.Create(new ExportSettings());

            _exporterFactory.Received().Create(_exportFile);
        }

        [Test]
        public void ItShouldAttachEventHandlers()
        {
            var exporter = Substitute.For<IExporter>();
            _exporterFactory.Create(_exportFile).Returns(exporter);

            _exportProcessBuilder.Create(new ExportSettings());

            _loggingMediator.Received().RegisterEventHandlers(_userMessageNotification, exporter);
            exporter.Received().InteractionManager = _userNotification;
        }

        private void MockSearchManagerReturnValue(ViewFieldInfo[] expectedExportableFields)
        {
            var searchManager = Substitute.For<ISearchManager>();
            searchManager.RetrieveAllExportableViewFields(_exportFile.CaseInfo.ArtifactID, _exportFile.ArtifactTypeID).Returns(expectedExportableFields);
            _searchManagerFactory.Create(null, null).ReturnsForAnyArgs(searchManager);
        }

        #endregion
    }
}