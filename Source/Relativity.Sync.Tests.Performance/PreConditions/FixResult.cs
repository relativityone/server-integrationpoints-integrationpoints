using System;
using System.Runtime.CompilerServices;

namespace Relativity.Sync.Tests.Performance.PreConditions
{
    internal class FixResult
    {
        public string PreConditionName { get; set; }
        public bool IsFixed { get; set; }
        public Exception Exception;

        public FixResult(string name)
        {
            PreConditionName = name;
        }

        public static FixResult Fixed([CallerMemberName] string name = "") => new FixResult(name) {IsFixed = true};

        public static FixResult Error(Exception exception, [CallerMemberName] string name = "") => 
            new FixResult(name) { IsFixed = false, Exception = exception};
    }
}
