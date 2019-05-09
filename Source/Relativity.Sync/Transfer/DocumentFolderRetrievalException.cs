using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	///     Exception thrown by methods of <see cref="IFolderPathRetriever" />
	///     when errors occur in external services.
	/// </summary>
	[Serializable]
	public sealed class DocumentFolderRetrievalException : Exception
	{
		/// <inheritdoc />
		public DocumentFolderRetrievalException()
		{
		}

		/// <inheritdoc />
		public DocumentFolderRetrievalException(string message) : base(message)
		{
		}

		/// <inheritdoc />
		public DocumentFolderRetrievalException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <inheritdoc />
		private DocumentFolderRetrievalException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
