using System;

namespace Relativity.Sync
{
	internal sealed class WorkspaceGuid
	{
		public WorkspaceGuid(Guid value)
		{
			Value = value;
		}

		public Guid Value { get; }

		public override string ToString()
		{
			return Value.ToString();
		}
	}
}