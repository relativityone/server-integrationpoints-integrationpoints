using System;

namespace Relativity.Sync.Telemetry.RelEye
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class RelEyeAttribute : Attribute
    {
        public string Name { get; }

        public RelEyeAttribute(string name)
        {
            Name = name;
        }
    }
}
