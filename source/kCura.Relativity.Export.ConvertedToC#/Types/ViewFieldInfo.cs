using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using RelViewFieldInfo = Relativity.ViewFieldInfo;

namespace kCura.Relativity.Export.Types
{

	[Serializable()]

	public class ViewFieldInfo : RelViewFieldInfo, IComparable
	{

		public ViewFieldInfo(System.Data.DataRow row) : base(row)
		{
		}
		public ViewFieldInfo(RelViewFieldInfo vfi) : base(vfi)
		{
		}

		public override string ToString()
		{
			return this.DisplayName;
		}

		public int CompareTo(object obj)
		{
			return string.Compare(this.DisplayName, obj.ToString());
		}

		public new bool Equals(ViewFieldInfo other)
		{
			if (this.AvfId == other.AvfId && this.AvfColumnName == other.AvfColumnName)
				return true;
			return false;
		}

		public override int GetHashCode()
		{
			return 45 * this.AvfId;
		}
	}

}
