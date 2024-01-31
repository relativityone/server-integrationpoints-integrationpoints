﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
    internal class ValidationConfigurationTests : ConfigurationTestBase
    {
        private ValidationConfiguration _sut;
        private Mock<IFieldMappings> _fieldMappings;
        private Mock<ISourceServiceFactoryForUser> _serviceFactoryForUser;
        private const int _WORKSPACE_ID = 111;
        private const int _USER_ID = 323454;

        [SetUp]
        public void SetUp()
        {
            _fieldMappings = new Mock<IFieldMappings>();
            SyncJobParameters jobParameters = new SyncJobParameters(It.IsAny<int>(), _WORKSPACE_ID, _USER_ID, It.IsAny<Guid>(), Guid.Empty);
            _serviceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
            _sut = new ValidationConfiguration(_configuration, _fieldMappings.Object, jobParameters, _serviceFactoryForUser.Object);
        }

        [Test]
        public void ItShouldRetrieveSourceWorkspaceArtifactId()
        {
            _sut.SourceWorkspaceArtifactId.Should().Be(_WORKSPACE_ID);
        }

        [Test]
        public void ItShouldRetrieveDestinationWorkspaceArtifactId()
        {
            const int expected = 1;
            _configurationRdo.DestinationWorkspaceArtifactId = expected;

            _sut.DestinationWorkspaceArtifactId.Should().Be(expected);
        }

        [Test]
        public void ItShouldRetrieveDataDestinationArtifactId()
        {
            const int expected = 1;
            _configurationRdo.DataDestinationArtifactId = expected;

            _sut.DestinationFolderArtifactId.Should().Be(expected);
        }

        [Test]
        public void ItShouldRetrieveDataSourceArtifactId()
        {
            const int expected = 1;
            _configurationRdo.DataSourceArtifactId = expected;

            _sut.SavedSearchArtifactId.Should().Be(expected);
        }

        [Test]
        public void ItShouldRetrieveEmailNotificationRecipients()
        {
            const string expected = "email1@example.com;email2@example.com";
            _configurationRdo.EmailNotificationRecipients = expected;

            _sut.GetNotificationEmails().Should().Be(expected);
        }

        [Test]
        public void ItShouldRetrieveFieldMappings()
        {
            List<FieldMap> fieldMappings = new List<FieldMap>();
            _fieldMappings.Setup(x => x.GetFieldMappings()).Returns(fieldMappings);

            _sut.GetFieldMappings().Should().BeSameAs(fieldMappings);
        }

        [Test]
        public void ItShouldRetrieveFieldOverlayBehavior()
        {
            const FieldOverlayBehavior expected = FieldOverlayBehavior.UseFieldSettings;
            _configurationRdo.FieldOverlayBehavior = expected;

            _sut.FieldOverlayBehavior.Should().Be(expected);
        }

        [Test]
        public void ItShouldRetrieveFolderPathSourceFieldArtifactId()
        {
            const string expected = "name";
            _configurationRdo.FolderPathSourceFieldName = expected;

            _sut.GetFolderPathSourceFieldName().Should().Be(expected);
        }

        [Test]
        public void ItShouldRetrieveImportOverwriteMode()
        {
            ImportOverwriteMode expected = ImportOverwriteMode.AppendOverlay;
            _configurationRdo.ImportOverwriteMode = expected;

            _sut.ImportOverwriteMode.Should().Be(expected);
        }

        [Test]
        public void ItShouldRetrieveDestinationFolderStructureBehavior()
        {
            DestinationFolderStructureBehavior expected = DestinationFolderStructureBehavior.ReadFromField;
            _configurationRdo.DestinationFolderStructureBehavior = expected;

            _sut.DestinationFolderStructureBehavior.Should().Be(expected);
        }

        [Test]
        public void ItShouldRetrieveJobHistoryName()
        {
            const string jobName = "Job name";

            var objectManagerMock = new Mock<IObjectManager>();
            _serviceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>())
                .Returns(Task.FromResult(objectManagerMock.Object));

            SetupJobName(objectManagerMock, jobName);

            _sut.GetJobName().Should().Be(jobName);
        }
    }
}