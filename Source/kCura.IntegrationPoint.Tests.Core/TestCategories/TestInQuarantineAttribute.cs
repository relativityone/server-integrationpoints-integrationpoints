using System;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.TestCategories
{
	public class TestInQuarantineAttribute : CategoryAttribute
	{
		private readonly Regex _jiraIssueRegex = new Regex("^.*REL-([0-9]){6}.*$");

		public TestInQuarantineAttribute(string reason) : base(TestCategories.IN_QUARANTINE)
		{
			ValidateReason(reason);
		}

		private void ValidateReason(string reason)
		{
			if (string.IsNullOrEmpty(reason))
			{
				throw new ArgumentException("Putting test into Quarantine requires a reason!");
			}

			if (!_jiraIssueRegex.IsMatch(reason))
			{
				throw new ArgumentException("The reason for test in Quarantine must include related JIRA issue for fixing it!");
			}
		}
	}
}
