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
		public event EventHandler<UserMessageEventArgs> UserFatalMessageEvent;
		public event EventHandler<UserMessageEventArgs> UserWarningMessageEvent;

		public void Alert(string message)
		{
			RaiseUserFatalMessageEvent(message);
		}

		public void AlertCriticalError(string message)
		{
			RaiseUserFatalMessageEvent(message);
		}

		/// <summary>
		///     This method always return false, because export in RIP is performed in Agent and we can't ask user.
		///     After returning false, process will be shutdown.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public bool AlertWarningSkippable(string message)
		{
			RaiseUserWarningMessageEvent(message);
			return true;
		}

		private void RaiseUserFatalMessageEvent(string message)
		{
			UserFatalMessageEvent?.Invoke(this, new UserMessageEventArgs(message));
		}

		private void RaiseUserWarningMessageEvent(string message)
		{
			UserWarningMessageEvent?.Invoke(this, new UserMessageEventArgs(message));
		}
	}
}