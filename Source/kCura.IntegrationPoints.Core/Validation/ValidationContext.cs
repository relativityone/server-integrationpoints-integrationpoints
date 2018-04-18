
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Validation
{
	public class ValidationContext
	{
		public IntegrationPointModelBase Model;
		public SourceProvider SourceProvider;
		public DestinationProvider DestinationProvider;
		public IntegrationPointType IntegrationPointType;
		public string ObjectTypeGuid;
		public int UserId;
	}
}
