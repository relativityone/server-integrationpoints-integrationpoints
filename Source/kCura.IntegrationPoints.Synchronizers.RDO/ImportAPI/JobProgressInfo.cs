namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
	internal class JobProgressInfo
	{
		public int NumberOfItemsTransferred { get; private set; }
		public int NumberOfItemsErrored { get; private set; }

		public void Reset()
		{
			NumberOfItemsErrored = 0;
			NumberOfItemsTransferred = 0;
		}

		public void ItemErrored()
		{
			NumberOfItemsErrored++;
			NumberOfItemsTransferred--;
		}

		public void ItemTransferred()
		{
			NumberOfItemsTransferred++;
		}

		public bool IsValid()
		{
			return NumberOfItemsTransferred >= 0 && NumberOfItemsErrored >= 0;
		}
	}
}