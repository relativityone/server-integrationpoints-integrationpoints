using System;
using System.Runtime.Serialization;

namespace kCura.IntegrationPoints.Contracts.Provider
{
	/// <summary>
	/// Represents exception used to identify error during read operation from IDataReader that is returned from IFieldProvider.
	/// If this throw when IFieldProvider object trying to get data. We will treats such error as item-level error.
	/// </summary>
	[Serializable]
	public class ProviderReadDataException : Exception
	{
		private string _identifier;

		/// <summary>
		/// Gets or sets the item identifier.
		/// <remarks>This idenfier will be the identfier of the data that the Data Source Provider excepts on when reading.</remarks>
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
