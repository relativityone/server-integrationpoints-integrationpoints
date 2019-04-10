using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync
{
	internal sealed class SafeProgressWrapper<T> : IProgress<T>
	{
		private readonly IProgress<T> _progress;
		private readonly ISyncLog _logger;

		public SafeProgressWrapper(IProgress<T> progress, ISyncLog logger)
		{
			_progress = progress;
			_logger = logger;
		}

		public void Report(T value)
		{
			try
			{
				_progress.Report(value);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, 
					$"Received error when invoking externally-provided implementation of {nameof(IProgress<T>.Report)} ({{type}})",
					_progress.GetType().FullName);
			}
		}
	}
}
