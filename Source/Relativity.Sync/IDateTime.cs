using System;

namespace Relativity.Sync
{
	/// <summary>
	/// Interface for <see cref="System.DateTime"/>
	/// </summary>
	public interface IDateTime
	{
		/// <inheritdoc cref="System.DateTime"/>
		DateTime Now { get; }
	}
}