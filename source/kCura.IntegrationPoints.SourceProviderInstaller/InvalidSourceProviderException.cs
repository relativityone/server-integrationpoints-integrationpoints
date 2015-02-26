﻿using System;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
    /// <summary>
    /// This exception is thrown when an error occurs during the installation of a data source provider.
    /// </summary>
	
    [Serializable]
	public class InvalidSourceProviderException : Exception
	{
		/// <summary>
        /// Initializes a new instance of the InvalidSourceProviderException() class.
		/// </summary>
        public InvalidSourceProviderException()
		{

		}
		/// <summary>
        /// Initializes a new instance of the InvalidSourceProviderException() class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
        public InvalidSourceProviderException(string message)
			: base(message)
		{
		}
		/// <summary>
        /// Initializes a new instance of the InvalidSourceProviderException() class with a specified error message and a reference to an inner exception that caused this exception.
		/// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that caused the current exception, or a null reference if no inner exception is specified. </param>
        public InvalidSourceProviderException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
