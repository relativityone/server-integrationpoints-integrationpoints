using System;
using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
	internal interface IConfigurationCache : IDisposable
	{
		T GetFieldValue<T>(Guid guid);
		Task UpdateFieldValueAsync<T>(Guid guid, T value);
	}
}