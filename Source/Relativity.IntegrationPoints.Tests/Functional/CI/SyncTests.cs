using Atata;
using System;
using System.Collections.Generic;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Web.Models;
using Relativity.Testing.Framework.Web.Navigation;
using Relativity.Testing.Framework.Web.Extensions;
using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using FluentAssertions;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[IdentifiedTestFixture("74f17f40-697f-42b7-bad5-f83b8eeaa86a", Description = "RIP SYNC CI GOLD FLOWS")]
	[TestType.UI, TestType.MainFlow]
	public class SyncTests : TestsBase
	{
		private readonly SyncTestsImplementation _testsImplementation;

		public SyncTests()
			: base(nameof(SyncTests))
		{
			_testsImplementation = new SyncTestsImplementation(this);
		}

		protected override void OnSetUpFixture()
		{
			base.OnSetUpFixture();

			_testsImplementation.OnSetUpFixture();
		}

		protected override void OnTearDownFixture()
		{
			base.OnTearDownFixture();

			_testsImplementation.OnTearDownFixture();
		}

		[IdentifiedTest("b0afe8eb-e898-4763-9f95-e998f220b421")]
		public void SavedSearch_NativesAndMetadata_GoldFlow()
		{
			_testsImplementation.SavedSearchNativesAndMetadataGoldFlow();
		}
	}
}
