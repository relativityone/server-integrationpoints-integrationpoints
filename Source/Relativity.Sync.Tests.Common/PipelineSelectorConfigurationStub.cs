using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Common
{
	public class PipelineSelectorConfigurationStub : IPipelineSelectorConfiguration
	{
		public void Dispose()
		{
		}

		public int? JobHistoryToRetryId { get; internal set; }
	}
}
