using System;

namespace Relativity.Sync.Utils
{
	public interface ISerializer
	{
		string Serialize(object o);

		object Deserialize(Type objectType, string serializedString);

		T Deserialize<T>(string serializedString);
	}
}