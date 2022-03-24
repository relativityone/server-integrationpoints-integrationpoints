using System;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FileRepositoryException : Exception
	{
		public FileRepositoryException() { }
		public FileRepositoryException(string message, Exception innerException) : base(message, innerException) { }
		public FileRepositoryException(string message) : base(message) { }
	}
}