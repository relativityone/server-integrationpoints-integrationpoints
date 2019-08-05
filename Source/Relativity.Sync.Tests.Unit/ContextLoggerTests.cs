﻿using System;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class ContextLoggerTests
	{
		private Mock<ISyncLog> _logger;
		
		private ContextLogger _instance;
		private ImportSettingsDto _importSettingsDto;
		private object[] _expectedParams;

		private SyncJobParameters _syncJobParameters;
		private const int _JOB_ID = 234;
		private const int _WORKSPACE_ID = 123;
		private const int _PARAM1 = 1;

		private const string _MESSAGE = "message template {param1}";
		private readonly Exception _exception = new IOException();

		private readonly object[] _params = { _PARAM1 };


		[SetUp]
		public void SetUp()
		{
			_logger = new Mock<ISyncLog>();

			_importSettingsDto = new ImportSettingsDto();
			_syncJobParameters = new SyncJobParameters(_JOB_ID, _WORKSPACE_ID, _importSettingsDto);

			_expectedParams = new object[] {_PARAM1, _syncJobParameters};
			_instance = new ContextLogger(_syncJobParameters, _logger.Object);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForVerboseLevel()
		{
			// ACT
			_instance.LogVerbose(_MESSAGE, _params);

			// ASSERT
			_logger.Verify(x => x.LogVerbose(_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForVerboseLevelWithException()
		{
			// ACT
			_instance.LogVerbose(_exception, _MESSAGE, _params);

			// ASSERT
			_logger.Verify(x => x.LogVerbose(_exception, _MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForDebugLevel()
		{
			// ACT
			_instance.LogDebug(_MESSAGE, _params);

			// ASSERT
			_logger.Verify(x => x.LogDebug(_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForDebugLevelWithException()
		{
			// ACT
			_instance.LogDebug(_exception, _MESSAGE, _params);

			// ASSERT
			_logger.Verify(x => x.LogDebug(_exception, _MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForInformationLevel()
		{
			// ACT
			_instance.LogInformation(_MESSAGE, _params);

			// ASSERT
			_logger.Verify(x => x.LogInformation(_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForInformationLevelWithException()
		{
			// ACT
			_instance.LogInformation(_exception, _MESSAGE, _params);

			// ASSERT
			_logger.Verify(x => x.LogInformation(_exception, _MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForWarningLevel()
		{
			// ACT
			_instance.LogWarning(_MESSAGE, _params);

			// ASSERT
			_logger.Verify(x => x.LogWarning(_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForWarningLevelWithException()
		{
			// ACT
			_instance.LogWarning(_exception, _MESSAGE, _params);

			// ASSERT
			_logger.Verify(x => x.LogWarning(_exception, _MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForErrorLevel()
		{
			// ACT
			_instance.LogError(_MESSAGE, _params);

			// ASSERT
			_logger.Verify(x => x.LogError(_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForErrorLevelWithException()
		{
			// ACT
			_instance.LogError(_exception, _MESSAGE, _params);

			// ASSERT
			_logger.Verify(x => x.LogError(_exception, _MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForFatalLevel()
		{
			// ACT
			_instance.LogFatal(_MESSAGE, _params);

			// ASSERT
			_logger.Verify(x => x.LogFatal(_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForFatalLevelWithException()
		{
			// ACT
			_instance.LogFatal(_exception, _MESSAGE, _params);

			// ASSERT
			_logger.Verify(x => x.LogFatal(_exception, _MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}
	}
}