using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class NotificationStepTests : FailingStepsBase<INotificationConfiguration>
	{
		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			// nothing special to assert
		}

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// all except notification
			const int expectedNumberOfExecutedSteps = 13;
			return expectedNumberOfExecutedSteps;
		}
	}
}