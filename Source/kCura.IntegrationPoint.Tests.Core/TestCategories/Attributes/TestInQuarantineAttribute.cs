using System;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes
{
	public class TestInQuarantineAttribute : TestCaseAttribute
	{
		private readonly Regex _jiraIssueRegex = new Regex("^.*REL-([0-9]){6}.*$");

		public TestInQuarantineAttribute(string reason = null)
			: this(TestQuarantineState.Observation, reason)
		{
		}

		public TestInQuarantineAttribute(TestQuarantineState state, string reason)
		{
			Validate(state, reason);
			Category = TestCategories.IN_QUARANTINE;
		}

		private void Validate(TestQuarantineState state, string reason)
		{
			if (string.IsNullOrEmpty(reason))
			{
				throw new ArgumentException("Putting test into Quarantine requires a reason!");
			}

			if (state == TestQuarantineState.Observation)
			{
				return;
			}

			if (!_jiraIssueRegex.IsMatch(reason))
			{
				throw new ArgumentException("The reason for test in Quarantine must include related JIRA issue for fixing it!");
			}
		}
	}
}
