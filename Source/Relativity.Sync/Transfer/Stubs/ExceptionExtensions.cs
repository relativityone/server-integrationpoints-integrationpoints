using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.AntiMalware.SDK
{
	public static class ExceptionExtensions
	{
		public static bool ContainsAntiMalwareEvent(this Exception ex)
		{
			return false;
		}
	}
}
