using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.SyncConfiguration;

namespace Relativity.Sync.Tests.Unit.Types
{
    [TestFixture]
    public class ServiceFactoryTypesTests
    {
        private List<Type> _whiteListTypes;

        [SetUp]
        public void SetUp()
        {
            _whiteListTypes = new List<Type>
            {
                typeof(ServiceFactoryBase),
                typeof(ServiceFactoryForAdmin),
                typeof(ServiceFactoryForUser),
                typeof(RelativityServices),
                typeof(SyncConfigurationBuilder),
                typeof(SyncJobConfigurationBuilder),
            };
        }

        [Test]
        public void RelativitySync_TypesShouldNotUseISyncServiceManagerUnlessTheyAreWhitelisted()
        {
            // Arrange
            AssemblyName[] assemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            AssemblyName syncAssemblyName = assemblies.Single(x => x.Name == "Relativity.Sync");
            Assembly syncAssembly = Assembly.Load(syncAssemblyName.FullName);
            Type[] syncTypes = syncAssembly.GetTypes();
            
            // Act && Assert
            foreach (Type syncType in syncTypes)
            {
                if (_whiteListTypes.Contains(syncType))
                {
                    continue;
                }

                ConstructorInfo[] constructors = syncType.GetConstructors();

                foreach (ConstructorInfo constructor in constructors)
                {
                    var parameters = constructor.GetParameters();
                    foreach (var parameter in parameters)
                    {
                        parameter.ParameterType.Should().NotBe(typeof(ISyncServiceManager), 
                        $"Class {syncType} should not use {nameof(ISyncServiceManager)} type. Use {nameof(ISourceServiceFactoryForAdmin)} or {nameof(ISourceServiceFactoryForUser)} instead.");
                    }
                }
            }
        }
    }
}
