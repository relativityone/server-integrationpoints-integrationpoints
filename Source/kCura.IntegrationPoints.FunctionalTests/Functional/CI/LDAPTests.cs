﻿using NUnit.Framework;

using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[TestType.UI, TestType.MainFlow]
	public class LDAPTests : TestsBase
	{
		private readonly ImportLDAPTestImplementation _testsImplementation;

		public LDAPTests()
			: base(nameof(LDAPTests))
		{
			_testsImplementation = new ImportLDAPTestImplementation(this);
		}

		protected override void OnSetUpFixture()
		{
			base.OnSetUpFixture();

			_testsImplementation.OnSetUpFixture();
		}

		[IdentifiedTest("09c54ba0-04d9-4f6e-9c46-0075612582fa")]
		[Ignore("REL-841500: Resolve RIP functional test failures.")]
        public void LoadFromLDAP_GoldFlow()
		{
			_testsImplementation.ImportFromLDAPGoldFlow();
		}
	}
}