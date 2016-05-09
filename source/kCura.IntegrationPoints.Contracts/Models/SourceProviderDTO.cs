using System;

namespace kCura.IntegrationPoints.Contracts.Models
{
	/// <summary>
	/// DTO for the Source Provider RDO
	/// </summary>
	public class SourceProviderDTO : BaseDTO
	{
		public Guid Identifier { get; set; }
	}
}