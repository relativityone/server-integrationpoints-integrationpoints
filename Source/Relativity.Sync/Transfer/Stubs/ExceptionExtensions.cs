using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.AntiMalware.SDK
{
	/// <summary>
	/// Stubbed the original interface and registered a NO-OP implementation into this project.This will not only reduce the number of changes but makes future backports easier.
	/// </summary>
	public static class ExceptionExtensions
	{
		public static bool ContainsAntiMalwareEvent(this Exception ex)
		{
			return false;
		}
	}
}
