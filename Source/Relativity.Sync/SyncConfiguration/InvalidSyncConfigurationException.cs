using System;

namespace Relativity.Sync.SyncConfiguration
{
	/// <inheritdoc />
	[Serializable]
	public sealed class InvalidSyncConfigurationException : Exception
	{
		/// <inheritdoc />
		public InvalidSyncConfigurationException()
		{
		}

		/// <inheritdoc />
		public InvalidSyncConfigurationException(string message) : base(message)
		{
		}

		/// <inheritdoc />
		public InvalidSyncConfigurationException(string message, Exception inner) : base(message, inner)
		{
		}
	}
}
