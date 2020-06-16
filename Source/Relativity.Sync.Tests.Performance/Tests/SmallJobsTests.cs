﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Performance.Helpers;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Runner;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Performance.Tests
{
	[TestFixture]
	public class SmallJobsTests : PerformanceTestBase
	{
		public SmallJobsTests() : base("1066387_Small_jobs_tests_20200603140140.zip")
		{
		}

		public static IEnumerable<TestCaseData> Cases()
		{
#pragma warning disable RG2009 // Hardcoded Numeric Value

			PerformanceTestCase[] testCases = new[]
			{
				new PerformanceTestCase
				{
					TestCaseName = "Small-1a",
					ExpectedItemsTransferred = 10,
					NumberOfMappedFields = 150
				},
				new PerformanceTestCase
				{
					TestCaseName = "Small-1b",
					ExpectedItemsTransferred = 200,
					NumberOfMappedFields = 85,
				},
				new PerformanceTestCase
				{
					TestCaseName = "Small-1c",
					ExpectedItemsTransferred = 100,
					NumberOfMappedFields = 190,
				},
				new PerformanceTestCase
				{
					TestCaseName = "Small-1d",
					ExpectedItemsTransferred = 200,
					NumberOfMappedFields = 7,
				},
				new PerformanceTestCase
				{
					TestCaseName = "Small-2a",
					ExpectedItemsTransferred = 10,
					NumberOfMappedFields = 150,
				},
				new PerformanceTestCase
				{
					TestCaseName = "Small-2b",
					ExpectedItemsTransferred = 200,
					NumberOfMappedFields = 85,
				},
				new PerformanceTestCase
				{
					TestCaseName = "Small-2c",
					ExpectedItemsTransferred = 100,
					NumberOfMappedFields = 190,
				},
				new PerformanceTestCase
				{
					TestCaseName = "Small-2d",
					ExpectedItemsTransferred = 200,
					NumberOfMappedFields = 7,
				},
				new PerformanceTestCase
				{
					TestCaseName = "Small-3a",
					ExpectedItemsTransferred = 20,
					NumberOfMappedFields = 300,
				},
				new PerformanceTestCase
				{
					TestCaseName = "Small-3b",
					ExpectedItemsTransferred = 200,
					NumberOfMappedFields = 85,
				},
				new PerformanceTestCase
				{
					TestCaseName = "Small-3c",
					ExpectedItemsTransferred = 100,
					NumberOfMappedFields = 190,
				},
				new PerformanceTestCase
				{
					TestCaseName = "Small-3d",
					ExpectedItemsTransferred = 200,
					NumberOfMappedFields = 7,
				},
			};

			return testCases.Select(x => new TestCaseData(x)
			{
				TestName = $"{x.TestCaseName} ({x.ExpectedItemsTransferred} docs, {x.NumberOfMappedFields} fields)"
			});

#pragma warning restore RG2009 // Hardcoded Numeric Value
		}

		[TestCaseSource(nameof(Cases))]
		public async Task RunJob(PerformanceTestCase testCase)
		{
			await RunTestCase(testCase).ConfigureAwait(false);
		}
	}
}
