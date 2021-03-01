﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	internal class NotificationConfigurationTests : ConfigurationTestBase
	{
		private static readonly string[] TestEmailRecipients = { "relativity.admin@kcura.com", string.Empty, "relativity-sync@kcura.onmicrosoft.com", "distro_list@relativity.com" };

		private Mock<ISyncServiceManager> _syncServiceManagerMock;

		[SetUp]
		public void Setup()
		{
			_syncServiceManagerMock = new Mock<ISyncServiceManager>();
			string emailRecipients = string.Join(";", TestEmailRecipients);
			_configurationRdo.EmailNotificationRecipients = emailRecipients;
		}

		[Test]
		public void SendEmailsReturnsTrueWhenEmailsExistTest()
		{
			// Arrange
			var syncJobParameters = new SyncJobParameters(int.MaxValue, int.MaxValue, int.MaxValue);
			var instance = new NotificationConfiguration(_configuration.Object, syncJobParameters, _syncServiceManagerMock.Object);

			// Act
			bool actualResult = instance.SendEmails;

			// Assert
			actualResult.Should().BeTrue();
			
			_configuration.Verify(x => x.GetFieldValue(It.IsAny<Func<SyncConfigurationRdo, string>>()), Times.Once);
		}

		[Test]
		public void SendEmailsReturnsFalseWhenNoEmailsExistTest()
		{
			// Arrange
			var syncJobParameters = new SyncJobParameters(int.MaxValue, int.MaxValue, int.MaxValue);
			var instance = new NotificationConfiguration(_configuration.Object, syncJobParameters, _syncServiceManagerMock.Object);
			_configurationRdo.EmailNotificationRecipients = null;
			

			// Act
			bool actualResult = instance.SendEmails;

			// Assert
			actualResult.Should().BeFalse();
			_configuration.Verify(x => x.GetFieldValue(It.IsAny<Func<SyncConfigurationRdo, string>>()), Times.Once);
		}

		[Test]
		public void EmailRecipientsPopulatedOnlyOnceFrom_configurationTest()
		{
			// Arrange
			var syncJobParameters = new SyncJobParameters(int.MaxValue, int.MaxValue, int.MaxValue);
			var instance = new NotificationConfiguration(_configuration.Object, syncJobParameters, _syncServiceManagerMock.Object);

			// Act
			const int numberOfCalls = 2;
			IEnumerable<string> actualEmailRecipients = Enumerable.Empty<string>();
			for (int i = 0; i < numberOfCalls; i++)
			{
				actualEmailRecipients = instance.GetEmailRecipients();
			}

			// Assert
			actualEmailRecipients.Should().NotBeNullOrEmpty();
			_configuration.Verify(x => x.GetFieldValue(It.IsAny<Func<SyncConfigurationRdo, string>>()), Times.Once);
		}

		[Test]
		public void EmailRecipientsParsingShouldRemoveEmptyEntriesTest()
		{
			// Arrange
			var syncJobParameters = new SyncJobParameters(int.MaxValue, int.MaxValue, int.MaxValue);
			var instance = new NotificationConfiguration(_configuration.Object, syncJobParameters, _syncServiceManagerMock.Object);

			// Act
			IEnumerable<string> actualEmailRecipients = instance.GetEmailRecipients();

			// Assert
			int expectedNumberOfRecipients = TestEmailRecipients.Length - 1;    // removing one empty entry
			actualEmailRecipients.Should().NotBeNullOrEmpty();
			actualEmailRecipients.Should().HaveCount(expectedNumberOfRecipients);
			_configuration.Verify(x => x.GetFieldValue(It.IsAny<Func<SyncConfigurationRdo, string>>()), Times.Once);
		}

		[Test]
		public void PropertiesIn_configurationShouldBeRetrievedTest()
		{
			// Arrange
			const int expectedDestinationWorkspaceArtifactId = 1064478;
			const int expectedJobHistoryArtifactId = 1043378;
			const string expectedJobName = "Test Job Name";
			const string expectedSourceWorkspaceTag = "This Instance - My Source Case";

			_configurationRdo.DestinationWorkspaceArtifactId = expectedDestinationWorkspaceArtifactId;
			_configurationRdo.JobHistoryId = expectedJobHistoryArtifactId;
			_configurationRdo.SourceWorkspaceTagName = expectedSourceWorkspaceTag;

			var objectManagerMock = new Mock<IObjectManager>();
			SetupJobName(objectManagerMock, expectedJobName);

			_syncServiceManagerMock.Setup(x => x.CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
				.Returns(objectManagerMock.Object);

			var syncJobParameters = new SyncJobParameters(int.MaxValue, int.MaxValue, int.MaxValue);
			var instance = new NotificationConfiguration(_configuration.Object, syncJobParameters, _syncServiceManagerMock.Object);

			// Act
			int actualDestinationWorkspaceArtifactId = instance.DestinationWorkspaceArtifactId;
			int actualJobHistoryArtifactId = instance.JobHistoryArtifactId;
			string actualJobName = instance.GetJobName();
			string actualSourceWorkspaceTag = instance.GetSourceWorkspaceTag();

			// Assert
			actualDestinationWorkspaceArtifactId.Should().Be(expectedDestinationWorkspaceArtifactId);
			actualJobHistoryArtifactId.Should().Be(expectedJobHistoryArtifactId);
			actualJobName.Should().Be(expectedJobName);
			actualSourceWorkspaceTag.Should().Be(expectedSourceWorkspaceTag);
			_configuration.Verify();
		}

		[Test]
		public void PropertiesInSyncJobParametersShouldBeRetrievedTest()
		{
			// Arrange
			var syncJobParameters = new SyncJobParameters(int.MaxValue, int.MaxValue, int.MaxValue);
			_syncServiceManagerMock = new Mock<ISyncServiceManager>();
			var instance = new NotificationConfiguration(_configuration.Object, syncJobParameters, _syncServiceManagerMock.Object);

			// Act
			int actualSourceWorkspaceArtifactId = instance.SourceWorkspaceArtifactId;
			int actualSyncConfigurationArtifactId = instance.SyncConfigurationArtifactId;

			// Assert
			actualSourceWorkspaceArtifactId.Should().Be(int.MaxValue);
			actualSyncConfigurationArtifactId.Should().Be(int.MaxValue);
		}
	}
}