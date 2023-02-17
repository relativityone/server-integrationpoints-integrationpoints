using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
    public class ConsoleBuilder : IConsoleBuilder
    {
        private const string _TRANSFER_OPTIONS = "Transfer Options";
        private const string _RUN = "Run";
        private const string _RETRY_ERRORS = "Retry Errors";
        private const string _VIEW_ERRORS = "View Errors";
        private const string _STOP = "Stop";
        private const string _SAVE_AS_PROFILE = "Save as Profile";
        private const string _DOWNLOAD_ERROR_FILE = "Download Error File";

        public Console CreateConsole(ButtonStateDTO buttonState, OnClickEventDTO onClickEvents)
        {
            List<IConsoleItem> buttonList = BuildConsoleButtonList(buttonState, onClickEvents);

            return new Console
            {
                Title = _TRANSFER_OPTIONS,
                Items = buttonList
            };
        }

        private List<IConsoleItem> BuildConsoleButtonList(ButtonStateDTO buttonState, OnClickEventDTO onClickEvents)
        {
            var consoleItems = new List<IConsoleItem>();

            ConsoleButton actionButton = AddActionButton(buttonState, onClickEvents);
            consoleItems.Add(actionButton);

            if (buttonState.RetryErrorsButtonVisible)
            {
                ConsoleButton retryErrorsButton = GetRetryErrorsButton(buttonState.RetryErrorsButtonEnabled, onClickEvents.RetryErrorsOnClickEvent);
                consoleItems.Add(retryErrorsButton);
            }

            if (buttonState.ViewErrorsLinkVisible)
            {
                ConsoleButton viewErrorsLink = GetViewErrorsLink(buttonState.ViewErrorsLinkEnabled, onClickEvents.ViewErrorsOnClickEvent);
                consoleItems.Add(viewErrorsLink);
            }

            if (buttonState.SaveAsProfileButtonVisible)
            {
                ConsoleSeparator separator = new ConsoleSeparator();
                consoleItems.Add(separator);

                ConsoleButton saveAsProfileButton = GetSaveAsProfileButton(onClickEvents.SaveAsProfileOnClickEvent);
                consoleItems.Add(saveAsProfileButton);
            }

            if (buttonState.DownloadErrorFileLinkVisible)
            {
                ConsoleSeparator separator = new ConsoleSeparator();
                consoleItems.Add(separator);

                ConsoleButton downloadErrorFileButton = GetDownloadErrorFileLink(buttonState.DownloadErrorFileLinkEnabled, onClickEvents.DownloadErrorFileOnClickEvent);
                consoleItems.Add(downloadErrorFileButton);
            }

            return consoleItems;
        }

        private ConsoleButton AddActionButton(ButtonStateDTO actionButtonState, OnClickEventDTO actionButtonOnClickEvents)
        {
            bool runButtonEnabled = actionButtonState.RunButtonEnabled;
            bool stopButtonEnabled = actionButtonState.StopButtonEnabled;

            string displayText;
            string cssClass;
            string onClickEvent;
            bool enabled;

            if (runButtonEnabled)
            {
                displayText = _RUN;
                cssClass = "consoleButtonEnabled";
                onClickEvent = actionButtonOnClickEvents.RunOnClickEvent;
                enabled = true;
            }
            else if (stopButtonEnabled)
            {
                displayText = _STOP;
                cssClass = "consoleButtonDestructive";
                onClickEvent = actionButtonOnClickEvents.StopOnClickEvent;
                enabled = true;
            }
            else
            {
                displayText = _STOP;
                cssClass = "consoleButtonDisabled";
                onClickEvent = string.Empty;
                enabled = false;
            }

            return new ConsoleButton
            {
                DisplayText = displayText,
                CssClass = cssClass,
                RaisesPostBack = false,
                Enabled = enabled,
                OnClickEvent = onClickEvent
            };
        }

        private ConsoleButton GetRetryErrorsButton(bool isEnabled, string onClickEvent)
        {
            return new ConsoleButton
            {
                DisplayText = _RETRY_ERRORS,
                RaisesPostBack = false,
                Enabled = isEnabled,
                OnClickEvent = onClickEvent
            };
        }

        private ConsoleButton GetViewErrorsLink(bool isEnabled, string onClickEvent)
        {
            return new ConsoleLinkButton
            {
                DisplayText = _VIEW_ERRORS,
                Enabled = isEnabled,
                RaisesPostBack = false,
                OnClickEvent = onClickEvent
            };
        }

        private ConsoleButton GetSaveAsProfileButton(string onClickEvent)
        {
            return new ConsoleButton
            {
                DisplayText = _SAVE_AS_PROFILE,
                Enabled = true,
                RaisesPostBack = false,
                OnClickEvent = onClickEvent
            };
        }

        private ConsoleButton GetDownloadErrorFileLink(bool isEnabled, string onClickEvent)
        {
            return new ConsoleLinkButton
            {
                DisplayText = _DOWNLOAD_ERROR_FILE,
                Enabled = isEnabled,
                RaisesPostBack = false,
                OnClickEvent = onClickEvent
            };
        }
    }
}
