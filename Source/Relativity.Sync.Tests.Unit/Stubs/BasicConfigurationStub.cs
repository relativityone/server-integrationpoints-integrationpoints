using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Stubs
{
    internal class BasicConfigurationStub : IConfiguration
    {
        private readonly SyncConfigurationRdo _rdo;
        private static readonly RdoGuidProvider _rdoGuidProvider = new RdoGuidProvider();
        private static readonly RdoTypeInfo _rdoFieldsInfo = _rdoGuidProvider.GetValue<SyncConfigurationRdo>();

        public BasicConfigurationStub(SyncConfigurationRdo rdo)
        {
            _rdo = rdo;
        }

        public void Dispose()
        {
        }

        public T GetFieldValue<T>(Func<SyncConfigurationRdo, T> valueGetter)
        {
            return valueGetter(_rdo);
        }

        public Task UpdateFieldValueAsync<T>(Expression<Func<SyncConfigurationRdo, T>> memberExpression, T value)
        {
            Guid fieldGuid = _rdoGuidProvider.GetGuidFromFieldExpression(memberExpression);
            RdoFieldInfo fieldInfo = _rdoFieldsInfo.Fields[fieldGuid];

            fieldInfo.PropertyInfo.SetValue(_rdo, value);

            return Task.CompletedTask;
        }
    }
}