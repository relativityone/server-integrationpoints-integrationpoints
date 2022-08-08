using netDumbster.smtp;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    internal sealed class FakeSmtpServer : IDisposable
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
                throw new Exception($"An error occurred while starting SMTP server. Verify that port '{port}' is available.", ex);
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

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
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
            GC.SuppressFinalize(this);
        }
    }
}
