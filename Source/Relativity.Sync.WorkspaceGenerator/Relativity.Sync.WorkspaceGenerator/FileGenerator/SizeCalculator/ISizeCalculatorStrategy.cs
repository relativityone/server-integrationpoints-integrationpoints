﻿using System.Collections.Generic;

namespace Relativity.Sync.WorkspaceGenerator.FileGenerator.SizeCalculator
{
	public interface IFileSizeCalculatorStrategy
	{
		IEnumerable<long> GetSizesInBytes(int count, long totalSizeInMB);
	}
}