using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public class RDOHelper
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
	}
}
