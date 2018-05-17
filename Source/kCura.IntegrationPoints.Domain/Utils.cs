﻿namespace kCura.IntegrationPoints.Domain
{
	public static class Utils
	{
		public static string GetFormatForWorkspaceOrJobDisplay(string name, int? id)
		{
			return id.HasValue ? $"{name} - {id}" : name;
		}

		public static string GetFormatForWorkspaceOrJobDisplay(string prefix, string name, int? id)
		{
			return $"{prefix} - {GetFormatForWorkspaceOrJobDisplay(name, id)}";
		}
	}
}
