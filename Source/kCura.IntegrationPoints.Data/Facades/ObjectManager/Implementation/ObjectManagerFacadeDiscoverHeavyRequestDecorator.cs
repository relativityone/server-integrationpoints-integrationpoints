﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Data.Facades.ObjectManager.DTOs;
using Relativity.API;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Facades.ObjectManager.Implementation
{
	internal class ObjectManagerFacadeDiscoverHeavyRequestDecorator : IObjectManagerFacade
	{
		private bool _disposedValue = false;

		private const int _MAX_COUNT_OF_COLLECTION_IN_REQUEST = 100000;
		private const string _UNKNOWN = "[UNKNOWN]";

		private readonly IObjectManagerFacade _objectManager;
		private readonly IAPILog _logger;

		/// <summary>
		/// Discovers heavy requests sent to Object Manager and logs them
		/// </summary>
		/// <param name="objectManager">This object will be disposed when Dispose is called</param>
		/// <param name="logger">Logger where discovery results will be sent to</param>
		public ObjectManagerFacadeDiscoverHeavyRequestDecorator(
			IObjectManagerFacade objectManager,
			IAPILog logger)
		{
			_objectManager = objectManager;
			_logger = logger.ForContext<ObjectManagerFacadeDiscoverHeavyRequestDecorator>();
		}

		public Task<CreateResult> CreateAsync(int workspaceArtifactID, CreateRequest request)
		{
			Func<string> getWarningMessageHeader =
				() => GetWarningMessageHeader<CreateRequest>(
					workspaceArtifactID,
					rdoArtifactId: _UNKNOWN,
					rdoType: request.ObjectType.Name);

			IEnumerable<FieldValueMap> fieldValues = request.FieldValues
				.Select(x => new FieldValueMap(x));

			AnalyzeFields(fieldValues, getWarningMessageHeader);

			return _objectManager.CreateAsync(workspaceArtifactID, request);
		}

		public async Task<ReadResult> ReadAsync(int workspaceArtifactID, ReadRequest request)
		{
			Func<string> getWarningMessageHeader =
				() => GetWarningMessageHeader<ReadRequest>(
					workspaceArtifactID,
					request.Object.ArtifactID.ToString(),
					rdoType: _UNKNOWN);

			ReadResult result = await _objectManager
				.ReadAsync(workspaceArtifactID, request)
				.ConfigureAwait(false);

			IEnumerable<FieldValueMap> fieldValues = result.Object.FieldValues
				.Select(x => new FieldValueMap(x));

			AnalyzeFields(fieldValues, getWarningMessageHeader);

			return result;
		}

		public Task<UpdateResult> UpdateAsync(int workspaceArtifactID, UpdateRequest request)
		{
			Func<string> getWarningMessageHeader =
				() => GetWarningMessageHeader<UpdateRequest>(
					workspaceArtifactID,
					request.Object.ArtifactID.ToString(),
					rdoType: _UNKNOWN);

			IEnumerable<FieldValueMap> fieldValues = request.FieldValues
				.Select(x => new FieldValueMap(x));

			AnalyzeFields(fieldValues, getWarningMessageHeader);

			return _objectManager.UpdateAsync(workspaceArtifactID, request);
		}

		public Task<MassUpdateResult> UpdateAsync(
			int workspaceArtifactID,
			MassUpdateByObjectIdentifiersRequest request,
			MassUpdateOptions updateOptions)
		{
			Func<string> getWarningMessage =
				() => GetWarningMessageHeader<UpdateRequest>(
					workspaceArtifactID,
					rdoArtifactId: _UNKNOWN,
					rdoType: _UNKNOWN);

			AnalyzeMassUpdateObjectsCollection(getWarningMessage, request);
			AnalyzeMassUpdateFields(getWarningMessage, workspaceArtifactID, request);

			return _objectManager.UpdateAsync(workspaceArtifactID, request, updateOptions);
		}

		public Task<DeleteResult> DeleteAsync(int workspaceArtifactID, DeleteRequest request)
		{
			return _objectManager.DeleteAsync(workspaceArtifactID, request);
		}

		public async Task<QueryResult> QueryAsync(int workspaceArtifactID, QueryRequest request, int start, int length)
		{
			Func<string> getWarningMessageHeader =
				() => GetWarningMessageHeader<QueryRequest>(
					workspaceArtifactID,
					rdoArtifactId: _UNKNOWN,
					rdoType: request.ObjectType.Name);

			QueryResult result = await _objectManager
				.QueryAsync(workspaceArtifactID, request, start, length)
				.ConfigureAwait(false);

			IEnumerable<FieldValueMap> fieldValues = result.Objects
				.SelectMany(x => x.FieldValues)
				.Select(x => new FieldValueMap(x));

			AnalyzeFields(fieldValues, getWarningMessageHeader);

			return result;
		}

		public Task<IKeplerStream> StreamLongTextAsync(int workspaceArtifactID, RelativityObjectRef exportObject,
			FieldRef longTextField)
		{
			return _objectManager.StreamLongTextAsync(workspaceArtifactID, exportObject, longTextField);
		}

		private void AnalyzeMassUpdateObjectsCollection(
			Func<string> getWarningMessageHeader,
			MassUpdateByObjectIdentifiersRequest request)
		{
			if (request.Objects.Count > _MAX_COUNT_OF_COLLECTION_IN_REQUEST)
			{
				string massUpdateWarningMessage = "Requested mass update operation exceeded max collection count" +
										$" - {request.Objects.Count}, when allowed is {_MAX_COUNT_OF_COLLECTION_IN_REQUEST}";

				string[] warningsToLog = {getWarningMessageHeader(), massUpdateWarningMessage};
				LogWarnings(warningsToLog);
			}
		}

		private void AnalyzeMassUpdateFields(
			Func<string> getWarningMessageHeader,
			int workspaceArtifactID,
			MassUpdateByObjectIdentifiersRequest request)
		{
			IEnumerable<FieldValueMap> fieldValues = request.FieldValues
				.Select(x => new FieldValueMap(x));

			AnalyzeFields(fieldValues, getWarningMessageHeader);
		}

		private void AnalyzeFields(
			IEnumerable<FieldValueMap> fieldValues,
			Func<string> getWarningMessageHeader)
		{
			IList<string> warnings = DiscoverFieldsCollectionsWhichExceedMaxCountValue(fieldValues);

			if (!warnings.Any())
			{
				return;
			}

			warnings.Insert(0, getWarningMessageHeader());
			LogWarnings(warnings);
		}

		private IList<string> DiscoverFieldsCollectionsWhichExceedMaxCountValue(
			IEnumerable<FieldValueMap> fieldValues)
		{
			return fieldValues
				.Select(fieldValue => new
				{
					FieldValue = fieldValue,
					Value = fieldValue.Value as ICollection
				})
				.Where(x => x.Value != null && x.Value.Count > _MAX_COUNT_OF_COLLECTION_IN_REQUEST)
				.Select(x =>
					$"Requested field {x.FieldValue.FieldName} exceeded max collection count" +
					$" - {x.Value.Count}, when allowed is {_MAX_COUNT_OF_COLLECTION_IN_REQUEST}")
				.ToList();
		}

		private void LogWarnings(IList<string> warnings)
		{
			warnings.ForEach(warning => _logger.LogWarning(warning));
		}

		private string GetWarningMessageHeader<T>(
			int workspaceArtifactId,
			string rdoArtifactId,
			string rdoType)
		{
			string operationName = GetOperationNameForRequestType<T>();
			return $"Heavy request discovered when executing {operationName}"
			 + $" on object of type [{rdoType}], id {rdoArtifactId} with ObjectManager"
			 + $" (Workspace: {workspaceArtifactId})";
		}

		private string GetOperationNameForRequestType<T>()
		{
			return typeof(T).Name.Replace("Request", string.Empty).ToUpperInvariant();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposedValue)
			{
				return;
			}
			if (disposing)
			{
				_objectManager?.Dispose();
			}

			_disposedValue = true;
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
