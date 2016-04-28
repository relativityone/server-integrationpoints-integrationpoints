using System;
using System.Linq;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Data
{
	public class RdoHelper
	{
		public static void CheckResult<TResult>(ResultSet<TResult> result) where TResult : kCura.Relativity.Client.DTOs.Artifact
		{
			if (!result.Success)
			{
				var messages = result.Results.Where(x => !x.Success).Select(x => x.Message);
				var e = new AggregateException(result.Message, messages.Select(x => new Exception(x)));
				throw e;
			}
		}

		public static void CheckObjectQueryResultSet(ObjectQueryResultSet result)
		{
			if (!result.Success)
			{
				throw new Exception(result.Message);
			}
		}
	}
}
