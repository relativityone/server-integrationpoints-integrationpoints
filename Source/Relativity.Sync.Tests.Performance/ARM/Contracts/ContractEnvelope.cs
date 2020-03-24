namespace Relativity.Sync.Tests.Performance.ARM.Contracts
{
	public class ContractEnvelope<T> where T: class
	{
		public T Contract { get; set; }
	}
}
