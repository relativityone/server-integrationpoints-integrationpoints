﻿using System;
using System.Runtime.Serialization;

namespace kCura.IntegrationPoints.Contracts.Provider
{
	/// <summary>
	/// Represents an error thrown by an IDataReader when a read failure occurs.
	/// </summary>
	/// <remarks>
	/// If an IFieldProvider object is attempting to retrieve data when the ProviderReadDataException is thrown, Relativity treats this error as an item-level error. Relativity creates an item-level Job History Error for the current Job History and the import job continues running. 
	/// If this exception isn’t thrown when an error occurs, then Relativity treats it as a job-level error. It creates a job-level Job History Error for the current Job History and the job stops. 
	/// </remarks>

	[Serializable]
	public class ProviderReadDataException : Exception
	{
		private string _identifier;

		/// <summary>
		/// Gets or sets the item identifier.
		/// <remarks>The identfier of the data that the Data Source Provider excepts on when reading.</remarks>
		/// </summary>
		public String Identifier
		{
			set
			{
				_identifier = value;
			}
			get
			{
				return _identifier ?? String.Empty;
			}
		}

		public ProviderReadDataException() : base()
		{
		}

		public ProviderReadDataException(string message) : base(message)
		{
		}

		public ProviderReadDataException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public ProviderReadDataException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
