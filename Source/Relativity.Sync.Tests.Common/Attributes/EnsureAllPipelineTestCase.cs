using System;

namespace Relativity.Sync.Tests.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EnsureAllPipelineTestCase : Attribute
    {
        public int PipelineArgumentIndex { get; }

        public EnsureAllPipelineTestCase(int pipelineArgumentIndex)
        {
            PipelineArgumentIndex = pipelineArgumentIndex;
        }
    }
}
