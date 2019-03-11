using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Tests.Integration.Stubs;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class SyncJobFactoryTests
	{
		private SyncJobFactory _instance;

		[SetUp]
		public void SetUp()
		{
			_instance = new SyncJobFactory();
		}

		[Test]
		public void ItShouldCreateSyncJob()
		{
			IContainer container = IntegrationTestsContainerBuilder.CreateContainer();
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1);
			List<IInstaller> installers = Assembly.GetAssembly(typeof(IInstaller))
				.GetTypes()
				.Where(t => !t.IsAbstract && t.IsAssignableTo<IInstaller>())
				.Select(t => (IInstaller)Activator.CreateInstance(t))
				.ToList();
			installers.Add(new OutsideDependenciesStubInstaller());

			// ACT
			ISyncJob job = _instance.Create(container, installers, syncJobParameters);

			// ASSERT
			job.Should().NotBeNull();
		}
	}
}