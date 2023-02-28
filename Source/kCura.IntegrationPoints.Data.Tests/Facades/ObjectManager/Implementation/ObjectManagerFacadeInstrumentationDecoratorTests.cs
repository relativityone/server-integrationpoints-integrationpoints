using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Facades.ObjectManager;
using kCura.IntegrationPoints.Data.Facades.ObjectManager.Implementation;
using kCura.IntegrationPoints.Data.Tests.Facades.ObjectManager.Implementation.TestsHelpers;
using kCura.IntegrationPoints.Domain.Exceptions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Kepler.Exceptions;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Facades.ObjectManager.Implementation
{
    [TestFixture]
    [Category("Unit")]
    public class ObjectManagerFacadeInstrumentationDecoratorTests
    {
        private Mock<IObjectManagerFacade> _objectManagerMock;
        private Mock<IExternalServiceInstrumentationProvider> _instrumentationProviderMock;
        private Mock<IExternalServiceInstrumentation> _instrumentationMock;
        private Mock<IExternalServiceInstrumentationStarted> _startedInstrumentationMock;
        private ObjectManagerFacadeInstrumentationDecorator _sut;

        [SetUp]
        public void SetUp()
        {
            _objectManagerMock = new Mock<IObjectManagerFacade>();
            var loggerMock = new Mock<IAPILog>
            {
                DefaultValue = DefaultValue.Mock
            };
            _instrumentationMock = new Mock<IExternalServiceInstrumentation>();
            _startedInstrumentationMock = new Mock<IExternalServiceInstrumentationStarted>();
            _instrumentationMock
                .Setup(x => x.Started())
                .Returns(_startedInstrumentationMock.Object);
            _instrumentationProviderMock = new Mock<IExternalServiceInstrumentationProvider>();
            _instrumentationProviderMock
                .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_instrumentationMock.Object);
            _sut = new ObjectManagerFacadeInstrumentationDecorator(
                _objectManagerMock.Object,
                _instrumentationProviderMock.Object,
                loggerMock.Object);
        }

        [Test]
        public Task CreateAsync_ShouldCallStartedAndCompletedForSuccessfulCall()
        {
            return ShouldCallStartedAndCompletedForSuccessfulCallAsync(ObjectManagerFacadeTestsHelpers.CreateCallWithAnyArgs);
        }

        [Test]
        public Task CreateAsync_ShouldCallStartedAndFailedForResultWithFailedEventHandlers()
        {
            return ShouldCallStartedAndFailedForResultWithFailedEventHandlersAsync(
                ObjectManagerFacadeTestsHelpers.CreateCallWithAnyArgs,
                (result, statuses) => result.EventHandlerStatuses = statuses);
        }

        [Test]
        public Task CreateAsync_ShouldAggregateFailReasonsFromEventHandler()
        {
            return ShouldAggregateFailReasonsFromEventHandlerAsync(
                ObjectManagerFacadeTestsHelpers.CreateCallWithAnyArgs,
                (result, statuses) => result.EventHandlerStatuses = statuses);
        }

        [Test]
        public Task CreateAsync_ShouldCallFailedWhenExceptionIsThrown()
        {
            return ShouldCallFailedWhenExceptionIsThrownAsync(ObjectManagerFacadeTestsHelpers.CreateCallWithAnyArgs);
        }

        [Test]
        public void CreateAsync_ShouldRethrowExceptions()
        {
            ShouldRethrowExceptions(ObjectManagerFacadeTestsHelpers.CreateCallWithAnyArgs);
        }

        [Test]
        public void CreateAsync_ShouldWrapServiceNotFoundException()
        {
            ShouldWrapServiceNotFoundException(ObjectManagerFacadeTestsHelpers.CreateCallWithAnyArgs);
        }

        [Test]
        public Task ReadAsync_ShouldCallStartedAndCompletedForSuccessfulCall()
        {
            return ShouldCallStartedAndCompletedForSuccessfulCallAsync(ObjectManagerFacadeTestsHelpers.ReadCallWithAnyArgs);
        }

        [Test]
        public Task ReadAsync_ShouldCallFailedWhenExceptionIsThrown()
        {
            return ShouldCallFailedWhenExceptionIsThrownAsync(ObjectManagerFacadeTestsHelpers.ReadCallWithAnyArgs);
        }

        [Test]
        public void ReadAsync_ShouldRethrowExceptions()
        {
            ShouldRethrowExceptions(ObjectManagerFacadeTestsHelpers.ReadCallWithAnyArgs);
        }

        [Test]
        public void ReadAsync_ShouldWrapServiceNotFoundException()
        {
            ShouldWrapServiceNotFoundException(ObjectManagerFacadeTestsHelpers.ReadCallWithAnyArgs);
        }

        [Test]
        public Task UpdateAsync_ShouldCallStartedAndCompletedForSuccessfulCall()
        {
            return ShouldCallStartedAndCompletedForSuccessfulCallAsync(ObjectManagerFacadeTestsHelpers.UpdateCallWithAnyArgs);
        }

        [Test]
        public Task UpdateAsync_ShouldCallStartedAndFailedForResultWithFailedEventHandlers()
        {
            return ShouldCallStartedAndFailedForResultWithFailedEventHandlersAsync(
                ObjectManagerFacadeTestsHelpers.UpdateCallWithAnyArgs,
                (result, statuses) => result.EventHandlerStatuses = statuses);
        }

        [Test]
        public Task UpdateAsync_ShouldCallFailedWhenExceptionIsThrown()
        {
            return ShouldCallFailedWhenExceptionIsThrownAsync(ObjectManagerFacadeTestsHelpers.UpdateCallWithAnyArgs);
        }

        [Test]
        public void UpdateAsync_ShouldRethrowExceptions()
        {
            ShouldRethrowExceptions(ObjectManagerFacadeTestsHelpers.UpdateCallWithAnyArgs);
        }

        [Test]
        public void UpdateAsync_ShouldWrapServiceNotFoundException()
        {
            ShouldWrapServiceNotFoundException(ObjectManagerFacadeTestsHelpers.UpdateCallWithAnyArgs);
        }

        [Test]
        public Task UpdateAsync_MassUpdate_ShouldCallStartedAndCompletedForSuccessfulCall()
        {
            return ShouldCallStartedAndCompletedForSuccessfulCallAsync(
                ObjectManagerFacadeTestsHelpers.MassUpdateCallWithAnyArgs,
                setupResult: result => result.Success = true);
        }

        [Test]
        public Task UpdateAsync_MassUpdate_ShouldCallStartedAndFailedForNonSuccessfulCall()
        {
            return ShouldCallStartedAndFailedForNonSuccessfulCallAsync(
                ObjectManagerFacadeTestsHelpers.MassUpdateCallWithAnyArgs,
                setupResult: (result, failureReason) =>
                {
                    result.Success = false;
                    result.Message = failureReason;
                });
        }

        [Test]
        public Task UpdateAsync_MassUpdate_ShouldCallFailedWhenExceptionIsThrown()
        {
            return ShouldCallFailedWhenExceptionIsThrownAsync(ObjectManagerFacadeTestsHelpers.MassUpdateCallWithAnyArgs);
        }

        [Test]
        public void UpdateAsync_MassUpdate_ShouldRethrowExceptions()
        {
            ShouldRethrowExceptions(ObjectManagerFacadeTestsHelpers.MassUpdateCallWithAnyArgs);
        }

        [Test]
        public void UpdateAsync_MassUpdate_ShouldWrapServiceNotFoundException()
        {
            ShouldWrapServiceNotFoundException(ObjectManagerFacadeTestsHelpers.MassUpdateCallWithAnyArgs);
        }

        [Test]
        public Task DeleteAsync_ShouldCallStartedAndCompletedForSuccessfulCall()
        {
            return ShouldCallStartedAndCompletedForSuccessfulCallAsync(ObjectManagerFacadeTestsHelpers.DeleteCallWithAnyArgs);
        }

        [Test]
        public Task DeleteAsync_ShouldCallFailedWhenExceptionIsThrown()
        {
            return ShouldCallFailedWhenExceptionIsThrownAsync(ObjectManagerFacadeTestsHelpers.DeleteCallWithAnyArgs);
        }

        [Test]
        public void DeleteAsync_ShouldRethrowExceptions()
        {
            ShouldRethrowExceptions(ObjectManagerFacadeTestsHelpers.DeleteCallWithAnyArgs);
        }

        [Test]
        public void DeleteAsync_ShouldWrapServiceNotFoundException()
        {
            ShouldWrapServiceNotFoundException(ObjectManagerFacadeTestsHelpers.DeleteCallWithAnyArgs);
        }

        [Test]
        public Task QueryAsync_ShouldCallStartedAndCompletedForSuccessfulCall()
        {
            return ShouldCallStartedAndCompletedForSuccessfulCallAsync(ObjectManagerFacadeTestsHelpers.QueryCallWithAnyArgs);
        }

        [Test]
        public Task QueryAsync_ShouldCallFailedWhenExceptionIsThrown()
        {
            return ShouldCallFailedWhenExceptionIsThrownAsync(ObjectManagerFacadeTestsHelpers.QueryCallWithAnyArgs);
        }

        [Test]
        public void QueryAsync_ShouldRethrowExceptions()
        {
            ShouldRethrowExceptions(ObjectManagerFacadeTestsHelpers.QueryCallWithAnyArgs);
        }

        [Test]
        public void QueryAsync_ShouldWrapServiceNotFoundException()
        {
            ShouldWrapServiceNotFoundException(ObjectManagerFacadeTestsHelpers.QueryCallWithAnyArgs);
        }

        [Test]
        public Task QuerySlimAsync_ShouldCallStartedAndCompletedForSuccessfulCall()
        {
            return ShouldCallStartedAndCompletedForSuccessfulCallAsync(ObjectManagerFacadeTestsHelpers.QuerySlimCallWithAnyArgs);
        }

        [Test]
        public Task QuerySlimAsync_ShouldCallFailedWhenExceptionIsThrown()
        {
            return ShouldCallFailedWhenExceptionIsThrownAsync(ObjectManagerFacadeTestsHelpers.QuerySlimCallWithAnyArgs);
        }

        [Test]
        public void QuerySlimAsync_ShouldRethrowExceptions()
        {
            ShouldRethrowExceptions(ObjectManagerFacadeTestsHelpers.QuerySlimCallWithAnyArgs);
        }

        [Test]
        public void QuerySlimAsync_ShouldWrapServiceNotFoundException()
        {
            ShouldWrapServiceNotFoundException(ObjectManagerFacadeTestsHelpers.QuerySlimCallWithAnyArgs);
        }

        [Test]
        public Task InitializeExportAsync_ShouldCallStartedAndCompletedForSuccessfulCall()
        {
            return ShouldCallStartedAndCompletedForSuccessfulCallAsync(
                ObjectManagerFacadeTestsHelpers.InitializeExportCallWithAnyArgs);
        }

        [Test]
        public Task InitializeExportAsync_ShouldCallFailedWhenExceptionIsThrown()
        {
            return ShouldCallFailedWhenExceptionIsThrownAsync(ObjectManagerFacadeTestsHelpers.InitializeExportCallWithAnyArgs);
        }

        [Test]
        public void InitializeExportAsync_ShouldRethrowExceptions()
        {
            ShouldRethrowExceptions(ObjectManagerFacadeTestsHelpers.InitializeExportCallWithAnyArgs);
        }

        [Test]
        public void InitializeExportAsync_ShouldWrapServiceNotFoundException()
        {
            ShouldWrapServiceNotFoundException(ObjectManagerFacadeTestsHelpers.InitializeExportCallWithAnyArgs);
        }

        [Test]
        public Task RetrieveResultsBlockFromExportAsync_ShouldCallStartedAndCompletedForSuccessfulCall()
        {
            var result = new RelativityObjectSlim[] { };
            return ShouldCallStartedAndCompletedForSuccessfulCallWithResultAsync(
                ObjectManagerFacadeTestsHelpers.RetrieveResultsBlockFromExportCallWithAnyArgs,
                result);
        }

        [Test]
        public Task RetrieveResultsBlockFromExportAsync_ShouldCallFailedWhenExceptionIsThrown()
        {
            return ShouldCallFailedWhenExceptionIsThrownAsync(ObjectManagerFacadeTestsHelpers.InitializeExportCallWithAnyArgs);
        }

        [Test]
        public void RetrieveResultsBlockFromExportAsync_ShouldRethrowExceptions()
        {
            ShouldRethrowExceptions(ObjectManagerFacadeTestsHelpers.InitializeExportCallWithAnyArgs);
        }

        [Test]
        public void RetrieveResultsBlockFromExportAsync_ShouldWrapServiceNotFoundException()
        {
            ShouldWrapServiceNotFoundException(ObjectManagerFacadeTestsHelpers.InitializeExportCallWithAnyArgs);
        }

        private Task ShouldCallStartedAndCompletedForSuccessfulCallAsync<TResult>(
            Expression<Func<IObjectManagerFacade, Task<TResult>>> methodToTest,
            Action<TResult> setupResult = null)
            where TResult : new()
        {
            var result = new TResult();
            return ShouldCallStartedAndCompletedForSuccessfulCallWithResultAsync(methodToTest, result, setupResult);
        }

        private async Task ShouldCallStartedAndCompletedForSuccessfulCallWithResultAsync<TResult>(
            Expression<Func<IObjectManagerFacade, Task<TResult>>> methodToTest,
            TResult result,
            Action<TResult> setupResult = null)
        {
            // arrange
            setupResult?.Invoke(result);

            _objectManagerMock
                .Setup(methodToTest)
                .ReturnsAsync(result);

            Func<IObjectManagerFacade, Task<TResult>> compiledMethodToTest = methodToTest.Compile();

            // act
            await compiledMethodToTest(_sut).ConfigureAwait(false);

            // assert
            _instrumentationMock.Verify(x => x.Started());
            _startedInstrumentationMock.Verify(x => x.Completed());
        }

        private async Task ShouldCallStartedAndFailedForNonSuccessfulCallAsync<TResult>(
            Expression<Func<IObjectManagerFacade, Task<TResult>>> methodToTest,
            Action<TResult, string> setupResult)
            where TResult : new()
        {
            // arrange
            string failureReason = "access denied";
            var result = new TResult();
            setupResult.Invoke(result, failureReason);

            _objectManagerMock
                .Setup(methodToTest)
                .ReturnsAsync(result);

            Func<IObjectManagerFacade, Task<TResult>> compiledMethodToTest = methodToTest.Compile();

            // act
            await compiledMethodToTest(_sut).ConfigureAwait(false);

            // assert
            _instrumentationMock.Verify(x => x.Started());
            _startedInstrumentationMock.Verify(x => x.Failed(failureReason));
        }

        private async Task ShouldCallStartedAndFailedForResultWithFailedEventHandlersAsync<TResult>(
            Expression<Func<IObjectManagerFacade, Task<TResult>>> methodToTest,
            Action<TResult, List<EventHandlerStatus>> setEventHandlerStatuses)
            where TResult : new()
        {
            // arrange
            const string failReason = "Unauthorized";

            var result = new TResult();
            var eventHandlerStatuses = new List<EventHandlerStatus>
            {
                new EventHandlerStatus { Message = failReason, Success = false }
            };
            setEventHandlerStatuses(result, eventHandlerStatuses);

            _objectManagerMock
                .Setup(methodToTest)
                .ReturnsAsync(result);

            Func<IObjectManagerFacade, Task<TResult>> compiledMethodToTest = methodToTest.Compile();

            // act
            await compiledMethodToTest(_sut).ConfigureAwait(false);

            // assert
            _instrumentationMock.Verify(x => x.Started());
            _startedInstrumentationMock.Verify(x => x.Failed(failReason));
        }

        private async Task ShouldAggregateFailReasonsFromEventHandlerAsync<TResult>(
            Expression<Func<IObjectManagerFacade, Task<TResult>>> methodToTest,
            Action<TResult, List<EventHandlerStatus>> setEventHandlerStatuses)
            where TResult : new()
        {
            // arrange
            const string failReason1 = "Bad request";
            const string failReason2 = "Internal error";
            string expectedFailureReason = $"{failReason1};{failReason2}";

            var result = new TResult();
            var eventHandlerStatuses = new List<EventHandlerStatus>
            {
                new EventHandlerStatus { Message = failReason1, Success = false },
                new EventHandlerStatus { Message = failReason2, Success = false }
            };
            setEventHandlerStatuses(result, eventHandlerStatuses);

            _objectManagerMock
                .Setup(methodToTest)
                .ReturnsAsync(result);

            Func<IObjectManagerFacade, Task<TResult>> compiledMethodToTest = methodToTest.Compile();

            // act
            await compiledMethodToTest(_sut).ConfigureAwait(false);

            // assert
            _startedInstrumentationMock.Verify(x => x.Failed(expectedFailureReason));
        }

        private async Task ShouldCallFailedWhenExceptionIsThrownAsync<TResult>(Expression<Func<IObjectManagerFacade, Task<TResult>>> methodToTest)
        {
            // arrange
            var exception = new Exception();
            _objectManagerMock
                .Setup(methodToTest)
                .Throws(exception);

            Func<IObjectManagerFacade, Task<TResult>> compiledMethodToTest = methodToTest.Compile();

            // act
            try
            {
                await compiledMethodToTest(_sut).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // ignore
            }

            // assert
            _startedInstrumentationMock.Verify(x => x.Failed(exception));
        }

        private void ShouldRethrowExceptions<TResult>(Expression<Func<IObjectManagerFacade, Task<TResult>>> methodToTest)
        {
            // arrange
            var exception = new Exception();
            _objectManagerMock
                .Setup(methodToTest)
                .Throws(exception);

            Func<IObjectManagerFacade, Task<TResult>> compiledMethodToTest = methodToTest.Compile();

            // act
            Func<Task> executeMethodAction = () => compiledMethodToTest(_sut);

            // assert
            executeMethodAction.ShouldThrow<Exception>()
                .Which
                .Should().Be(exception);
        }

        private void ShouldWrapServiceNotFoundException<TResult>(Expression<Func<IObjectManagerFacade, Task<TResult>>> methodToTest)
        {
            // arrange
            var serviceNotFoundException = new ServiceNotFoundException();
            _objectManagerMock
                .Setup(methodToTest)
                .Throws(serviceNotFoundException);

            Func<IObjectManagerFacade, Task<TResult>> compiledMethodToTest = methodToTest.Compile();

            // act
            Func<Task> executeMethodAction = () => compiledMethodToTest(_sut);

            // assert
            executeMethodAction.ShouldThrow<IntegrationPointsException>()
                .WithInnerException<ServiceNotFoundException>()
                .And.InnerException
                .Should().Be(serviceNotFoundException);
        }
    }
}
