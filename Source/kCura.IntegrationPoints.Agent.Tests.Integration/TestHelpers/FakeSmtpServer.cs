using kCura.IntegrationPoint.Tests.Core.Exceptions;
using netDumbster.smtp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.Tests.Integration.TestHelpers
{
	internal class FakeSmtpServer : IDisposable
	{
		private bool _isDisposed = false;

		private readonly SimpleSmtpServer _smtpServer;
		private readonly TaskCompletionSource<SmtpMessage> _receivedEmail;

		private FakeSmtpServer(SimpleSmtpServer smtpServer)
		{
			_receivedEmail = new TaskCompletionSource<SmtpMessage>();

			_smtpServer = smtpServer;
			smtpServer.MessageReceived += SmtpServerOnMessageReceived;
		}

		public static FakeSmtpServer Start(int port)
		{
			try
			{
				SimpleSmtpServer smtpServer = SimpleSmtpServer.Start(port);
				return new FakeSmtpServer(smtpServer);
			}
			catch (SocketException ex)
			{
				throw new TestSetupException($"An error occured while starting SMTP server. Verify that port '{port}' is available.", ex);
			}
		}

		/// <summary>
		/// Returns first received message or null if no message was received in a given timeout
		/// </summary>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public async Task<FakeSmtpMessage> GetFirstMessage(TimeSpan timeout)
		{
			await Task.WhenAny(
				_receivedEmail.Task,
				Task.Delay(timeout)
			).ConfigureAwait(false);

			return _receivedEmail.Task.IsCompleted
				? new FakeSmtpMessage(_receivedEmail.Task.Result)
				: null;
		}

		private void SmtpServerOnMessageReceived(object sender, MessageReceivedArgs e)
		{
			_receivedEmail.TrySetResult(e.Message);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				return;
			}
			_isDisposed = true;

			if (disposing)
			{
				_smtpServer.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
