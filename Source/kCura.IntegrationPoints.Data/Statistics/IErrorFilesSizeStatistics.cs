﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Statistics
{
	public interface IErrorFilesSizeStatistics
	{
		int ForJobHistoryOmmitedFiles(int workspaceArtifactId, int artifactId);
	}
}
