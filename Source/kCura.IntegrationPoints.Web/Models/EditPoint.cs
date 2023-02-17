
namespace kCura.IntegrationPoints.Web.Models
{
    public class EditPoint
    {
        public int AppID { get; set; }

        public int ArtifactID { get; set; }

        public int UserID { get; set; }

        public int CaseUserID { get; set; }

        public string URL { get; set; }

        public string APIControllerName { get; set; }

        public string ArtifactTypeName { get; set; }
    }
}
