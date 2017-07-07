using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Management.Monitoring;
using Relativity.API;

namespace kCura.IntegrationPoints.Management
{
	public class IntegrationPointsManager : IIntegrationPointsManager
	{
		private readonly IAPILog _logger;
		private readonly IEnumerable<IManagerTask> _monitoring;

		public IntegrationPointsManager(IAPILog logger, IEnumerable<IManagerTask> monitoring)
		{
			_monitoring = monitoring;
			_logger = logger;
		}

		public void Start()
		{
			foreach (var monitoring in _monitoring)
			{
				try
				{
					monitoring.Run();
				}
				catch (Exception e)
				{
					_logger.LogError(e, "Failed to execute monitoring {type}", monitoring.GetType());
				}
			}
		}
	}
}