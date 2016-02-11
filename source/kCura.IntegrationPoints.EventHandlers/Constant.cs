namespace kCura.IntegrationPoints.EventHandlers
{
	public class Constant
	{
		// ReSharper disable InconsistentNaming
		public const int EDDS_WORKSPACE_ID = -1;
		public const string WEB_GUID = Core.Application.GUID;
		public const string CUSTOMPAGE_TEMPLATE = @"%applicationpath%/CustomPages/";
		public const string URL_FOR_WEB = CUSTOMPAGE_TEMPLATE + WEB_GUID;
		public const string URL_FOR_EXTERNAL = @"/Relativity/External.aspx?AppID={0}&ArtifactID={1}&DirectTo={2}";
		
		public const string URL_FOR_INTEGRATIONPOINTSCONTROLLER = "IntegrationPoints";

		public const string URL_FOR_INTEGRATIONPOINTS_EDIT = "Edit";
		public const string URL_FOR_INTEGRATIONPOINTS_VIEW = "Details";
		// ReSharper restore InconsistentNaming
	}
}
