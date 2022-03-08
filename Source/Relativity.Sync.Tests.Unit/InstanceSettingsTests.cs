using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services;
using Relativity.Services.InstanceSetting;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class InstanceSettingsTests
	{
		private Mock<ISourceServiceFactoryForAdmin> _serviceFactoryForAdmin;
		private Mock<IInstanceSettingManager> _instanceSettingManager;
		private InstanceSettings _sut;

		private const string _WEB_API_PATH_SETTING_SECTION = "kCura.IntegrationPoints";
		private const string _WEB_API_PATH_SETTING_NAME = "WebAPIPath";
		private const string _RELATIVITY_CORE_SETTING_SECTION = "Relativity.Core";
		private const string _RESTRICT_REF_FILE_LINKS_ON_IMPORT_NAME = "RestrictReferentialFileLinksOnImport";

		private const string _DEFAULT_WEB_API_PATH = "test";
		private const bool _DEFAULT_RESTRICT_REF_FILE_LINKS_ON_IMPORT = false;

		[SetUp]
		public void SetUp()
		{
			_serviceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();
			_instanceSettingManager = new Mock<IInstanceSettingManager>();
			_serviceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IInstanceSettingManager>()).ReturnsAsync(_instanceSettingManager.Object);
			_sut = new InstanceSettings(_serviceFactoryForAdmin.Object, new EmptyLogger());
		}

		[Test]
		public async Task GetWebApiPathAsync_ShouldSuccessfullyReturnValue()
		{
			//arrange
			const string expectedValue = "My Value";
			SetupValidInstanceSettingResult(
				_WEB_API_PATH_SETTING_SECTION,
				_WEB_API_PATH_SETTING_NAME,
				expectedValue);

			//act
			string actualValue = await _sut.GetWebApiPathAsync().ConfigureAwait(false);

			//assert
			actualValue.Should().Be(expectedValue);
		}

		[Test]
		public async Task GetWebApiPathAsync_ShouldReturnExpectedDefault_WhenInstanceSettingNotFound()
		{
			//arrange
			var resultSet = new InstanceSettingQueryResultSet
			{
				Success = true,
				TotalCount = 0,
				Results = new List<Result<Services.InstanceSetting.InstanceSetting>>()
			};
			SetupInstanceSettingResult(
				_WEB_API_PATH_SETTING_SECTION,
				_WEB_API_PATH_SETTING_NAME,
				resultSet);

			//act
			string actualValue = await _sut.GetWebApiPathAsync(_DEFAULT_WEB_API_PATH).ConfigureAwait(false);

			//assert
			actualValue.Should().Be(_DEFAULT_WEB_API_PATH);
		}

		[Test]
		public async Task GetWebApiPathAsync_ShouldReturnExpectedDefault_WhenQueryReturnNoSuccess()
		{
			//arrange
			var resultSet = new InstanceSettingQueryResultSet
			{
				Success = false,
				TotalCount = 0,
				Message = "Catastrophic failure."
			};
			SetupInstanceSettingResult(
				_WEB_API_PATH_SETTING_SECTION,
				_WEB_API_PATH_SETTING_NAME,
				resultSet);

			//act
			string actualValue = await _sut.GetWebApiPathAsync(_DEFAULT_WEB_API_PATH).ConfigureAwait(false);

			//assert
			actualValue.Should().Be(_DEFAULT_WEB_API_PATH);
		}

		[Test]
		public async Task GetWebApiPathAsync_ShouldReturnExpectedDefault_WhenResultCanNotBeConvertedToExpectedType()
		{
			//arrange
			SetupValidInstanceSettingResult(
				_RELATIVITY_CORE_SETTING_SECTION,
				_RESTRICT_REF_FILE_LINKS_ON_IMPORT_NAME,
				"Test");

			//act
			bool actualValue = await _sut.GetRestrictReferentialFileLinksOnImportAsync(_DEFAULT_RESTRICT_REF_FILE_LINKS_ON_IMPORT).ConfigureAwait(false);

			//assert
			actualValue.Should().Be(_DEFAULT_RESTRICT_REF_FILE_LINKS_ON_IMPORT);
		}

		[Test]
		public async Task GetWebApiPathAsync_ShouldReturnExpectedDefault_WhenQueryFails()
		{
			//arrange
			_instanceSettingManager.Setup(x => x.QueryAsync(It.IsAny<Services.Query>())).Throws<InvalidOperationException>();

			//act
			string actualValue = await _sut.GetWebApiPathAsync(_DEFAULT_WEB_API_PATH).ConfigureAwait(false);

			//assert
			actualValue.Should().Be(_DEFAULT_WEB_API_PATH);
		}

		[Test]
		public async Task GetRestrictReferentialFileLinksOnImportAsync_ShouldSuccessfullyReturnValue()
		{
			//arrange
			SetupValidInstanceSettingResult(
				_RELATIVITY_CORE_SETTING_SECTION,
				_RESTRICT_REF_FILE_LINKS_ON_IMPORT_NAME,
				"True");

			//act
			bool actualValue = await _sut.GetRestrictReferentialFileLinksOnImportAsync().ConfigureAwait(false);

			//assert
			actualValue.Should().Be(true);
		}

		[Test]
		public async Task GetRestrictReferentialFileLinksOnImportAsync_ShouldReturnExpectedDefault_WhenInstanceSettingNotFound()
		{
			//arrange
			var resultSet = new InstanceSettingQueryResultSet
			{
				Success = true,
				TotalCount = 0,
				Results = new List<Result<Services.InstanceSetting.InstanceSetting>>()
			};
			SetupInstanceSettingResult(
				_RELATIVITY_CORE_SETTING_SECTION,
				_RESTRICT_REF_FILE_LINKS_ON_IMPORT_NAME,
				resultSet);

			//act
			bool actualValue = await _sut.GetRestrictReferentialFileLinksOnImportAsync(_DEFAULT_RESTRICT_REF_FILE_LINKS_ON_IMPORT).ConfigureAwait(false);

			//assert
			actualValue.Should().Be(_DEFAULT_RESTRICT_REF_FILE_LINKS_ON_IMPORT);
		}

		[Test]
		public async Task GetRestrictReferentialFileLinksOnImportAsync_ShouldReturnExpectedDefault_WhenQueryReturnNoSuccess()
		{
			//arrange
			var resultSet = new InstanceSettingQueryResultSet
			{
				Success = false,
				TotalCount = 0,
				Message = "Catastrophic failure."
			};
			SetupInstanceSettingResult(
				_RELATIVITY_CORE_SETTING_SECTION,
				_RESTRICT_REF_FILE_LINKS_ON_IMPORT_NAME,
				resultSet);

			//act
			bool actualValue = await _sut.GetRestrictReferentialFileLinksOnImportAsync(_DEFAULT_RESTRICT_REF_FILE_LINKS_ON_IMPORT).ConfigureAwait(false);

			//assert
			actualValue.Should().Be(_DEFAULT_RESTRICT_REF_FILE_LINKS_ON_IMPORT);
		}

		[Test]
		public async Task GetRestrictReferentialFileLinksOnImportAsync_ShouldReturnExpectedDefault_WhenResultCanNotBeConvertedToExpectedType()
		{
			//arrange
			SetupValidInstanceSettingResult(
				_RELATIVITY_CORE_SETTING_SECTION,
				_RESTRICT_REF_FILE_LINKS_ON_IMPORT_NAME,
				"Test");

			//act
			bool actualValue = await _sut.GetRestrictReferentialFileLinksOnImportAsync(_DEFAULT_RESTRICT_REF_FILE_LINKS_ON_IMPORT).ConfigureAwait(false);

			//assert
			actualValue.Should().Be(_DEFAULT_RESTRICT_REF_FILE_LINKS_ON_IMPORT);
		}

		[Test]
		public async Task GetRestrictReferentialFileLinksOnImportAsync_ShouldReturnExpectedDefault_WhenQueryFails()
		{
			//arrange
			_instanceSettingManager.Setup(x => x.QueryAsync(It.IsAny<Services.Query>())).Throws<InvalidOperationException>();

			//act
			bool actualValue = await _sut.GetRestrictReferentialFileLinksOnImportAsync(_DEFAULT_RESTRICT_REF_FILE_LINKS_ON_IMPORT).ConfigureAwait(false);

			//assert
			actualValue.Should().Be(_DEFAULT_RESTRICT_REF_FILE_LINKS_ON_IMPORT);
		}

		private void SetupValidInstanceSettingResult(
			string section,
			string name,
			string value)
		{
			var resultSet = new InstanceSettingQueryResultSet
			{
				Success = true,
				TotalCount = 1,
				Results = new List<Result<Services.InstanceSetting.InstanceSetting>>
				{
					new Result<Services.InstanceSetting.InstanceSetting>
					{
						Success = true,
						Artifact = new Services.InstanceSetting.InstanceSetting
						{
							Value = value
						}
					}
				}
			};

			SetupInstanceSettingResult(section, name, resultSet);
		}

		private void SetupInstanceSettingResult(
			string section,
			string name,
			InstanceSettingQueryResultSet resultSet)
		{
			_instanceSettingManager.Setup(x => x.QueryAsync(It.Is<Services.Query>(q =>
				q.Condition == $"'Name' == '{name}' AND 'Section' == '{section}'"))).ReturnsAsync(resultSet);
		}
	}
}