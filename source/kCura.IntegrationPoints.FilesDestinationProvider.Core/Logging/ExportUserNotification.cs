using System;
using kCura.WinEDDS.Exporters;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
	/// <summary>
	///     All user notifications from Shared Export Library during export
	///     should be handled as fatal error on RIP side.
	///     During export after user notification is shown, process is terminated.
	/// </summary>
	public class ExportUserNotification : IUserMessageNotification, IUserNotification
	{
		public event EventHandler<UserMessageEventArgs> UserMessageEvent;

		public void Alert(string message)
		{
			RaiseUserMessageEvent(message);
		}

		public void AlertCriticalError(string message)
		{
			RaiseUserMessageEvent(message);
		}

		/// <summary>
		///     This method always return false, because export in RIP is performed in Agent and we can't ask user.
		///     After returning false, process will be shutdown.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public bool AlertWarningSkippable(string message)
		{
			RaiseUserMessageEvent(message);
			return false;
		}

		private void RaiseUserMessageEvent(string message)
		{
			UserMessageEvent?.Invoke(this, new UserMessageEventArgs(message));
		}
	}
}