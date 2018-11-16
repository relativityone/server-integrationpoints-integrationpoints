using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;
using Relativity.Kepler.Exceptions;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using IObjectManager = Relativity.Services.Objects.IObjectManager;

namespace kCura.IntegrationPoints.Data.Facades.Implementations
{
	internal class ObjectManagerFacade : IObjectManagerFacade
	{
		private bool _isDisposed = false;

		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
		private readonly IAPILog _logger;
		private readonly Lazy<IObjectManager> _objectManager;

		public ObjectManagerFacade(Func<IObjectManager> objectManagerFactoryFactory,
			IExternalServiceInstrumentationProvider instrumentationProvider, IAPILog logger)
		{
			_objectManager = new Lazy<IObjectManager>(objectManagerFactoryFactory);
			_instrumentationProvider = instrumentationProvider;
			_logger = logger.ForContext<ObjectManagerFacade>();
		}

		public async Task<CreateResult> CreateAsync(int workspaceArtifactId, CreateRequest createRequest)
		{
			IExternalServiceInstrumentationStarted startedInstrumentation = StartInstrumentation();
			CreateResult result =
				await Execute(x => x.CreateAsync(workspaceArtifactId, createRequest), startedInstrumentation)
					.ConfigureAwait(false);
			CompleteResultWithEventHandlers(result.EventHandlerStatuses, startedInstrumentation);
			return result;
		}

		public async Task<DeleteResult> DeleteAsync(int workspaceArtifactId, DeleteRequest request)
		{
			IExternalServiceInstrumentationStarted startedInstrumentation = StartInstrumentation();
			DeleteResult result =
				await Execute(x => x.DeleteAsync(workspaceArtifactId, request), startedInstrumentation)
					.ConfigureAwait(false);
			startedInstrumentation.Completed();
			return result;
		}

		public async Task<QueryResult> QueryAsync(int workspaceArtifactId, QueryRequest request, int start, int length)
		{
			IExternalServiceInstrumentationStarted startedInstrumentation = StartInstrumentation();
			QueryResult result =
				await Execute(x => x.QueryAsync(workspaceArtifactId, request, start, length), startedInstrumentation)
					.ConfigureAwait(false);
			startedInstrumentation.Completed();
			return result;
		}

		public async Task<ReadResult> ReadAsync(int workspaceArtifactId, ReadRequest request)
		{
			IExternalServiceInstrumentationStarted startedInstrumentation = StartInstrumentation();
			ReadResult result = await Execute(x => x.ReadAsync(workspaceArtifactId, request), startedInstrumentation)
				.ConfigureAwait(false);
			startedInstrumentation.Completed();
			return result;
		}

		public async Task<UpdateResult> UpdateAsync(int workspaceArtifactId, UpdateRequest request)
		{
			IExternalServiceInstrumentationStarted startedInstrumentation = StartInstrumentation();
			UpdateResult result =
				await Execute(x => x.UpdateAsync(workspaceArtifactId, request), startedInstrumentation)
					.ConfigureAwait(false);
			CompleteResultWithEventHandlers(result.EventHandlerStatuses, startedInstrumentation);
			return result;
		}

		private void CompleteResultWithEventHandlers(List<EventHandlerStatus> eventHandlerStatuses,
			IExternalServiceInstrumentationStarted startedInstrumentation)
		{
			string[] failedEventHandlersMessages =
				eventHandlerStatuses.Where(x => x.Success != true).Select(x => x.Message).ToArray();
			if (failedEventHandlersMessages.Any())
			{
				string reason = string.Join(";", failedEventHandlersMessages);
				startedInstrumentation.Failed(reason);
			}
			else
			{
				startedInstrumentation.Completed();
			}
		}

		private async Task<T> Execute<T>(Func<IObjectManager, Task<T>> f,
			IExternalServiceInstrumentationStarted instrumentation, [CallerMemberName]string operation = "")
		{
			try
			{
				using (IObjectManager client = _objectManager.Value)
				{
					return await f(client).ConfigureAwait(false);
				}
			}
			catch (ServiceNotFoundException ex)
			{
				instrumentation.Failed(ex);
				throw LogServiceNotFoundException(operation, ex);
			}
			catch (Exception ex)
			{
				instrumentation.Failed(ex);
				throw;
			}
		}

		private IExternalServiceInstrumentationStarted StartInstrumentation([CallerMemberName] string operationName = "")
		{
			IExternalServiceInstrumentation instrumentation =
				_instrumentationProvider.Create("Kepler", nameof(IObjectManager), operationName);
			return instrumentation.Started();
		}

		private IntegrationPointsException LogServiceNotFoundException(string operationName,
			ServiceNotFoundException ex)
		{
			string message = $"Error while connecting to object manager service. Cannot perform {operationName} operation.";
			_logger.LogError(
				"Error while connecting to object manager service. Cannot perform {operationName} operation.",
				operationName);

			return new IntegrationPointsException(message, ex)
			{
				ShouldAddToErrorsTab = true,

				Source = IntegrationPointsExceptionSource.KEPLER
			};
		}

		#region IDisposable Support
		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					if (_objectManager.IsValueCreated)
					{
						_objectManager.Value.Dispose();
					}
				}

				_isDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}