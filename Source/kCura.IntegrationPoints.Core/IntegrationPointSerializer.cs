﻿using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Serialization;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class IntegrationPointSerializer : IIntegrationPointSerializer
	{
		private readonly ISerializer _serializer;

		public IntegrationPointSerializer(IAPILog logger)
		{
			var serializerToDecorate = new JSONSerializer();
			_serializer = new SerializerWithLogging(serializerToDecorate, logger);
		}

		public string Serialize(object o)
		{
			return _serializer.Serialize(o);
		}

		public object Deserialize(Type objectType, string serializedString)
		{
			return _serializer.Deserialize(objectType, serializedString);
		}

		public T Deserialize<T>(string serializedString)
		{
			return _serializer.Deserialize<T>(serializedString);
		}
	}
}
