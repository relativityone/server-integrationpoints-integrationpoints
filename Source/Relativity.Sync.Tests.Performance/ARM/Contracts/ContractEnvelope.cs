using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Performance.ARM.Contracts
{
	public class ContractEnvelope<T> where T: class
	{
		public T Contract { get; set; }
	}
}
