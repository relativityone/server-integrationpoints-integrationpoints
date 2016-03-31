
namespace kCura.Windows.Process
{
	/// <summary>
	/// Possible event types for the <see cref="kCura.Windows.Process.ProcessEvent"></see> object.
	/// </summary>
	public enum ProcessEventTypeEnum
	{
		/// <summary>
		/// Information message type
		/// </summary>
		Status = 0,
		/// <summary>
		/// Warning message type
		/// </summary>
		Warning = 1,
		/// <summary>
		/// Error message type
		/// </summary>
		Error = 2
	}
}
