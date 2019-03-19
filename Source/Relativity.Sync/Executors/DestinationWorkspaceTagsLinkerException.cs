using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	/// <inheritdoc/>
	[Serializable]
	public sealed class DestinationWorkspaceTagsLinkerException : Exception
	{
		/// <inheritdoc/>
		public DestinationWorkspaceTagsLinkerException() : base()
		{
		}

		/// <inheritdoc/>
		public DestinationWorkspaceTagsLinkerException(string message) : base(message)
		{
		}

		/// <inheritdoc/>
		public DestinationWorkspaceTagsLinkerException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		/// <inheritdoc/>
		private DestinationWorkspaceTagsLinkerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
