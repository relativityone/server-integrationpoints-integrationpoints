using System;
using System.Diagnostics;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation.Model;
using Relativity.API;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.Common.Monitoring.Instrumentation
{
	public class ExternalServiceInstrumentation : IExternalServiceInstrumentation, IExternalServiceInstrumentationStarted
	{
		private Stopwatch _stopwatch;

		private readonly IAPILog _logger;
		private readonly IMessageService _messageService;
		private readonly InstrumentationJobContext _jobContext;
		private readonly InstrumentationServiceCallContext _serviceCallContext;
		
		public ExternalServiceInstrumentation(InstrumentationJobContext jobJobContext, InstrumentationServiceCallContext serviceCallContext, IMessageService messageService, IAPILog logger)
		{
			_jobContext = jobJobContext;
			_serviceCallContext = serviceCallContext;
			_messageService = messageService;
			_logger = logger.ForContext<ExternalServiceInstrumentation>();
		}

		public IExternalServiceInstrumentationStarted Started()
		{
			if (_stopwatch != null)
			{
				throw new InvalidOperationException("Cannot be started again until completion.");
			}
			_stopwatch = Stopwatch.StartNew();

			return this;
		}

		public void Completed()
		{
			if (IsStarted)
			{
				long elapsed = GetElapsedMillisecondsAndRemoveStopwatch();
				ExternalCallCompletedMessage message = ExternalCallCompletedMessage.CreateSuccessMessage(_jobContext, _serviceCallContext, elapsed);
				SendMessage(message);
			}
		}

		public void Failed(string reason)
		{
			if (IsStarted)
			{
				SendFailedMessage(reason);
			}
			_logger.LogError("Call to external service failed. Service: {@serviceCallContext}, reason: {reason}", _serviceCallContext, reason);
		}

		public void Failed(Exception ex)
		{
			if (IsStarted)
			{
				SendFailedMessage(ex?.GetType().Name);
			}
			_logger.LogError(ex, "Call to external service failed. Service: {@serviceCallContext}", _serviceCallContext);
		}

		private bool IsStarted => _stopwatch != null;

		private long GetElapsedMillisecondsAndRemoveStopwatch()
		{
			_stopwatch.Stop();
			long elapsed = _stopwatch.ElapsedMilliseconds;
			_stopwatch = null;
			return elapsed;
		}

		private void SendFailedMessage(string reason)
		{
			ExternalCallCompletedMessage message = ExternalCallCompletedMessage.CreateFailureMessage(_jobContext, _serviceCallContext, GetElapsedMillisecondsAndRemoveStopwatch(), reason);
			SendMessage(message);
		}

		private void SendMessage(ExternalCallCompletedMessage message)
		{
			SendMessageAsyncFireAndForget(message);
		}

		private async void SendMessageAsyncFireAndForget(ExternalCallCompletedMessage message)
		{
			try
			{
				await _messageService.Send(message).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger?.LogWarning(ex, "Error occurred while sending metric message.");
			}
		}
	}
}
