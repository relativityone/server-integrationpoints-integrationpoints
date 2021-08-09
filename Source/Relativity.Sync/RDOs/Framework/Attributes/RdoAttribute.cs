using System;

namespace Relativity.Sync.RDOs.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class RdoAttribute : Attribute
    {
        public Guid TypeGuid { get; }
        public string Name { get; }
        public Guid? ParentObjectTypeGuid { get; }

        /// <summary>
        /// Attribute describing how to create RDO
        /// </summary>
        /// <param name="typeGuid">GUID of the type</param>
        /// <param name="name">Name of the type</param>
        /// <param name="parentObjectTypeGuid">GUID of parent object type. If null, type is parented by workspace</param>
        public RdoAttribute(string typeGuid, string name, string parentObjectTypeGuid = null)
        {
            TypeGuid = Guid.Parse(typeGuid);
            Name = name;

            if (!string.IsNullOrEmpty(parentObjectTypeGuid))
            {
                ParentObjectTypeGuid = Guid.Parse(parentObjectTypeGuid);
            }
        }
    }
}