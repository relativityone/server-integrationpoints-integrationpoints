using System.Collections.Generic;

namespace kCura.IntegrationPoints.Domain.Models
{
	public class TreeItemDTO
	{
		public TreeItemDTO()
		{
			Children = new List<TreeItemDTO>();
		}

		public string Text { get; set; }
		public string Id { get; set; }
		public List<TreeItemDTO> Children { get; }
        public bool isDirectory { get; set; }
	}
}