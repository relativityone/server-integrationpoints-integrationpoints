using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;

namespace kCura.IntegrationPoints.Core
{
	public class IntegrationPointSerializer : IIntegrationPointSerializer
	{
		private readonly ISerializer _serializer = new JSONSerializer();

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
