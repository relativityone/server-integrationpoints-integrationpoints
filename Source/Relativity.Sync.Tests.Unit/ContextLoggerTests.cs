﻿using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class ContextLoggerTests
	{
		private ContextLogger _instance;

		private Mock<ISyncLog> _logger;

		private const string _KEY = "some key";

		private const string _MESSAGE_TEMPLATE = "message template {param1}";
		private const string _EXPECTED_MESSAGE = "{key} message template {param1}";

		private readonly object[] _params = {"param1", 1};
		private readonly object[] _expectedParams = {_KEY, "param1", 1};

		private readonly Exception _exception = new IOException();

		[SetUp]
		public void SetUp()
		{
			_logger = new Mock<ISyncLog>();

			_instance = new ContextLogger(new CorrelationId(_KEY), _logger.Object);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForVerboseLevel()
		{
			// ACT
			_instance.LogVerbose(_MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogVerbose(_EXPECTED_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForVerboseLevelWithException()
		{
			// ACT
			_instance.LogVerbose(_exception, _MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogVerbose(_exception, _EXPECTED_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForDebugLevel()
		{
			// ACT
			_instance.LogDebug(_MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogDebug(_EXPECTED_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForDebugLevelWithException()
		{
			// ACT
			_instance.LogDebug(_exception, _MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogDebug(_exception, _EXPECTED_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForInformationLevel()
		{
			// ACT
			_instance.LogInformation(_MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogInformation(_EXPECTED_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForInformationLevelWithException()
		{
			// ACT
			_instance.LogInformation(_exception, _MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogInformation(_exception, _EXPECTED_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForWarningLevel()
		{
			// ACT
			_instance.LogWarning(_MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogWarning(_EXPECTED_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForWarningLevelWithException()
		{
			// ACT
			_instance.LogWarning(_exception, _MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogWarning(_exception, _EXPECTED_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForErrorLevel()
		{
			// ACT
			_instance.LogError(_MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogError(_EXPECTED_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForErrorLevelWithException()
		{
			// ACT
			_instance.LogError(_exception, _MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogError(_exception, _EXPECTED_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForFatalLevel()
		{
			// ACT
			_instance.LogFatal(_MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogFatal(_EXPECTED_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}

		[Test]
		public void ItShouldPassModifiedMessageAndParametersForFatalLevelWithException()
		{
			// ACT
			_instance.LogFatal(_exception, _MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogFatal(_exception, _EXPECTED_MESSAGE, It.Is<object[]>(y => y.SequenceEqual(_expectedParams))), Times.Once);
		}
	}
}