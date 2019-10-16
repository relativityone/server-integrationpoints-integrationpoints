using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class NotificationStepTests : FailingStepsBase<INotificationConfiguration>
	{
		protected override bool ShouldStopExecution { get; } = false;

		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			// nothing special to assert
		}

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// validation, permissions, object types, snapshot,
			// source tags, dest tags, data destination init, sum reporting,
			// saved search, snapshot partition, sync, data destination finalization,
			// job status consolidation, job cleanup
			const int expectedNumberOfExecutedSteps = 14;
			return expectedNumberOfExecutedSteps;
		}
	}
}