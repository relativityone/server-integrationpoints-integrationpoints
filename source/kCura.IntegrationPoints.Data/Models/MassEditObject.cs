namespace kCura.IntegrationPoints.Data.Models
{
	public class MassEditObject
	{
		/// <summary>
		/// The GUID of the field we are editing
		/// </summary>
		public string FieldGuid { get; set; }
		
		/// <summary>
		/// The field we are editing
		/// </summary>
		public global::Relativity.Core.DTO.Field FieldToUpdate { get; set; }
		
		/// <summary>
		/// The Artifact ID of the RDO instance we are linking to
		/// </summary>
		public int ObjectToLinkTo { get; set; }
	}
}
