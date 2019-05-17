﻿using System;
using System.Runtime.Serialization;

namespace Relativity.Sync.Executors
{
	/// <summary>
	///     Exception thrown by methods of <see cref="IDestinationWorkspaceTagRepository" />
	///     when errors occur in external services.
	/// </summary>
	[Serializable]
	public sealed class KeplerServiceException : Exception
	{
		/// <inheritdoc />
		public KeplerServiceException()
		{
		}

		/// <inheritdoc />
		public KeplerServiceException(string message) : base(message)
		{
		}

		/// <inheritdoc />
		public KeplerServiceException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <inheritdoc />
		private KeplerServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}