using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class DataSourceSnapshotStepTests : FailingStepsBase<IDataSourceSnapshotConfiguration>
	{
		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			// nothing special to assert
		}

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// validation, permissions, object types, notification
			const int expectedNumberOfExecutedSteps = 4;
			return expectedNumberOfExecutedSteps;
		}
	}
}