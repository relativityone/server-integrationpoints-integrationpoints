using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core
{
	public interface IIntegrationPointSerializer
	{
		string Serialize(object o);

		object Deserialize(Type objectType, string serializedString);

		T Deserialize<T>(string serializedString);
	}
}
