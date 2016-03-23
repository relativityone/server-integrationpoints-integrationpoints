using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Exports
{
	[Serializable()]
	public class VolumeInfo : System.Runtime.Serialization.ISerializable
	{

		#region " Member and Accessors "

		private string _subdirectoryImagePrefix;
		private string _subdirectoryNativePrefix;

		private string _subdirectoryTextPrefix;
		public string VolumePrefix { get; set; }

		public Int32 VolumeStartNumber { get; set; }

		public Int64 VolumeMaxSize { get; set; }

		public Int32 SubdirectoryStartNumber { get; set; }

		public Int64 SubdirectoryMaxSize { get; set; }

		public bool CopyFilesFromRepository { get; set; }

		public string GetSubdirectoryImagePrefix(bool includeTopFolder) {
			
				string result = _subdirectoryImagePrefix;
				if (includeTopFolder) {
					result = "IMAGES\\" + result;
				}
				return result;
			
			
		}

	    public string SubdirectoryImagePrefix
	    {
	        get { return _subdirectoryImagePrefix; }
	        set { _subdirectoryImagePrefix = value; }
        }


        public string GetSubdirectoryNativePrefix(bool includeTopFolder)
        {
          
                string result = _subdirectoryNativePrefix;
                if (includeTopFolder)
                {
                    result = "NATIVES\\" + result;
                }
                return result;
           
       
        }
        public string SubdirectoryNativePrefix {
            get { return _subdirectoryNativePrefix; }
            set { _subdirectoryNativePrefix = value; }
		}

        public string GetSubdirectoryFullTextPrefix(bool includeTopFolder)
        {
                string result = _subdirectoryTextPrefix;
                if (includeTopFolder)
                {
                    result = "TEXT\\" + result;
                }
                return result;
        }

        public string SubdirectoryFullTextPrefix {
            get { return _subdirectoryTextPrefix; }
            set { _subdirectoryTextPrefix = value; }
		}

		#endregion

		#region " Constructors "
		public VolumeInfo() : base()
		{
			this.CopyFilesFromRepository = true;
		}
		#endregion

		#region " Serialization "

		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("CopyFilesFromRepository", this.CopyFilesFromRepository, typeof(bool));
			info.AddValue("SubdirectoryMaxSize", this.SubdirectoryMaxSize, typeof(Int64));
			info.AddValue("SubdirectoryStartNumber", this.SubdirectoryStartNumber, typeof(Int32));
			info.AddValue("SubdirectoryFullTextPrefix", this.GetSubdirectoryFullTextPrefix(false), typeof(string));
			info.AddValue("SubdirectoryNativePrefix", this.GetSubdirectoryNativePrefix(false), typeof(string));
			info.AddValue("SubdirectoryImagePrefix", this.GetSubdirectoryImagePrefix(false), typeof(string));
			info.AddValue("VolumeMaxSize", this.VolumeMaxSize, typeof(Int64));
			info.AddValue("VolumeStartNumber", this.VolumeStartNumber, typeof(Int32));
			info.AddValue("VolumePrefix", this.VolumePrefix, typeof(string));
		}
		private VolumeInfo(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext Context)
		{
			var _with1 = info;
			this.CopyFilesFromRepository = _with1.GetBoolean("CopyFilesFromRepository");
			this.SubdirectoryMaxSize = _with1.GetInt64("SubdirectoryMaxSize");
			this.SubdirectoryStartNumber = _with1.GetInt32("SubdirectoryStartNumber");
			this.SubdirectoryFullTextPrefix = _with1.GetString("SubdirectoryFullTextPrefix");
			this.SubdirectoryNativePrefix = _with1.GetString("SubdirectoryNativePrefix");
			this.SubdirectoryImagePrefix = _with1.GetString("SubdirectoryImagePrefix");
			this.VolumeMaxSize = _with1.GetInt64("VolumeMaxSize");
			this.VolumeStartNumber = _with1.GetInt32("VolumeStartNumber");
			this.VolumePrefix = _with1.GetString("VolumePrefix");
		}
		#endregion

	}
}
