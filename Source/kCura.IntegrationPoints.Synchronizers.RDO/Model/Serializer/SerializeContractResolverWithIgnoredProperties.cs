using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Model.Serializer
{
    internal class SerializeContractResolverWithIgnoredProperties<T> : DefaultContractResolver
    {
        private readonly ISet<string> _propertiesToIgnore;

        public SerializeContractResolverWithIgnoredProperties(ISet<string> propertiesToIgnore)
        {
            _propertiesToIgnore = propertiesToIgnore;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(T) && _propertiesToIgnore.Contains(property.PropertyName))
            {
                property.ShouldSerialize = instance => false;
            }

            return property;
        }
    }
}