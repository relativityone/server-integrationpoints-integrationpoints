using System;

namespace Relativity.Sync.Tests.Performance.PreConditions
{
	internal class FixResult
	{
		public string PreConditionName { get; set; }
		public bool IsFixed { get; set; }
		public Exception Exception;
	}
}
