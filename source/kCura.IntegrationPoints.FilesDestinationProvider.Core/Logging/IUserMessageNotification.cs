﻿using System;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
	public interface IUserMessageNotification
	{
		event EventHandler<UserMessageEventArgs> UserMessageEvent;
	}
}