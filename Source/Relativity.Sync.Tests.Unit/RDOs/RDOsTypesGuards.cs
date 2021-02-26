using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.RDOs.Framework.Attributes;

namespace Relativity.Sync.Tests.Unit.RDOs
{
    [TestFixture]
    public class RDOsTypesGuards
    {
        [Test]
        public void AllRdos_ShouldBeSealedClasses()
        {
            Type[] allRdoTypes = GetRdoTypesFromAssembly(typeof(IRdoType).Assembly);

            allRdoTypes.All(x => x.IsSealed && !x.IsAbstract && x.IsClass).Should()
                .BeTrue("RDOs should be simple DTOs");
        }

        [Test]
        public void AllRdos_ShouldBeMarkedWithRdoAttribute()
        {
            Type[] allRdoTypes = GetRdoTypesFromAssembly(typeof(IRdoType).Assembly);

            allRdoTypes.ForEach(x =>
            {
                try
                {
                    x.GetCustomAttribute<RdoAttribute>().Should()
                        .NotBeNull($"{x.Name} is not marked with {nameof(RdoAttribute)}");
                }
                catch (FormatException)
                {
                    Assert.Fail($"Incorrect value for {nameof(RdoAttribute)} on type {x.FullName}");
                }
            });
        }

        [Test]
        public void AllRdosProperties_ShouldBeMarkedWithRdoFieldAttribute()
        {
            Type[] allRdoTypes = GetRdoTypesFromAssembly(typeof(IRdoType).Assembly);

            allRdoTypes
                .ForEach(t =>
                {
                    t.GetProperties()
                        .Where(p => p.Name != nameof(IRdoType.ArtifactId))
                        .ForEach(x =>
                        {
                            try
                            {
                                x.GetCustomAttribute<RdoFieldAttribute>().Should()
                                    .NotBeNull($"{t.Name}.{x.Name} is not marked with RdoFieldAttribute");
                            }
                            catch (FormatException)
                            {
                                Assert.Fail(
                                    $"Incorrect value for {nameof(RdoFieldAttribute)} on  {t.FullName}.{x.Name}");
                            }
                        });
                });
        }

        [Test]
        public void AllRdosGuidProperties_ShouldBeOfTypeFixedLengthText_WithAtLeast36Characters()
        {
            Type[] allRdoTypes = GetRdoTypesFromAssembly(typeof(IRdoType).Assembly);

            const int guidTextLength = 36;
            
            allRdoTypes
                .ForEach(t =>
                {
                    t.GetProperties()
                        .Where(p => p.Name != nameof(IRdoType.ArtifactId) &&
                                    (p.PropertyType == typeof(Guid) || p.PropertyType == typeof(Guid?)))
                        .ForEach(x =>
                        {
                            var fieldAttribute = x.GetCustomAttribute<RdoFieldAttribute>();

                            fieldAttribute.FieldType.Should().Be(RdoFieldType.FixedLengthText);
                            fieldAttribute.FixedTextLength.Should().BeGreaterOrEqualTo(guidTextLength);
                        });
                });
        }

        private static Type[] GetRdoTypesFromAssembly(Assembly assembly)
        {
            Type iRdoType = typeof(IRdoType);

            return assembly.GetTypes()
                .Where(x => iRdoType.IsAssignableFrom(x) && x != iRdoType)
                .ToArray();
        }
    }
}