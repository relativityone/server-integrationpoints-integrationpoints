using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions.Common;
using NUnit.Framework;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Tests.Common.Attributes;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
    [TestFixture]
    public class AllPipelineTypesGuard
    {
        [Test]
        public void Enforce_EnsureAllPipelineTestCase_Attribute()
        {
            Type attributeType = typeof(EnsureAllPipelineTestCase);

            TypeInfo[] allPipelineTypes = GetAllPipelineTypes();

            IEnumerable<MethodInfo> testsWithAttribute = typeof(AllPipelineTypesGuard).Assembly.DefinedTypes.SelectMany(x => x.DeclaredMethods)
                .Where(m => m.CustomAttributes.Any(a => a.AttributeType == attributeType));

            Dictionary<MethodInfo, TypeInfo[]> missingPipelinesForMethods = new Dictionary<MethodInfo, TypeInfo[]>();
            
            testsWithAttribute.ForEach(t =>
            {
                int pipelineArgumentIndex =
                    ((EnsureAllPipelineTestCase)Attribute.GetCustomAttributes(t)
                        .First(x => x.GetType() == attributeType))
                    .PipelineArgumentIndex;

                Type[] pipelineTestCases = Attribute.GetCustomAttributes(t).OfType<TestCaseAttribute>()
                    .Select(x => (Type)x.Arguments[pipelineArgumentIndex]).ToArray();

                TypeInfo[] missingPipelines = allPipelineTypes.Where(p => !pipelineTestCases.Contains(p)).ToArray();

                if (missingPipelines.Length > 0)
                {
                    missingPipelinesForMethods.Add(t, missingPipelines);
                }
            });

            if (missingPipelinesForMethods.Any())
            {
                Assert.Fail($"There are missing pipeline test cases for some tests: {Environment.NewLine} [{Environment.NewLine}{{0}}{Environment.NewLine}]", string.Join("," + Environment.NewLine, missingPipelinesForMethods.Select(GetMissingPipelineDescription)));
            }
        }

        private string GetMissingPipelineDescription(KeyValuePair<MethodInfo, TypeInfo[]> arg)
        {
            return
                $"({arg.Key.DeclaringType.FullName}.{arg.Key.Name} => {{ {string.Join(", ", arg.Value.Select(p => p.FullName))} }} )";
        }

        private TypeInfo[] GetAllPipelineTypes()
        {
            return typeof(ISyncPipeline).Assembly.DefinedTypes
                .Where(x => x.Implements(typeof(ISyncPipeline)) && !x.IsAbstract && x.IsClass).ToArray();
        }
    }
}
