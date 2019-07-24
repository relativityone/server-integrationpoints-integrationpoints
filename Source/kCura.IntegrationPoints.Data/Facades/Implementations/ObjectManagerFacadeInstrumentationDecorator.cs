﻿using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;
using Relativity.Kepler.Exceptions;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Constants;
using Relativity.Kepler.Transport;
using IObjectManager = Relativity.Services.Objects.IObjectManager;

namespace kCura.IntegrationPoints.Data.Facades.Implementations
{
	internal class ObjectManagerFacadeInstrumentationDecorator : IObjectManagerFacade
	{
		private bool _isDisposed = false;

		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
		private readonly IAPILog _logger;
		private readonly IObjectManagerFacade _objectManager;

		public ObjectManagerFacadeInstrumentationDecorator(
			IObjectManagerFacade objectManager,
			IExternalServiceInstrumentationProvider instrumentationProvider, 
			IAPILog logger)
		{
			_objectManager = objectManager;
			_instrumentationProvider = instrumentationProvider;
			_logger = logger.ForContext<ObjectManagerFacadeInstrumentationDecorator>();
		}

		public async Task<CreateResult> CreateAsync(int workspaceArtifactID, CreateRequest createRequest)
		{
			IExternalServiceInstrumentationStarted startedInstrumentation = StartInstrumentation();
			CreateResult result =
				await Execute(x => x.CreateAsync(workspaceArtifactID, createRequest), startedInstrumentation)
					.ConfigureAwait(false);
			CompleteResultWithEventHandlers(result.EventHandlerStatuses, startedInstrumentation);
			return result;
		}

		public async Task<DeleteResult> DeleteAsync(int workspaceArtifactID, DeleteRequest request)
		{
			IExternalServiceInstrumentationStarted startedInstrumentation = StartInstrumentation();
			DeleteResult result =
				await Execute(x => x.DeleteAsync(workspaceArtifactID, request), startedInstrumentation)
					.ConfigureAwait(false);
			startedInstrumentation.Completed();
			return result;
		}

		public async Task<QueryResult> QueryAsync(int workspaceArtifactID, QueryRequest request, int start, int length)
		{
			IExternalServiceInstrumentationStarted startedInstrumentation = StartInstrumentation();
			QueryResult result =
				await Execute(x => x.QueryAsync(workspaceArtifactID, request, start, length), startedInstrumentation)
					.ConfigureAwait(false);
			startedInstrumentation.Completed();
			return result;
		}

		public async Task<ReadResult> ReadAsync(int workspaceArtifactID, ReadRequest request)
		{
			IExternalServiceInstrumentationStarted startedInstrumentation = StartInstrumentation();
			ReadResult result = await Execute(x => x.ReadAsync(workspaceArtifactID, request), startedInstrumentation)
				.ConfigureAwait(false);
			startedInstrumentation.Completed();
			return result;
		}

		public async Task<UpdateResult> UpdateAsync(int workspaceArtifactID, UpdateRequest request)
		{
			IExternalServiceInstrumentationStarted startedInstrumentation = StartInstrumentation();
			UpdateResult result =
				await Execute(x => x.UpdateAsync(workspaceArtifactID, request), startedInstrumentation)
					.ConfigureAwait(false);
			CompleteResultWithEventHandlers(result.EventHandlerStatuses, startedInstrumentation);
			return result;
		}

		public async Task<IKeplerStream> StreamLongTextAsync(int workspaceArtifactID, RelativityObjectRef exportObject, FieldRef fieldRef)
		{
			IExternalServiceInstrumentationStarted startedInstrumentation = StartInstrumentation();
			IKeplerStream result = 
				await Execute(x => 
					x.StreamLongTextAsync(workspaceArtifactID, exportObject, fieldRef), 
					startedInstrumentation)
				.ConfigureAwait(false);
			startedInstrumentation.Completed();
			return result;
		}

		private void CompleteResultWithEventHandlers(List<EventHandlerStatus> eventHandlerStatuses,
			IExternalServiceInstrumentationStarted startedInstrumentation)
		{
			string[] failedEventHandlersMessages =
				eventHandlerStatuses.Where(x => !x.Success).Select(x => x.Message).ToArray();
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

		private async Task<T> Execute<T>(Func<IObjectManagerFacade, Task<T>> f,
			IExternalServiceInstrumentationStarted instrumentation, [CallerMemberName]string operation = "")
		{
			try
			{
				return await f(_objectManager).ConfigureAwait(false);
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
				_instrumentationProvider.Create(ExternalServiceTypes.KEPLER, nameof(IObjectManager), operationName);
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
			if (_isDisposed)
			{
				return;
			}
			if (disposing)
			{
				_objectManager?.Dispose();
			}

			_isDisposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}