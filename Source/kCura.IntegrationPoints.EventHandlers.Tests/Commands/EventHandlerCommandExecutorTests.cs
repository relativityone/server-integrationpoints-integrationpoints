using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture, Category("Unit")]
    public class EventHandlerCommandExecutorTests : TestBase
    {
        private const string _SUCCESS_MESSAGE = "success_message_326";
        private const string _FAILURE_MESSAGE = "failure_message_219";

        private EventHandlerCommandExecutor _instance;
        private IAPILog _logger;
        private ICommand _command;

        public override void SetUp()
        {
            _command = Substitute.For<ICommand>();
            _command.SuccessMessage.Returns(_SUCCESS_MESSAGE);
            _command.FailureMessage.Returns(_FAILURE_MESSAGE);
            _logger = Substitute.For<IAPILog>();
            _instance = new EventHandlerCommandExecutor(_logger);
        }

        [Test]
        public void ItShouldExecuteCommand()
        {
            var response = _instance.Execute(_command);

            Assert.That(response.Success, Is.True);
            Assert.That(response.Message, Is.EqualTo(_SUCCESS_MESSAGE));

            _command.Received(1).Execute();
        }

        [Test]
        public void ItShouldHandleUnknownException()
        {
            var expectedException = new Exception("message_707");
            _command.When(x => x.Execute()).Do(x => { throw expectedException; });

            var response = _instance.Execute(_command);

            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Is.EqualTo(_FAILURE_MESSAGE));
            Assert.That(response.Exception.Message, Is.EqualTo(expectedException.Message));

            _logger.Received(1).LogError(expectedException, "Failed to execute Event Handler Command. {message}", _FAILURE_MESSAGE);
        }

        [Test]
        public void ItShouldHandleCommandExecutionException()
        {
            var expectedException = new CommandExecutionException("message_683");
            _command.When(x => x.Execute()).Do(x => { throw expectedException; });

            var response = _instance.Execute(_command);

            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Is.EqualTo(expectedException.Message));
            Assert.That(response.Exception.Message, Is.EqualTo(expectedException.Message));

            _logger.Received(1).LogError(expectedException, "Failed to execute Event Handler Command. {message}", expectedException.Message);
        }
    }
}
