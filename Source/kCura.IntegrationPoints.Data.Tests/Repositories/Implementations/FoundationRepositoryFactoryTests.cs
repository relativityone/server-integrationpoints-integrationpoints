using System;
using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.API.Foundation;
using Relativity.API.Foundation.Repositories;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    [Ignore("Waiting for making IWorkspaceGateway public or applying [assembly: InternalsVisibleTo('DynamicProxyGenAssembly2')] in Relativity.API project")]
    public class FoundationRepositoryFactoryTests
    {
        private const int WORKSPACE_ID = 10001;
        private const string API_FOUNDATION = "API.Foundation";
        private const string IWORKSPACE_GATEWAY = "IWorkspaceGateway";
        private const string GET_WORKSPACE_CONTEXT = "GetWorkspaceContext";
        private const string IWORKSPACE_CONTEXT = "IWorkspaceContext";
        private const string CREATE_REPOSITORY = "CreateRepository";

        public static IEnumerable<ITestCase> TestCases()
        {
            yield return new TestCase<IFieldRepository>();
            yield return new TestCase<IRepository>();
            yield return new TestCase<IAuditRepository>();
        }

        [TestCaseSource(nameof(TestCases))]
        public void ShouldCallExecuteTwiceDuringInstrumentation(ITestCase testCase)
        {
            testCase.ShouldCallExecuteTwiceDuringInstrumentation();
        }

        [TestCaseSource(nameof(TestCases))]
        public void ShouldReturnRepositoryOfSameTypeAndImplementIRepository(ITestCase testCase)
        {
            testCase.ShouldReturnRepositoryOfSameTypeAndImplementIRepository();
        }

        [TestCaseSource(nameof(TestCases))]
        public void ShouldCallExecuteTwiceAndRethrowExceptionWhenSecondServiceThrowsDuringInstrumentation(ITestCase testCase)
        {
            testCase.ShouldCallExecuteTwiceAndRethrowExceptionWhenSecondServiceThrowsDuringInstrumentation();
        }

        [TestCaseSource(nameof(TestCases))]
        public void ShouldNotCallExecuteWhenFirstServiceThrowsDuringInstrumentation(ITestCase testCase)
        {
            testCase.ShouldCallExecuteOnceAndRethrowExceptionWhenFirstServiceThrowsDuringInstrumentation();
        }

        [TestCaseSource(nameof(TestCases))]
        public void ShouldCallExecuteOnceAndRethrowFirstExceptionWhenBothServicesThrowDuringInstrumentation(ITestCase testCase)
        {
            testCase.ShouldCallExecuteOnceAndRethrowFirstExceptionWhenBothServicesThrowDuringInstrumentation();
        }

        public interface ITestCase
        {
            void ShouldCallExecuteTwiceDuringInstrumentation();

            void ShouldReturnRepositoryOfSameTypeAndImplementIRepository();

            void ShouldCallExecuteTwiceAndRethrowExceptionWhenSecondServiceThrowsDuringInstrumentation();

            void ShouldCallExecuteOnceAndRethrowExceptionWhenFirstServiceThrowsDuringInstrumentation();

            void ShouldCallExecuteOnceAndRethrowFirstExceptionWhenBothServicesThrowDuringInstrumentation();
        }

        public class TestCase<T> : ITestCase where T : class, IRepository
        {
            private readonly IServicesMgr _servicesMgr;
            private readonly IWorkspaceGateway _workspaceGateway;
            private readonly IWorkspaceContext _workspaceContext;
            private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
            private readonly IExternalServiceSimpleInstrumentation _gatewayInstrumentation;
            private readonly IExternalServiceSimpleInstrumentation _contextInstrumentation;

            public TestCase()
            {
                _workspaceContext = Substitute.For<IWorkspaceContext>();
                _workspaceGateway = Substitute.For<IWorkspaceGateway>();
                _workspaceGateway.GetWorkspaceContext(Arg.Any<int>())
                    .Returns(_workspaceContext);
                _servicesMgr = Substitute.For<IServicesMgr>();
                _servicesMgr.CreateProxy<IWorkspaceGateway>(ExecutionIdentity.CurrentUser)
                    .Returns(_workspaceGateway);

                _gatewayInstrumentation = Substitute.For<IExternalServiceSimpleInstrumentation>();
                _contextInstrumentation = Substitute.For<IExternalServiceSimpleInstrumentation>();
                _instrumentationProvider = Substitute.For<IExternalServiceInstrumentationProvider>();

                _instrumentationProvider.CreateSimple(
                        API_FOUNDATION,
                        IWORKSPACE_GATEWAY,
                        GET_WORKSPACE_CONTEXT)
                    .Returns(_gatewayInstrumentation);
                _instrumentationProvider.CreateSimple(
                        API_FOUNDATION,
                        IWORKSPACE_CONTEXT,
                        CREATE_REPOSITORY)
                    .Returns(_contextInstrumentation);

                _gatewayInstrumentation.Execute(Arg.Any<Func<IWorkspaceContext>>())
                    .Returns(c => c.ArgAt<Func<IWorkspaceContext>>(0).Invoke());
                _contextInstrumentation.Execute(Arg.Any<Func<T>>())
                    .Returns(c => c.ArgAt<Func<T>>(0).Invoke());
            }

            public void ShouldCallExecuteTwiceDuringInstrumentation()
            {
                // arrange
                var sut = new FoundationRepositoryFactory(_servicesMgr, _instrumentationProvider);

                // act
                sut.GetRepository<T>(WORKSPACE_ID);

                // assert
                _instrumentationProvider.Received().CreateSimple(
                    API_FOUNDATION,
                    IWORKSPACE_GATEWAY,
                    GET_WORKSPACE_CONTEXT);
                _instrumentationProvider.Received().CreateSimple(
                    API_FOUNDATION,
                    IWORKSPACE_CONTEXT,
                    CREATE_REPOSITORY);
                _gatewayInstrumentation.Received()
                    .Execute(Arg.Any<Func<IWorkspaceContext>>());
                _contextInstrumentation.Received()
                    .Execute(Arg.Any<Func<T>>());
            }

            public void ShouldReturnRepositoryOfSameTypeAndImplementIRepository()
            {
                // arrange
                T repository = Substitute.For<T>();
                _workspaceContext.CreateRepository<T>().Returns(repository);
                var sut = new FoundationRepositoryFactory(_servicesMgr, _instrumentationProvider);

                // act
                T result = sut.GetRepository<T>(WORKSPACE_ID);

                // assert
                result.Should().Be(repository);
            }

            public void ShouldCallExecuteTwiceAndRethrowExceptionWhenSecondServiceThrowsDuringInstrumentation()
            {
                // arrange
                var exception = new InvalidOperationException();
                _workspaceContext.CreateRepository<T>().Throws(exception);
                var factory = new FoundationRepositoryFactory(_servicesMgr, _instrumentationProvider);

                // act
                Action sut = () => factory.GetRepository<T>(WORKSPACE_ID);

                // assert
                sut.ShouldThrowExactly<InvalidOperationException>();
                _instrumentationProvider.Received().CreateSimple(
                    API_FOUNDATION,
                    IWORKSPACE_GATEWAY,
                    GET_WORKSPACE_CONTEXT);
                _instrumentationProvider.Received().CreateSimple(
                    API_FOUNDATION,
                    IWORKSPACE_CONTEXT,
                    CREATE_REPOSITORY);
                _gatewayInstrumentation.Received()
                    .Execute(Arg.Any<Func<IWorkspaceContext>>());
                _contextInstrumentation.Received()
                    .Execute(Arg.Any<Func<T>>());
            }

            public void ShouldCallExecuteOnceAndRethrowExceptionWhenFirstServiceThrowsDuringInstrumentation()
            {
                // arrange
                var exception = new InvalidOperationException();
                _workspaceGateway.GetWorkspaceContext(Arg.Any<int>()).Throws(exception);
                var factory = new FoundationRepositoryFactory(_servicesMgr, _instrumentationProvider);

                // act
                Action sut = () => factory.GetRepository<T>(WORKSPACE_ID);

                // assert
                sut.ShouldThrowExactly<InvalidOperationException>();
                _instrumentationProvider.Received().CreateSimple(
                    API_FOUNDATION,
                    IWORKSPACE_GATEWAY,
                    GET_WORKSPACE_CONTEXT);
                _instrumentationProvider.Received().CreateSimple(
                    API_FOUNDATION,
                    IWORKSPACE_CONTEXT,
                    CREATE_REPOSITORY);
                _gatewayInstrumentation.Received()
                    .Execute(Arg.Any<Func<IWorkspaceContext>>());
                _contextInstrumentation.DidNotReceive()
                    .Execute(Arg.Any<Func<T>>());
            }

            public void ShouldCallExecuteOnceAndRethrowFirstExceptionWhenBothServicesThrowDuringInstrumentation()
            {
                // arrange
                var exception1 = new NullReferenceException();
                var exception2 = new InvalidOperationException();
                _workspaceGateway.GetWorkspaceContext(Arg.Any<int>()).Throws(exception1);
                _workspaceContext.CreateRepository<T>().Throws(exception2);
                var factory = new FoundationRepositoryFactory(_servicesMgr, _instrumentationProvider);

                // act
                Action sut = () => factory.GetRepository<T>(WORKSPACE_ID);

                // assert
                sut.ShouldThrowExactly<NullReferenceException>();
                _instrumentationProvider.Received().CreateSimple(
                    API_FOUNDATION,
                    IWORKSPACE_GATEWAY,
                    GET_WORKSPACE_CONTEXT);
                _instrumentationProvider.Received().CreateSimple(
                    API_FOUNDATION,
                    IWORKSPACE_CONTEXT,
                    CREATE_REPOSITORY);
                _gatewayInstrumentation.Received()
                    .Execute(Arg.Any<Func<IWorkspaceContext>>());
                _contextInstrumentation.DidNotReceive()
                    .Execute(Arg.Any<Func<T>>());
            }
        }
    }
}
