using System;
using System.IO;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class SyncLogWriterTests
	{
		private SyncLogWriter _instance;

		private Mock<IAPILog> _logger;

		private const string _MESSAGE_TEMPLATE = "message template {param1}";
		private const string _UNABLE_TO_RESOLVE_MESSAGE = "Unable to resolve logging message due to framework limitation.";

		private readonly object[] _params = {"param1", 1};

		private readonly Exception _exception = new IOException();

		[SetUp]
		public void SetUp()
		{
			_logger = new Mock<IAPILog>();

			_instance = new SyncLogWriter(_logger.Object);
		}

		[Test]
		public void ItShouldPassMessageAndParametersForDebugLevel()
		{
			// ACT
			_instance.Debug(_MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogDebug(_MESSAGE_TEMPLATE, _params), Times.Once);
		}

		[Test]
		public void ItShouldPassMessageAndParametersForDebugLevelWithException()
		{
			// ACT
			_instance.Debug(_MESSAGE_TEMPLATE, _exception);

			// ASSERT
			_logger.Verify(x => x.LogDebug(_exception, _MESSAGE_TEMPLATE), Times.Once);
		}

		[Test]
		public void ItShouldPassExceptionButSkipMessageResolutionForDebugLevel()
		{
			// ACT
			_instance.Debug(() => _MESSAGE_TEMPLATE, _exception);

			// ASSERT
			_logger.Verify(x => x.LogDebug(_exception, _UNABLE_TO_RESOLVE_MESSAGE), Times.Once);
		}

		[Test]
		public void ItShouldPassMessageAndParametersForInformationLevel()
		{
			// ACT
			_instance.Info(_MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogInformation(_MESSAGE_TEMPLATE, _params), Times.Once);
		}

		[Test]
		public void ItShouldPassMessageAndParametersForInformationLevelWithException()
		{
			// ACT
			_instance.Info(_MESSAGE_TEMPLATE, _exception);

			// ASSERT
			_logger.Verify(x => x.LogInformation(_exception, _MESSAGE_TEMPLATE), Times.Once);
		}

		[Test]
		public void ItShouldPassExceptionButSkipMessageResolutionForInformationLevel()
		{
			// ACT
			_instance.Info(() => _MESSAGE_TEMPLATE, _exception);

			// ASSERT
			_logger.Verify(x => x.LogInformation(_exception, _UNABLE_TO_RESOLVE_MESSAGE), Times.Once);
		}

		[Test]
		public void ItShouldPassMessageAndParametersForWarningLevel()
		{
			// ACT
			_instance.Warn(_MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogWarning(_MESSAGE_TEMPLATE, _params), Times.Once);
		}

		[Test]
		public void ItShouldPassMessageAndParametersForWarningLevelWithException()
		{
			// ACT
			_instance.Warn(_MESSAGE_TEMPLATE, _exception);

			// ASSERT
			_logger.Verify(x => x.LogWarning(_exception, _MESSAGE_TEMPLATE), Times.Once);
		}

		[Test]
		public void ItShouldPassExceptionButSkipMessageResolutionForWarningLevel()
		{
			// ACT
			_instance.Warn(() => _MESSAGE_TEMPLATE, _exception);

			// ASSERT
			_logger.Verify(x => x.LogWarning(_exception, _UNABLE_TO_RESOLVE_MESSAGE), Times.Once);
		}

		[Test]
		public void ItShouldPassMessageAndParametersForErrorLevel()
		{
			// ACT
			_instance.Error(_MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogError(_MESSAGE_TEMPLATE, _params), Times.Once);
		}

		[Test]
		public void ItShouldPassMessageAndParametersForErrorLevelWithException()
		{
			// ACT
			_instance.Error(_MESSAGE_TEMPLATE, _exception);

			// ASSERT
			_logger.Verify(x => x.LogError(_exception, _MESSAGE_TEMPLATE), Times.Once);
		}

		[Test]
		public void ItShouldPassExceptionButSkipMessageResolutionForErrorLevel()
		{
			// ACT
			_instance.Error(() => _MESSAGE_TEMPLATE, _exception);

			// ASSERT
			_logger.Verify(x => x.LogError(_exception, _UNABLE_TO_RESOLVE_MESSAGE), Times.Once);
		}

		[Test]
		public void ItShouldPassMessageAndParametersForFatalLevel()
		{
			// ACT
			_instance.Fatal(_MESSAGE_TEMPLATE, _params);

			// ASSERT
			_logger.Verify(x => x.LogFatal(_MESSAGE_TEMPLATE, _params), Times.Once);
		}

		[Test]
		public void ItShouldPassMessageAndParametersForFatalLevelWithException()
		{
			// ACT
			_instance.Fatal(_MESSAGE_TEMPLATE, _exception);

			// ASSERT
			_logger.Verify(x => x.LogFatal(_exception, _MESSAGE_TEMPLATE), Times.Once);
		}

		[Test]
		public void ItShouldPassExceptionButSkipMessageResolutionForFatalLevel()
		{
			// ACT
			_instance.Fatal(() => _MESSAGE_TEMPLATE, _exception);

			// ASSERT
			_logger.Verify(x => x.LogFatal(_exception, _UNABLE_TO_RESOLVE_MESSAGE), Times.Once);
		}
	}
}
