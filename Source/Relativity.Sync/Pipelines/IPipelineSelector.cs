using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using IConfiguration = Relativity.Sync.Storage.IConfiguration;

namespace Relativity.Sync.Pipelines
{
	internal interface IPipelineSelector
	{
		ISyncPipeline GetPipeline();
	}
}
