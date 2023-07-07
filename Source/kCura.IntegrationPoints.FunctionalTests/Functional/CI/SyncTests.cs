﻿using NUnit.Framework;

using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
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

		[TestType.Critical]
		[IdentifiedTest("b0afe8eb-e898-4763-9f95-e998f220b421")]
		[Ignore("REL-841500: Resolve RIP functional test failures.")]
        public void SavedSearch_NativesAndMetadata_GoldFlow()
		{
			_testsImplementation.SavedSearchNativesAndMetadataGoldFlow();
		}

		[IdentifiedTest("26b72aab-a7ef-44ed-8338-81f91523388c")]
		public void Production_Images_GoldFlow()
		{
			_testsImplementation.ProductionImagesGoldFlow();
		}
	}
}
