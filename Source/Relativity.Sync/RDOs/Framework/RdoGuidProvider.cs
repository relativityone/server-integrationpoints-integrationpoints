using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Relativity.Sync.RDOs.Framework.Attributes;

namespace Relativity.Sync.RDOs.Framework
{
    internal class RdoGuidProvider : IRdoGuidProvider
    {
        private readonly ConcurrentDictionary<Type, RdoTypeInfo> _cache =
            new ConcurrentDictionary<Type, RdoTypeInfo>();

        public RdoTypeInfo GetValue<TRdoType>()
            where TRdoType : IRdoType
        {
            Type rdoType = typeof(TRdoType);
            return _cache.GetOrAdd(rdoType, GetTypeGuids);
        }

        private static RdoTypeInfo GetTypeGuids(Type t)
        {
            RdoAttribute rdoAttribute = t.GetCustomAttribute<RdoAttribute>();
            return new RdoTypeInfo()
            {
                TypeGuid = rdoAttribute.TypeGuid,
                Name = rdoAttribute.Name,
                ParentTypeGuid = rdoAttribute.ParentObjectTypeGuid,
                Fields = t.GetProperties()
                    .Where(x => x.Name != nameof(IRdoType.ArtifactId))
                    .ToDictionary(x => x.GetCustomAttribute<RdoFieldAttribute>().FieldGuid,
                        x =>
                        {
                            RdoFieldAttribute fieldAttribute = x.GetCustomAttribute<RdoFieldAttribute>();
                            return new RdoFieldInfo
                            {
                                Name = x.Name,
                                Guid = fieldAttribute.FieldGuid,
                                Type = fieldAttribute.FieldType,
                                IsRequired = fieldAttribute.Required,
                                TextLength = fieldAttribute.FixedTextLength,
                                PropertyInfo = x
                            };
                        })
            };
        }
    }
}