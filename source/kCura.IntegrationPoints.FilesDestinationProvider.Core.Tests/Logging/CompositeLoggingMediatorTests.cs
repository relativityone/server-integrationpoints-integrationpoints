using System.Collections.Generic;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.WinEDDS;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Logging
{
    public class CompositeLoggingMediatorTests
    {
        private CompositeLoggingMediator _compositeLoggingMediator;
        private IExporterStatusNotification _exporterStatusNotification;
        private IUserMessageNotification _userMessageNotification;

        [SetUp]
        public void SetUp()
        {
            _compositeLoggingMediator = new CompositeLoggingMediator();
            _exporterStatusNotification = Substitute.For<IExporterStatusNotification>();
            _userMessageNotification = Substitute.For<IUserMessageNotification>();
        }

        [Test]
        public void ItShouldRegisterAllChildren()
        {
            var loggingMediators = new List<ILoggingMediator>
            {
                Substitute.For<ILoggingMediator>(),
                Substitute.For<ILoggingMediator>(),
                Substitute.For<ILoggingMediator>()
            };

            foreach (var loggingMediator in loggingMediators)
            {
                _compositeLoggingMediator.AddLoggingMediator(loggingMediator);
            }

            _compositeLoggingMediator.RegisterEventHandlers(_userMessageNotification, _exporterStatusNotification);

            foreach (var loggingMediator in loggingMediators)
            {
                loggingMediator.Received().RegisterEventHandlers(_userMessageNotification, _exporterStatusNotification);
            }
        }

        [Test]
        public void ItShouldHandleEmptyChildrenList()
        {
            _compositeLoggingMediator.RegisterEventHandlers(_userMessageNotification, _exporterStatusNotification);
            Assert.Pass();
        }
    }
}