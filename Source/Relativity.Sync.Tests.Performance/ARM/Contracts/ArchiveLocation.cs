﻿namespace Relativity.Sync.Tests.Performance.ARM.Contracts
{
	public class ArchiveLocation
	{
		public int ArchiveLocationType { get; } = 1; //Value representing a standard ARM Archive type
		public string Location { get; set; }
	}
}
