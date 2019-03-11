﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Services.InstanceSetting;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class EnvironmentPropertyProviderTests
	{
		private Mock<IInstanceSettingManager> _instanceSettingManagerMock;
		private Mock<ISourceServiceFactoryForAdmin> _sourceServiceFactoryMock;
		private Mock<ISyncLog> _logger;

		[SetUp]
		public void SetUp()
		{
			_instanceSettingManagerMock = new Mock<IInstanceSettingManager>();
			_sourceServiceFactoryMock = new Mock<ISourceServiceFactoryForAdmin>();
			_logger = new Mock<ISyncLog>();

			_sourceServiceFactoryMock.Setup(x => x.CreateProxyAsync<IInstanceSettingManager>())
				.Returns(Task.FromResult(_instanceSettingManagerMock.Object));
		}

		[Test]
		public void ItShouldSuccessfullyCreateNewInstance()
		{
			var resultSet = new InstanceSettingQueryResultSet
			{
				Success = true,
				Results = new List<Services.Result<Services.InstanceSetting.InstanceSetting>>
				{
					new Services.Result<Services.InstanceSetting.InstanceSetting>
					{
						Artifact = new Services.InstanceSetting.InstanceSetting
						{
							Value = "FooBar"
						}
					}
				}
			};
			_instanceSettingManagerMock.Setup(x => x.QueryAsync(It.IsAny<Services.Query>()))
				.Returns(Task.FromResult(resultSet));

			// Act
			IEnvironmentPropertyProvider environmentPropertyProvider = EnvironmentPropertyProvider.Create(_sourceServiceFactoryMock.Object, _logger.Object);

			// Assert
			Assert.AreEqual("FooBar", environmentPropertyProvider.InstanceName);
		}

		[Test]
		public void ItShouldUseDefaultInstanceNameIfNoResults()
		{
			var resultSet = new InstanceSettingQueryResultSet
			{
				Success = true,
				Results = new List<Services.Result<Services.InstanceSetting.InstanceSetting>>()
			};
			_instanceSettingManagerMock.Setup(x => x.QueryAsync(It.IsAny<Services.Query>()))
				.Returns(Task.FromResult(resultSet));

			// Act
			IEnvironmentPropertyProvider environmentPropertyProvider = EnvironmentPropertyProvider.Create(_sourceServiceFactoryMock.Object, _logger.Object);

			// Assert
			Assert.AreEqual("unknown", environmentPropertyProvider.InstanceName);
		}

		[Test]
		public void ItShouldUseDefaultInstanceNameIfQueryErrored()
		{
			var resultSet = new InstanceSettingQueryResultSet
			{
				Success = false
			};
			_instanceSettingManagerMock.Setup(x => x.QueryAsync(It.IsAny<Services.Query>()))
				.Returns(Task.FromResult(resultSet));

			// Act
			IEnvironmentPropertyProvider environmentPropertyProvider = EnvironmentPropertyProvider.Create(_sourceServiceFactoryMock.Object, _logger.Object);

			// Assert
			Assert.AreEqual("unknown", environmentPropertyProvider.InstanceName);
		}

		[Test]
		public void ItShouldUseDefaultInstanceNameIfQueryThrows()
		{
			_instanceSettingManagerMock.Setup(x => x.QueryAsync(It.IsAny<Services.Query>()))
				.Throws(new Exception("blech"));

			// Act
			IEnvironmentPropertyProvider environmentPropertyProvider = EnvironmentPropertyProvider.Create(_sourceServiceFactoryMock.Object, _logger.Object);

			// Assert
			Assert.AreEqual("unknown", environmentPropertyProvider.InstanceName);
		}
	}
}
