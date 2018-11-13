using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Core.Monitoring.Instrumentation;
using NSubstitute;
using NUnit.Framework;
using System;

namespace kCura.IntegrationPoints.Core.Tests.Monitoring.Instrumentation
{
	[TestFixture]
	public class ExternalServiceSimpleInstrumentationTests
	{
		private IExternalServiceInstrumentation _instrumentation;
		private IExternalServiceInstrumentationStarted _startedInstrumentation;
		private ExternalServiceSimpleInstrumentation _sut;

		private const int _EXPECTED_RETURN_VALUE = 4324324;

		[SetUp]
		public void SetUp()
		{
			_startedInstrumentation = Substitute.For<IExternalServiceInstrumentationStarted>();
			_instrumentation = Substitute.For<IExternalServiceInstrumentation>();
			_instrumentation.Started().Returns(_startedInstrumentation);
			_sut = new ExternalServiceSimpleInstrumentation(_instrumentation);
		}

		[Test]
		public void ItShouldCallStartedBeforeActionWasExecuted()
		{
			// act & assert
			_sut.Execute(ValidateActionExecution);
		}
		
		[Test]
		public void ItShouldCallCompletedWhenActionExecutedWithoutException()
		{
			// act
			_sut.Execute(() => { });

			// assert
			_startedInstrumentation.Received(1).Completed();
		}

		[Test]
		public void ItShouldCallFailedAndRethrowExceptionWhenActionThrowsException()
		{
			// arrange
			var expectedException = new ArgumentException();
			
			// act
			try
			{
				_sut.Execute(() => { throw expectedException; });
			}
			catch (Exception ex)
			{
				// assert
				Assert.AreEqual(expectedException, ex);
			}

			// assert
			_startedInstrumentation.Received(1).Failed(expectedException);
		}

		[Test]
		public void ItShouldCallStartedBeforeFunctionWasExecuted()
		{
			// act & assert
			_sut.Execute(ValidateFunctionExecution);
		}

		[Test]
		public void ItShouldCallCompletedWhenFunctionWasExecutedWithoutException()
		{
			// act
			_sut.Execute(() => _EXPECTED_RETURN_VALUE);

			// assert
			_startedInstrumentation.Received(1).Completed();
		}

		[Test]
		public void ItShouldReturnValueWhenFunctionWasExecutedWithoutException()
		{
			// act
			int result = _sut.Execute(() => _EXPECTED_RETURN_VALUE);

			// assert
			Assert.AreEqual(_EXPECTED_RETURN_VALUE, result);
		}

		[Test]
		public void ItShouldCallFailedAndRethrowExceptionWhenFunctionThrowsException()
		{
			// arrange
			var expectedException = new ArgumentException();
			Func<string> functionToExecute = () => { throw expectedException; };
			// act
			try
			{
				_sut.Execute(functionToExecute);
			}
			catch (Exception ex)
			{
				// assert
				Assert.AreEqual(expectedException, ex);
			}

			// assert
			_startedInstrumentation.Received(1).Failed(expectedException);
		}

		private int ValidateFunctionExecution()
		{
			ValidateActionExecution();
			return _EXPECTED_RETURN_VALUE;
		}

		private void ValidateActionExecution()
		{
			_instrumentation.Received(1).Started();
			_startedInstrumentation.DidNotReceiveWithAnyArgs().Completed();
			_startedInstrumentation.DidNotReceiveWithAnyArgs().Failed(Arg.Any<string>());
			_startedInstrumentation.DidNotReceiveWithAnyArgs().Failed(Arg.Any<Exception>());
		}
	}
}
