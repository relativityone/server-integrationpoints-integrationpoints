using Relativity.Sync.Storage;
using System.Collections.Generic;

namespace Relativity.Sync
{
	internal interface IJobProgressUpdaterFactory
	{
		IJobProgressUpdater CreateJobProgressUpdater();
	}
}