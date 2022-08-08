using System.Collections.Generic;

namespace kCura.IntegrationPoints.Domain.Models
{
    public class JsTreeItemDTO : JsTreeItemBaseDTO
    {
        public JsTreeItemDTO()
        {
            Children = new List<JsTreeItemDTO>();
        }

        public List<JsTreeItemDTO> Children { get; set; }

    }
}