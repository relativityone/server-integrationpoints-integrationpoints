using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Moq.Language;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core.Extensions.Moq
{
    public static class MoqExtensions
    {
        public static ISetupSequentialResult<T> Returns<T>(
            this ISetupSequentialResult<T> setupSequentialResult,
            IEnumerable<T> valuesToReturn)
        {
            return valuesToReturn
                .Aggregate(
                    setupSequentialResult,
                    (current, valueToReturn) => current.Returns(valueToReturn));
        }

        public static void SetupLog(this Mock<IAPILog> loggerMock)
        {
            loggerMock.Setup(m => m.ForContext(It.IsAny<Type>())).Returns(loggerMock.Object);
            loggerMock.Setup(m => m.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>())).Returns(loggerMock.Object);
            loggerMock.Setup(m => m.LogContextPushProperty(It.IsAny<string>(), It.IsAny<object>())).Returns((IDisposable)null);

            loggerMock.Setup(m => m.LogDebug(It.IsAny<string>(), It.IsAny<string[]>()));
            loggerMock.Setup(m => m.LogDebug(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string[]>()));
            loggerMock.Setup(m => m.LogError(It.IsAny<string>(), It.IsAny<string[]>()));
            loggerMock.Setup(m => m.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string[]>()));
            loggerMock.Setup(m => m.LogFatal(It.IsAny<string>(), It.IsAny<string[]>()));
            loggerMock.Setup(m => m.LogFatal(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string[]>()));
            loggerMock.Setup(m => m.LogInformation(It.IsAny<string>(), It.IsAny<string[]>()));
            loggerMock.Setup(m => m.LogInformation(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string[]>()));
            loggerMock.Setup(m => m.LogVerbose(It.IsAny<string>(), It.IsAny<string[]>()));
            loggerMock.Setup(m => m.LogVerbose(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string[]>()));
            loggerMock.Setup(m => m.LogWarning(It.IsAny<string>(), It.IsAny<string[]>()));
            loggerMock.Setup(m => m.LogWarning(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string[]>()));
        }

        public static void SetupLog<T>(this Mock<IAPILog> loggerMock) where T : class
        {
            loggerMock.Setup(m => m.ForContext<T>()).Returns(loggerMock.Object);

            SetupLog(loggerMock);
        }
    }
}
