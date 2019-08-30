using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services;
using Relativity.Services.InstanceSetting;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class WebApiPathQueryTests
	{
		private Mock<ISourceServiceFactoryForAdmin> _serviceFactory;
		private Mock<IInstanceSettingManager> _instanceSettingManager;
		private WebApiPathQuery _instance;

		private const string _WEB_API_PATH_SETTING_SECTION = "kCura.IntegrationPoints";
		private const string _WEB_API_PATH_SETTING_NAME = "WebAPIPath";

		[SetUp]
		public void SetUp()
		{
			_serviceFactory = new Mock<ISourceServiceFactoryForAdmin>();
			_instanceSettingManager = new Mock<IInstanceSettingManager>();
			_serviceFactory.Setup(x => x.CreateProxyAsync<IInstanceSettingManager>()).ReturnsAsync(_instanceSettingManager.Object);
			_instance = new WebApiPathQuery(_serviceFactory.Object);
		}

		[Test]
		public async Task ItShouldSuccessfullyReturnWebApiPath()
		{
			const string expectedValue = "My Value";
			InstanceSettingQueryResultSet resultSet = new InstanceSettingQueryResultSet()
			{
				Success = true,
				Results = new List<Result<Services.InstanceSetting.InstanceSetting>>()
				{
					new Result<Services.InstanceSetting.InstanceSetting>()
					{
						Success = true,
						Artifact = new Services.InstanceSetting.InstanceSetting()
						{
							Value = expectedValue
						}
					}
				}
			};
			_instanceSettingManager.Setup(x => x.QueryAsync(It.Is<Services.Query>(q =>
				q.Condition == $"'Name' == '{_WEB_API_PATH_SETTING_NAME}' AND 'Section' == '{_WEB_API_PATH_SETTING_SECTION}'"))).ReturnsAsync(resultSet);

			// act
			string actualValue = await _instance.GetWebApiPathAsync().ConfigureAwait(false);

			// assert
			actualValue.Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldThrowExceptionWhenInstanceSettingNotFound()
		{
			InstanceSettingQueryResultSet resultSet = new InstanceSettingQueryResultSet()
			{
				Success = true,
				Results = new List<Result<Services.InstanceSetting.InstanceSetting>>()
			};
			_instanceSettingManager.Setup(x => x.QueryAsync(It.Is<Services.Query>(q =>
				q.Condition == $"'Name' == '{_WEB_API_PATH_SETTING_NAME}' AND 'Section' == '{_WEB_API_PATH_SETTING_SECTION}'"))).ReturnsAsync(resultSet);

			// act
			Func<Task> action = async () => await _instance.GetWebApiPathAsync().ConfigureAwait(false);

			// assert
			action.Should().Throw<SyncException>().Which.Message
				.Equals($"Query for '{_WEB_API_PATH_SETTING_NAME}' instance setting from section '{_WEB_API_PATH_SETTING_SECTION}' returned empty results. Make sure instance setting exists.",
					StringComparison.InvariantCulture);
		}

		[Test]
		public void ItShouldThrowExceptionWhenQueryReturnNoSuccess()
		{
			const string errorMessage = "Catastrophic failure.";
			InstanceSettingQueryResultSet resultSet = new InstanceSettingQueryResultSet()
			{
				Success = false,
				Message = errorMessage
			};
			_instanceSettingManager.Setup(x => x.QueryAsync(It.Is<Services.Query>(q =>
				q.Condition == $"'Name' == '{_WEB_API_PATH_SETTING_NAME}' AND 'Section' == '{_WEB_API_PATH_SETTING_SECTION}'"))).ReturnsAsync(resultSet);

			// act
			Func<Task> action = async () => await _instance.GetWebApiPathAsync().ConfigureAwait(false);

			// assert
			action.Should().Throw<SyncException>().Which.Message
				.Equals($"Failed to query for '{_WEB_API_PATH_SETTING_NAME}' instance setting. Response message: {resultSet.Message}",
					StringComparison.InvariantCulture);
		}

		[Test]
		public void ItShouldRethrowWhenQueryFails()
		{
			_instanceSettingManager.Setup(x => x.QueryAsync(It.IsAny<Services.Query>())).Throws<InvalidOperationException>();

			// act
			Func<Task> action = async () => await _instance.GetWebApiPathAsync().ConfigureAwait(false);

			// assert
			action.Should().Throw<InvalidOperationException>();
		}
	}
}