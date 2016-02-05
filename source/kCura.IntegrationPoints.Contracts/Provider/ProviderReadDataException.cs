﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
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
