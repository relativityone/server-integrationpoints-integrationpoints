using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal interface IConfiguration : IDisposable
	{
		T GetFieldValue<T>(Expression<Func<SyncConfigurationRdo, T>> memberExpression);
		Task UpdateFieldValueAsync<T>(Expression<Func<SyncConfigurationRdo, T>> memberExpression, T value);
	}
}