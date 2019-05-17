using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.File.Models;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Tests.Integration
{
	internal static class Extensions
	{
		public static object ConvertTo<T>(this object value)
		{
			return ConvertTo(value, typeof(T));
		}

		public static object ConvertTo(this object value, Type target)
		{
			if (target == typeof(int))
			{
				return Convert.ToInt32(value, CultureInfo.InvariantCulture);
			}
			if (target == typeof(long))
			{
				return Convert.ToInt64(value, CultureInfo.InvariantCulture);
			}
			if (target == typeof(bool))
			{
				return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
			}
			if (target == typeof(string))
			{
				return Convert.ToString(value, CultureInfo.InvariantCulture);
			}

			throw new ArgumentException($"Method does not know how to convert to type {target}");
		}

		public static FileResponse ToFileResponse(this Document document)
		{
			return new FileResponse
			{
				DocumentArtifactID = document.ArtifactId,
				Filename = document.NativeFile.Filename,
				Location = document.NativeFile.Location,
				Size = document.NativeFile.Size
			};
		}
	}
}
