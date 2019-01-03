using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Facades.Implementations;
using kCura.IntegrationPoints.Domain.Exceptions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Kepler.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Tests.Facades.Implementations
{
	[TestFixture]
	public class ObjectManagerFacadeTests
	{
		private IObjectManager _objectManager;
		private IExternalServiceInstrumentationProvider _instrumentationProvider;
		private IExternalServiceInstrumentation _instrumentation;
		private IExternalServiceInstrumentationStarted _startedInstrumentation;
		private IAPILog _logger;
		private ObjectManagerFacade _sut;

		[SetUp]
		public void SetUp()
		{
			_objectManager = Substitute.For<IObjectManager>();
			_logger = Substitute.For<IAPILog>();
			_instrumentation = Substitute.For<IExternalServiceInstrumentation>();
			_startedInstrumentation = Substitute.For<IExternalServiceInstrumentationStarted>();
			_instrumentation.Started().Returns(_startedInstrumentation);
			_instrumentationProvider = Substitute.For<IExternalServiceInstrumentationProvider>();
			_instrumentationProvider.Create(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(_instrumentation);
			_sut = new ObjectManagerFacade(() => _objectManager, _instrumentationProvider, _logger);
		}

		[Test]
		public async Task ItShouldCallStartedAndCompletedForSuccessfulCall_Create()
		{
			// arrange
			_objectManager.CreateAsync(Arg.Any<int>(), Arg.Any<CreateRequest>()).Returns(new CreateResult());

			// act
			await _sut.CreateAsync(0, null);

			// assert
			_instrumentation.Received().Started();
			_startedInstrumentation.Received().Completed();
		}

		[Test]
		public async Task ItShouldCallStartedAndFailedForResultWithFailedEventHandlers_Create()
		{
			// arrange
			string failReason = "Bad request";
			var result = new CreateResult
			{
				EventHandlerStatuses = { new EventHandlerStatus { Message = failReason, Success = false } }
			};
			_objectManager.CreateAsync(Arg.Any<int>(), Arg.Any<CreateRequest>()).Returns(result);

			// act
			await _sut.CreateAsync(0, null);

			// assert
			_startedInstrumentation.Received().Failed(failReason);
		}

		[Test]
		public async Task ItShouldAggregateFailReasonsFromEventHandler_Create()
		{
			// arrange
			string failReason1 = "Bad request";
			string failReason2 = "Internal error";
			string expectedFailureReason = $"{failReason1};{failReason2}";
			var result = new CreateResult
			{
				EventHandlerStatuses =
				{
					new EventHandlerStatus { Message = failReason1, Success = false },
					new EventHandlerStatus { Message = failReason2, Success = false }
				}
			};
			_objectManager.CreateAsync(Arg.Any<int>(), Arg.Any<CreateRequest>()).Returns(result);

			// act
			await _sut.CreateAsync(0, null);

			// assert
			_startedInstrumentation.Received().Failed(expectedFailureReason);
		}

		[Test]
		public async Task ItShouldCallFailedWhenExceptionIsThrown_Create()
		{
			// arrange
			var exception = new Exception();
			_objectManager.CreateAsync(Arg.Any<int>(), Arg.Any<CreateRequest>()).Throws(exception);

			// act
			try
			{
				await _sut.CreateAsync(0, null);
			}
			catch (Exception)
			{
				// ignore
			}

			// assert
			_startedInstrumentation.Received().Failed(exception);
		}

		[Test]
		public async Task ItShouldRethrowExceptions_Create()
		{
			// arrange
			var exception = new Exception();
			_objectManager.CreateAsync(Arg.Any<int>(), Arg.Any<CreateRequest>()).Throws(exception);

			// act
			try
			{
				await _sut.CreateAsync(0, null);

				// assert
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.AreEqual(exception, ex);
			}
		}

		[Test]
		public async Task ItShouldWrapServiceNotFoundException_Create()
		{
			// arrange
			var exception = new ServiceNotFoundException();
			_objectManager.CreateAsync(Arg.Any<int>(), Arg.Any<CreateRequest>()).Throws(exception);

			// act
			try
			{
				await _sut.CreateAsync(0, null);

				// assert
				Assert.Fail();
			}
			catch (IntegrationPointsException ex)
			{
				Assert.AreEqual(exception, ex.InnerException);
			}
		}

		[Test]
		public async Task ItShouldCallStartedAndCompletedForSuccessfulCall_Read()
		{
			// arrange
			_objectManager.ReadAsync(Arg.Any<int>(), Arg.Any<ReadRequest>()).Returns(new ReadResult());

			// act
			await _sut.ReadAsync(0, null);

			// assert
			_instrumentation.Received().Started();
			_startedInstrumentation.Received().Completed();
		}

		[Test]
		public async Task ItShouldCallFailedWhenExceptionIsThrown_Read()
		{
			// arrange
			var exception = new Exception();
			_objectManager.ReadAsync(Arg.Any<int>(), Arg.Any<ReadRequest>()).Throws(exception);

			// act
			try
			{
				await _sut.ReadAsync(0, null);
			}
			catch (Exception)
			{
				// ignore
			}

			// assert
			_startedInstrumentation.Received().Failed(exception);
		}

		[Test]
		public async Task ItShouldRethrowExceptions_Read()
		{
			// arrange
			var exception = new Exception();
			_objectManager.ReadAsync(Arg.Any<int>(), Arg.Any<ReadRequest>()).Throws(exception);

			// act
			try
			{
				await _sut.ReadAsync(0, null);

				// assert
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.AreEqual(exception, ex);
			}
		}

		[Test]
		public async Task ItShouldWrapServiceNotFoundException_Read()
		{
			// arrange
			var exception = new ServiceNotFoundException();
			_objectManager.ReadAsync(Arg.Any<int>(), Arg.Any<ReadRequest>()).Throws(exception);

			// act
			try
			{
				await _sut.ReadAsync(0, null);

				// assert
				Assert.Fail();
			}
			catch (IntegrationPointsException ex)
			{
				Assert.AreEqual(exception, ex.InnerException);
			}
		}

		[Test]
		public async Task ItShouldCallStartedAndCompletedForSuccessfulCall_Update()
		{
			// arrange
			_objectManager.UpdateAsync(Arg.Any<int>(), Arg.Any<UpdateRequest>()).Returns(new UpdateResult());

			// act
			await _sut.UpdateAsync(0, null);

			// assert
			_instrumentation.Received().Started();
			_startedInstrumentation.Received().Completed();
		}

		[Test]
		public async Task ItShouldCallStartedAndFailedForResultWithFailedEventHandlers_Update()
		{
			// arrange
			string failReason = "Unauthorized";
			var result = new UpdateResult
			{
				EventHandlerStatuses = { new EventHandlerStatus { Message = failReason, Success = false } }
			};
			_objectManager.UpdateAsync(Arg.Any<int>(), Arg.Any<UpdateRequest>()).Returns(result);

			// act
			await _sut.UpdateAsync(0, null);

			// assert
			_instrumentation.Received().Started();
			_startedInstrumentation.Received().Failed(failReason);
		}

		[Test]
		public async Task ItShouldCallFailedWhenExceptionIsThrown_Update()
		{
			// arrange
			var exception = new Exception();
			_objectManager.UpdateAsync(Arg.Any<int>(), Arg.Any<UpdateRequest>()).Throws(exception);

			// act
			try
			{
				await _sut.UpdateAsync(0, null);
			}
			catch (Exception)
			{
				// ignore
			}

			// assert
			_startedInstrumentation.Received().Failed(exception);
		}

		[Test]
		public async Task ItShouldRethrowExceptions_Update()
		{
			// arrange
			var exception = new Exception();
			_objectManager.UpdateAsync(Arg.Any<int>(), Arg.Any<UpdateRequest>()).Throws(exception);

			// act
			try
			{
				await _sut.UpdateAsync(0, null);

				// assert
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.AreEqual(exception, ex);
			}
		}

		[Test]
		public async Task ItShouldWrapServiceNotFoundException_Update()
		{
			// arrange
			var exception = new ServiceNotFoundException();
			_objectManager.UpdateAsync(Arg.Any<int>(), Arg.Any<UpdateRequest>()).Throws(exception);

			// act
			try
			{
				await _sut.UpdateAsync(0, null);

				// assert
				Assert.Fail();
			}
			catch (IntegrationPointsException ex)
			{
				Assert.AreEqual(exception, ex.InnerException);
			}
		}

		[Test]
		public async Task ItShouldCallStartedAndCompletedForSuccessfulCall_Delete()
		{
			// arrange
			_objectManager.DeleteAsync(Arg.Any<int>(), Arg.Any<DeleteRequest>()).Returns(new DeleteResult());

			// act
			await _sut.DeleteAsync(0, null);

			// assert
			_instrumentation.Received().Started();
			_startedInstrumentation.Received().Completed();
		}

		[Test]
		public async Task ItShouldCallFailedWhenExceptionIsThrown_Delete()
		{
			// arrange
			var exception = new Exception();
			_objectManager.DeleteAsync(Arg.Any<int>(), Arg.Any<DeleteRequest>()).Throws(exception);

			// act
			try
			{
				await _sut.DeleteAsync(0, null);
			}
			catch (Exception)
			{
				// ignore
			}

			// assert
			_startedInstrumentation.Received().Failed(exception);
		}

		[Test]
		public async Task ItShouldRethrowExceptions_Delete()
		{
			// arrange
			var exception = new Exception();
			_objectManager.DeleteAsync(Arg.Any<int>(), Arg.Any<DeleteRequest>()).Throws(exception);

			// act
			try
			{
				await _sut.DeleteAsync(0, null);

				// assert
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.AreEqual(exception, ex);
			}
		}

		[Test]
		public async Task ItShouldWrapServiceNotFoundException_Delete()
		{
			// arrange
			var exception = new ServiceNotFoundException();
			_objectManager.DeleteAsync(Arg.Any<int>(), Arg.Any<DeleteRequest>()).Throws(exception);

			// act
			try
			{
				await _sut.DeleteAsync(0, null);

				// assert
				Assert.Fail();
			}
			catch (IntegrationPointsException ex)
			{
				Assert.AreEqual(exception, ex.InnerException);
			}
		}

		[Test]
		public async Task ItShouldCallStartedAndCompletedForSuccessfulCall_Query()
		{
			// arrange
			_objectManager.QueryAsync(Arg.Any<int>(), Arg.Any<QueryRequest>(), Arg.Any<int>(), Arg.Any<int>()).Returns(new QueryResult());

			// act
			await _sut.QueryAsync(0, null, 0, 0);

			// assert
			_instrumentation.Received().Started();
			_startedInstrumentation.Received().Completed();
		}

		[Test]
		public async Task ItShouldCallFailedWhenExceptionIsThrown_Query()
		{
			// arrange
			var exception = new Exception();
			_objectManager.QueryAsync(Arg.Any<int>(), Arg.Any<QueryRequest>(), Arg.Any<int>(), Arg.Any<int>()).Throws(exception);

			// act
			try
			{
				await _sut.QueryAsync(0, null, 0, 0);
			}
			catch (Exception)
			{
				// ignore
			}

			// assert
			_startedInstrumentation.Received().Failed(exception);
		}

		[Test]
		public async Task ItShouldRethrowExceptions_Query()
		{
			// arrange
			var exception = new Exception();
			_objectManager.QueryAsync(Arg.Any<int>(), Arg.Any<QueryRequest>(), Arg.Any<int>(), Arg.Any<int>()).Throws(exception);

			// act
			try
			{
				await _sut.QueryAsync(0, null, 0, 0);

				// assert
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.AreEqual(exception, ex);
			}
		}

		[Test]
		public async Task ItShouldWrapServiceNotFoundException_Query()
		{
			// arrange
			var exception = new ServiceNotFoundException();
			_objectManager.QueryAsync(Arg.Any<int>(), Arg.Any<QueryRequest>(), Arg.Any<int>(), Arg.Any<int>()).Throws(exception);

			// act
			try
			{
				await _sut.QueryAsync(0, null, 0, 0);

				// assert
				Assert.Fail();
			}
			catch (IntegrationPointsException ex)
			{
				Assert.AreEqual(exception, ex.InnerException);
			}
		}
	}
}
