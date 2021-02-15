using System;

namespace Relativity.Sync.RDOs.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class RdoAttribute : Attribute
    {
        public Guid TypeGuid { get; }
        public string Name { get; }
        public Guid? ParentObjectTypeGuid { get; }

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