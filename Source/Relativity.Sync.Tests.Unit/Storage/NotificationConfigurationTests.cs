using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	public class NotificationConfigurationTests
	{
		private static readonly string[] TestEmailRecipients = { "relativity.admin@kcura.com", string.Empty, "relativity-sync@kcura.onmicrosoft.com", "distro_list@relativity.com" };

		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		private static readonly Guid EmailNotificationRecipientsGuid = new Guid("4F03914D-9E86-4B72-B75C-EE48FEEBB583");
		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");
		private static readonly Guid SourceWorkspaceTagNameGuid = new Guid("D828B69E-AAAE-4639-91E2-416E35C163B1");

		[Test]
		public void SendEmailsReturnsTrueWhenEmailsExistTest()
		{
			// Arrange
			var cache = new Mock<Sync.Storage.IConfiguration>();

			string emailRecipients = string.Join(";", TestEmailRecipients);
			cache.Setup(x => x.GetFieldValue<string>(EmailNotificationRecipientsGuid)).Returns(emailRecipients);

			var syncJobParameters = new SyncJobParameters(int.MaxValue, int.MaxValue, int.MaxValue, string.Empty);
			var instance = new NotificationConfiguration(cache.Object, syncJobParameters);

			// Act
			bool actualResult = instance.SendEmails;

			// Assert
			actualResult.Should().BeTrue();
			cache.Verify(x => x.GetFieldValue<string>(EmailNotificationRecipientsGuid), Times.Once);
		}

		[Test]
		public void SendEmailsReturnsFalseWhenNoEmailsExistTest()
		{
			// Arrange
			var cache = new Mock<Sync.Storage.IConfiguration>();

			string emailRecipients = string.Join(";", string.Empty);
			cache.Setup(x => x.GetFieldValue<string>(EmailNotificationRecipientsGuid)).Returns(emailRecipients);

			var syncJobParameters = new SyncJobParameters(int.MaxValue, int.MaxValue, int.MaxValue, string.Empty);
			var instance = new NotificationConfiguration(cache.Object, syncJobParameters);

			// Act
			bool actualResult = instance.SendEmails;

			// Assert
			actualResult.Should().BeFalse();
			cache.Verify(x => x.GetFieldValue<string>(EmailNotificationRecipientsGuid), Times.Once);
		}

		[Test]
		public void EmailRecipientsPopulatedOnlyOnceFromCacheTest()
		{
			// Arrange
			var cache = new Mock<Sync.Storage.IConfiguration>();

			string emailRecipients = string.Join(";", TestEmailRecipients);
			cache.Setup(x => x.GetFieldValue<string>(EmailNotificationRecipientsGuid)).Returns(emailRecipients);

			var syncJobParameters = new SyncJobParameters(int.MaxValue, int.MaxValue, int.MaxValue, string.Empty);
			var instance = new NotificationConfiguration(cache.Object, syncJobParameters);

			// Act
			const int numberOfCalls = 2;
			IEnumerable<string> actualEmailRecipients = Enumerable.Empty<string>();
			for (int i = 0; i < numberOfCalls; i++)
			{
				actualEmailRecipients = instance.EmailRecipients;
			}

			// Assert
			actualEmailRecipients.Should().NotBeNullOrEmpty();
			cache.Verify(x => x.GetFieldValue<string>(EmailNotificationRecipientsGuid), Times.Once);
		}

		[Test]
		public void EmailRecipientsParsingShouldRemoveEmptyEntriesTest()
		{
			// Arrange
			var cache = new Mock<Sync.Storage.IConfiguration>();

			string emailRecipients = string.Join(";", TestEmailRecipients);
			cache.Setup(x => x.GetFieldValue<string>(EmailNotificationRecipientsGuid)).Returns(emailRecipients);

			var syncJobParameters = new SyncJobParameters(int.MaxValue, int.MaxValue, int.MaxValue, string.Empty);
			var instance = new NotificationConfiguration(cache.Object, syncJobParameters);

			// Act
			IEnumerable<string> actualEmailRecipients = instance.EmailRecipients;

			// Assert
			int expectedNumberOfRecipients = TestEmailRecipients.Length - 1;    // removing one empty entry
			actualEmailRecipients.Should().NotBeNullOrEmpty();
			actualEmailRecipients.Should().HaveCount(expectedNumberOfRecipients);
			cache.Verify(x => x.GetFieldValue<string>(EmailNotificationRecipientsGuid), Times.Once);
		}

		[Test]
		public void PropertiesInCacheShouldBeRetrievedTest()
		{
			// Arrange
			const int expectedDestinationWorkspaceArtifactId = 1064478;
			const int expectedJobHistoryArtifactId = 1043378;
			const string expectedJobName = "Test Job Name";
			const string expectedSourceWorkspaceTag = "This Instance - My Source Case";

			var cache = new Mock<Sync.Storage.IConfiguration>();

			cache.Setup(x => x.GetFieldValue<int>(DestinationWorkspaceArtifactIdGuid)).Returns(expectedDestinationWorkspaceArtifactId).Verifiable();
			cache.Setup(x => x.GetFieldValue<RelativityObjectValue>(JobHistoryGuid)).Returns(new RelativityObjectValue { ArtifactID = expectedJobHistoryArtifactId, Name = expectedJobName }).Verifiable();
			cache.Setup(x => x.GetFieldValue<string>(SourceWorkspaceTagNameGuid)).Returns(expectedSourceWorkspaceTag).Verifiable();

			var syncJobParameters = new SyncJobParameters(int.MaxValue, int.MaxValue, int.MaxValue, string.Empty);
			var instance = new NotificationConfiguration(cache.Object, syncJobParameters);

			// Act
			int actualDestinationWorkspaceArtifactId = instance.DestinationWorkspaceArtifactId;
			int actualJobHistoryArtifactId = instance.JobHistoryArtifactId;
			string actualJobName = instance.JobName;
			string actualSourceWorkspaceTag = instance.SourceWorkspaceTag;

			// Assert
			actualDestinationWorkspaceArtifactId.Should().Be(expectedDestinationWorkspaceArtifactId);
			actualJobHistoryArtifactId.Should().Be(expectedJobHistoryArtifactId);
			actualJobName.Should().Be(expectedJobName);
			actualSourceWorkspaceTag.Should().Be(expectedSourceWorkspaceTag);
			cache.Verify();
		}

		[Test]
		public void PropertiesInSyncJobParametersShouldBeRetrievedTest()
		{
			// Arrange
			var cache = new Mock<Sync.Storage.IConfiguration>();
			var syncJobParameters = new SyncJobParameters(int.MaxValue, int.MaxValue, int.MaxValue, string.Empty);
			var instance = new NotificationConfiguration(cache.Object, syncJobParameters);

			// Act
			int actualSourceWorkspaceArtifactId = instance.SourceWorkspaceArtifactId;
			int actualSyncConfigurationArtifactId = instance.SyncConfigurationArtifactId;

			// Assert
			actualSourceWorkspaceArtifactId.Should().Be(int.MaxValue);
			actualSyncConfigurationArtifactId.Should().Be(int.MaxValue);
		}
	}
}