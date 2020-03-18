using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.Sync.Dashboards;
using Relativity.Sync.Dashboards.Api;
using Relativity.Sync.Dashboards.Configuration;

namespace UnitTests
{
	[TestFixture]
	public class SplunkKVStoreUpdaterTests
	{
		private AppSettings _appSettingsFake;
		private Mock<IJiraApi> _jiraApiMock;
		private Mock<ISplunkApi> _splunkApiMock;
		private List<SyncIssueDTO> _syncIssuesFake;
		private Mock<ILogger> _loggerFake;
		private SplunkKVStoreUpdater _sut;

		[SetUp]
		public void SetUp()
		{
			_appSettingsFake = new AppSettings()
			{
				SplunkKVCollectionName = "OurKVCollection"
			};
			_jiraApiMock = new Mock<IJiraApi>();
			_splunkApiMock = new Mock<ISplunkApi>();
			_syncIssuesFake = new List<SyncIssueDTO>()
			{
				new SyncIssueDTO()
				{
					Jira = "REL-1234",
					SearchMatch = "*UglyException*"
				},
				new SyncIssueDTO()
				{
					Jira = "REL-5678",
					SearchMatch = "*TerribleException*"
				}
			};
			_loggerFake = new Mock<ILogger>();
			_sut = new SplunkKVStoreUpdater(_appSettingsFake, _jiraApiMock.Object, _splunkApiMock.Object, _syncIssuesFake, _loggerFake.Object);
		}

		[Test]
		public async Task UpdateSplunkKVStoreAsync_ShouldUpdateCollection()
		{
			// Arrange
			Fields fields = new Fields()
			{
				Summary = "summary",
				Labels = new[] {"label1", "label2"},
				IssueType = new IssueType()
				{
					Name = "Incident"
				},
				Status = new Status()
				{
					Name = "In progress"
				},
				FixVersions = new[]
				{
					new FixVersion()
					{
						Name = "Release Name",
						ReleaseDate = "2020-01-01"
					}
				}
			};

			_jiraApiMock.Setup(x => x.GetIssueAsync(It.IsAny<string>())).ReturnsAsync(new JiraTicketInfo()
			{
				Fields = fields
			});

			// Act
			await _sut.UpdateSplunkKVStoreAsync().ConfigureAwait(false);

			// Assert
			_splunkApiMock.Verify(x => x.ClearKVStoreCollectionAsync(It.Is<string>(name => name == _appSettingsFake.SplunkKVCollectionName)));

			foreach (SyncIssueDTO dto in _syncIssuesFake)
			{
				_jiraApiMock.Verify(x => x.GetIssueAsync(
						It.Is<string>(jira => jira == dto.Jira)),
					Times.Once);

				_splunkApiMock.Verify(x => x.AddToKVStoreCollectionAsync(
						It.Is<string>(name => name == _appSettingsFake.SplunkKVCollectionName),
						It.Is<SplunkKVCollectionItem>(item => VerifySplunkKVCollectionItem(dto.Jira, dto.SearchMatch, fields, item))),
					Times.Once);
			}
		}

		private bool VerifySplunkKVCollectionItem(string expectedJira, string expectedSearchMatch, Fields expectedFields, SplunkKVCollectionItem item)
		{
			CollectionAssert.AreEquivalent(item.Labels, expectedFields.Labels);
			CollectionAssert.AreEquivalent(item.FixVersions, expectedFields.FixVersions);

			return
				item.Jira == expectedJira &&
				item.SearchMatch == expectedSearchMatch &&
				item.IssueType == expectedFields.IssueType.Name &&
				item.Summary == expectedFields.Summary &&
				item.Status == expectedFields.Status.Name;
		}
	}
}