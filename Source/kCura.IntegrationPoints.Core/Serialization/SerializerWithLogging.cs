﻿using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Serialization
{
	public class SerializerWithLogging : ISerializer
	{
		private readonly ISerializer _serializerImplementation;
		private readonly IAPILog _logger;

		public SerializerWithLogging(ISerializer serializer, IAPILog logger)
		{
			_serializerImplementation = serializer;
			_logger = logger?.ForContext<SerializerWithLogging>();
		}

		public string Serialize(object @object)
		{
			try
			{
				return _serializerImplementation.Serialize(@object);
			}
			catch (Exception e)
			{
				_logger?.LogError(e, "An error occured serializng object. Type: {objectType}", @object?.GetType());
				throw new IntegrationPointsException($"An error occured serializng object. Type: { @object?.GetType()}", e);
			}
		}

		public object Deserialize(Type objectType, string serializedString)
		{
			try
			{
				return _serializerImplementation.Deserialize(objectType, serializedString);
			}
			catch (Exception e)
			{
				_logger?.LogError(e, "An error occured deserializing object. Type: {objectType}", objectType);
				throw new IntegrationPointsException($"An error occured deserializing object. Type: { objectType}", e);
			}
		}

		public T Deserialize<T>(string serializedString)
		{
			try
			{
				return _serializerImplementation.Deserialize<T>(serializedString);
			}
			catch (Exception e)
			{
				_logger?.LogError(e, "An error occured deserializing object. Type: {objectType}", typeof(T));
				throw new IntegrationPointsException($"An error occured deserializing object. Type: { typeof(T)}", e);
			}
		}
	}
}
