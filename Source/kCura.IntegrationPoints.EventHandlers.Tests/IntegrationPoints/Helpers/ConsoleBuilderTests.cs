using System.Collections.Generic;
using System.Linq;
using kCura.EventHandler;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers
{
    [TestFixture, Category("Unit")]
    public class ConsoleBuilderTests : TestBase
    {
        [SetUp]
        public override void SetUp()
        {
            _onClickEventDTO = new OnClickEventDTO
            {
                RetryErrorsOnClickEvent = "event_659",
                StopOnClickEvent = "event_477",
                ViewErrorsOnClickEvent = "event_251",
                RunOnClickEvent = "event_927",
                SaveAsProfileOnClickEvent = "event_511"
            };
            _consoleBuilder = new ConsoleBuilder();
        }

        private const string _CONSOLE_TITLE = "Transfer Options";
        private const string _RUN = "Run";
        private const string _STOP = "Stop";
        private const string _RETRY_ERRORS = "Retry Errors";
        private const string _VIEW_ERRORS = "View Errors";
        private const string _SAVE_AS_PROFILE = "Save as Profile";
        private const string _DOWNLOAD_ERROR_FILE = "Download Error File";
        private ConsoleBuilder _consoleBuilder;
        private OnClickEventDTO _onClickEventDTO;
        private bool ButtonExists(IEnumerable<IConsoleItem> items, string buttonName)
        {
            return FindButton(items, buttonName) != null;
        }

        private ConsoleButton FindButton(IEnumerable<IConsoleItem> items, string buttonName)
        {
            return items.OfType<ConsoleButton>().FirstOrDefault(x => x.DisplayText == buttonName);
        }

        private List<string> GetExpectedButtons(ButtonStateDTO buttonState)
        {
            var result = new List<string>();

            if (buttonState.RunButtonEnabled)
            {
                result.Add(_RUN);
            }
            if (!buttonState.RunButtonEnabled)
            {
                result.Add(_STOP);
            }
            if (buttonState.RetryErrorsButtonVisible)
            {
                result.Add(_RETRY_ERRORS);
            }
            if (buttonState.ViewErrorsLinkVisible)
            {
                result.Add(_VIEW_ERRORS);
            }
            if (buttonState.SaveAsProfileButtonVisible)
            {
                result.Add(_SAVE_AS_PROFILE);
            }
            if (buttonState.DownloadErrorFileLinkVisible)
            {
                result.Add(_DOWNLOAD_ERROR_FILE);
            }

            return result;
        }

        [Test]
        public void ItShouldCreateConsoleButtons([Values(true, false)] bool runButtonEnabled,
            [Values(true, false)] bool stopButtonEnabled,
            [Values(true, false)] bool retryErrorsButtonEnabled,
            [Values(true, false)] bool retryErrorsButtonVisible,
            [Values(true, false)] bool viewErrorsLinkEnabled,
            [Values(true, false)] bool viewErrorsLinkVisible,
            [Values(true, false)] bool saveAsProfileButtonVisible,
            [Values(true, false)] bool downloadErrorFileLinkVisible)
        {
            var buttonState = new ButtonStateDTO
            {
                ViewErrorsLinkVisible = viewErrorsLinkVisible,
                RetryErrorsButtonVisible = retryErrorsButtonVisible,
                ViewErrorsLinkEnabled = viewErrorsLinkEnabled,
                RetryErrorsButtonEnabled = retryErrorsButtonEnabled,
                StopButtonEnabled = stopButtonEnabled,
                RunButtonEnabled = runButtonEnabled,
                SaveAsProfileButtonVisible = saveAsProfileButtonVisible,
                DownloadErrorFileLinkVisible = downloadErrorFileLinkVisible
            };

            var expectedButtons = GetExpectedButtons(buttonState);

            var console = _consoleBuilder.CreateConsole(buttonState, _onClickEventDTO);

            foreach (var expectedButton in expectedButtons)
            {
                Assert.That(ButtonExists(console.Items, expectedButton));
            }
        }

        [Test]
        public void ItShouldCreateConsoleTitle()
        {
            var console = _consoleBuilder.CreateConsole(new ButtonStateDTO(), new OnClickEventDTO());

            Assert.That(console.Title, Is.EqualTo(_CONSOLE_TITLE));
        }

        [Test]
        public void ItShouldCreateDisabledRetryButton()
        {
            var buttonState = new ButtonStateDTO
            {
                ViewErrorsLinkVisible = false,
                RetryErrorsButtonVisible = true,
                ViewErrorsLinkEnabled = false,
                RetryErrorsButtonEnabled = false,
                StopButtonEnabled = false,
                RunButtonEnabled = false,
                SaveAsProfileButtonVisible = false,
                DownloadErrorFileLinkVisible = false,
                DownloadErrorFileLinkEnabled = false
            };

            var console = _consoleBuilder.CreateConsole(buttonState, _onClickEventDTO);

            ConsoleButton button = FindButton(console.Items, _RETRY_ERRORS);

            Assert.That(button.DisplayText, Is.EqualTo(_RETRY_ERRORS));
            Assert.That(button.CssClass, Is.EqualTo("consoleButtonDisabled"));
            Assert.That(button.OnClickEvent, Is.EqualTo(_onClickEventDTO.RetryErrorsOnClickEvent));
            Assert.That(button.Enabled, Is.False);
            Assert.That(button.RaisesPostBack, Is.False);
        }

        [Test]
        public void ItShouldCreateDisabledStopButton()
        {
            var buttonState = new ButtonStateDTO
            {
                ViewErrorsLinkVisible = false,
                RetryErrorsButtonVisible = false,
                ViewErrorsLinkEnabled = false,
                RetryErrorsButtonEnabled = false,
                StopButtonEnabled = false,
                RunButtonEnabled = false,
                SaveAsProfileButtonVisible = false,
                DownloadErrorFileLinkVisible = false,
                DownloadErrorFileLinkEnabled = false
            };

            var console = _consoleBuilder.CreateConsole(buttonState, _onClickEventDTO);

            ConsoleButton button = FindButton(console.Items, _STOP);

            Assert.That(button.DisplayText, Is.EqualTo(_STOP));
            Assert.That(button.CssClass, Is.EqualTo("consoleButtonDisabled"));
            Assert.That(button.OnClickEvent, Is.Null.Or.Empty);
            Assert.That(button.Enabled, Is.False);
            Assert.That(button.RaisesPostBack, Is.False);
        }

        [Test]
        public void ItShouldCreateDisabledViewErrorLink()
        {
            var buttonState = new ButtonStateDTO
            {
                ViewErrorsLinkVisible = true,
                RetryErrorsButtonVisible = false,
                ViewErrorsLinkEnabled = false,
                RetryErrorsButtonEnabled = false,
                StopButtonEnabled = false,
                RunButtonEnabled = false,
                SaveAsProfileButtonVisible = false,
                DownloadErrorFileLinkVisible = false,
                DownloadErrorFileLinkEnabled = false
            };

            var console = _consoleBuilder.CreateConsole(buttonState, _onClickEventDTO);

            ConsoleButton button = FindButton(console.Items, _VIEW_ERRORS);

            Assert.That(button.DisplayText, Is.EqualTo(_VIEW_ERRORS));
            Assert.That(button.CssClass, Is.EqualTo("consoleLinkDisabled"));
            Assert.That(button.OnClickEvent, Is.EqualTo(_onClickEventDTO.ViewErrorsOnClickEvent));
            Assert.That(button.Enabled, Is.False);
            Assert.That(button.RaisesPostBack, Is.False);
        }

        [Test]
        public void ItShouldCreateDisabledDownloadErrorLink()
        {
            var buttonState = new ButtonStateDTO
            {
                ViewErrorsLinkVisible = false,
                RetryErrorsButtonVisible = false,
                ViewErrorsLinkEnabled = false,
                RetryErrorsButtonEnabled = false,
                StopButtonEnabled = false,
                RunButtonEnabled = false,
                SaveAsProfileButtonVisible = false,
                DownloadErrorFileLinkVisible = true,
                DownloadErrorFileLinkEnabled = false
            };

            var console = _consoleBuilder.CreateConsole(buttonState, _onClickEventDTO);

            ConsoleButton button = FindButton(console.Items, _DOWNLOAD_ERROR_FILE);

            Assert.That(button.DisplayText, Is.EqualTo(_DOWNLOAD_ERROR_FILE));
            Assert.That(button.CssClass, Is.EqualTo("consoleLinkDisabled"));
            Assert.That(button.OnClickEvent, Is.EqualTo(_onClickEventDTO.DownloadErrorFileOnClickEvent));
            Assert.That(button.Enabled, Is.False);
            Assert.That(button.RaisesPostBack, Is.False);
        }

        [Test]
        public void ItShouldCreateRetryButton()
        {
            var buttonState = new ButtonStateDTO
            {
                ViewErrorsLinkVisible = false,
                RetryErrorsButtonVisible = true,
                ViewErrorsLinkEnabled = false,
                RetryErrorsButtonEnabled = true,
                StopButtonEnabled = false,
                RunButtonEnabled = false,
                SaveAsProfileButtonVisible = false,
                DownloadErrorFileLinkVisible = false,
                DownloadErrorFileLinkEnabled = false
            };

            var console = _consoleBuilder.CreateConsole(buttonState, _onClickEventDTO);

            ConsoleButton button = FindButton(console.Items, _RETRY_ERRORS);

            Assert.That(button.DisplayText, Is.EqualTo(_RETRY_ERRORS));
            Assert.That(button.CssClass, Is.EqualTo("consoleButtonEnabled"));
            Assert.That(button.OnClickEvent, Is.EqualTo(_onClickEventDTO.RetryErrorsOnClickEvent));
            Assert.That(button.Enabled, Is.True);
            Assert.That(button.RaisesPostBack, Is.False);
        }

        [Test]
        public void ItShouldCreateRunButton()
        {
            var buttonState = new ButtonStateDTO
            {
                ViewErrorsLinkVisible = false,
                RetryErrorsButtonVisible = false,
                ViewErrorsLinkEnabled = false,
                RetryErrorsButtonEnabled = false,
                StopButtonEnabled = false,
                RunButtonEnabled = true,
                SaveAsProfileButtonVisible = false,
                DownloadErrorFileLinkVisible = false,
                DownloadErrorFileLinkEnabled = false
            };

            var console = _consoleBuilder.CreateConsole(buttonState, _onClickEventDTO);

            ConsoleButton button = FindButton(console.Items, _RUN);

            Assert.That(button.DisplayText, Is.EqualTo(_RUN));
            Assert.That(button.CssClass, Is.EqualTo("consoleButtonEnabled"));
            Assert.That(button.OnClickEvent, Is.EqualTo(_onClickEventDTO.RunOnClickEvent));
            Assert.That(button.Enabled, Is.True);
            Assert.That(button.RaisesPostBack, Is.False);
        }

        [Test]
        public void ItShouldCreateStopButton()
        {
            var buttonState = new ButtonStateDTO
            {
                ViewErrorsLinkVisible = false,
                RetryErrorsButtonVisible = false,
                ViewErrorsLinkEnabled = false,
                RetryErrorsButtonEnabled = false,
                StopButtonEnabled = true,
                RunButtonEnabled = false,
                SaveAsProfileButtonVisible = false,
                DownloadErrorFileLinkVisible = false,
                DownloadErrorFileLinkEnabled = false
            };

            var console = _consoleBuilder.CreateConsole(buttonState, _onClickEventDTO);

            ConsoleButton button = FindButton(console.Items, _STOP);

            Assert.That(button.DisplayText, Is.EqualTo(_STOP));
            Assert.That(button.CssClass, Is.EqualTo("consoleButtonDestructive"));
            Assert.That(button.OnClickEvent, Is.EqualTo(_onClickEventDTO.StopOnClickEvent));
            Assert.That(button.Enabled, Is.True);
            Assert.That(button.RaisesPostBack, Is.False);
        }

        [Test]
        public void ItShouldCreateViewErrorLink()
        {
            var buttonState = new ButtonStateDTO
            {
                ViewErrorsLinkVisible = true,
                RetryErrorsButtonVisible = false,
                ViewErrorsLinkEnabled = true,
                RetryErrorsButtonEnabled = false,
                StopButtonEnabled = false,
                RunButtonEnabled = false,
                SaveAsProfileButtonVisible = false,
                DownloadErrorFileLinkVisible = false,
                DownloadErrorFileLinkEnabled = false
            };

            var console = _consoleBuilder.CreateConsole(buttonState, _onClickEventDTO);

            ConsoleButton button = FindButton(console.Items, _VIEW_ERRORS);

            Assert.That(button.DisplayText, Is.EqualTo(_VIEW_ERRORS));
            Assert.That(button.CssClass, Is.EqualTo("consoleLinkEnabled"));
            Assert.That(button.OnClickEvent, Is.EqualTo(_onClickEventDTO.ViewErrorsOnClickEvent));
            Assert.That(button.Enabled, Is.True);
            Assert.That(button.RaisesPostBack, Is.False);
        }

        [Test]
        public void ItShouldNotCreateRetryButton()
        {
            var buttonState = new ButtonStateDTO
            {
                ViewErrorsLinkVisible = false,
                RetryErrorsButtonVisible = false,
                ViewErrorsLinkEnabled = false,
                RetryErrorsButtonEnabled = true,
                StopButtonEnabled = false,
                RunButtonEnabled = false,
                SaveAsProfileButtonVisible = false,
                DownloadErrorFileLinkVisible = false,
                DownloadErrorFileLinkEnabled = false
            };

            var console = _consoleBuilder.CreateConsole(buttonState, _onClickEventDTO);

            Assert.That(ButtonExists(console.Items, _RETRY_ERRORS), Is.False);
        }

        [Test]
        public void ItShouldNotCreateViewErrorLink()
        {
            var buttonState = new ButtonStateDTO
            {
                ViewErrorsLinkVisible = false,
                RetryErrorsButtonVisible = false,
                ViewErrorsLinkEnabled = true,
                RetryErrorsButtonEnabled = false,
                StopButtonEnabled = false,
                RunButtonEnabled = false,
                SaveAsProfileButtonVisible = false,
                DownloadErrorFileLinkVisible = false,
                DownloadErrorFileLinkEnabled = false
            };

            var console = _consoleBuilder.CreateConsole(buttonState, _onClickEventDTO);

            Assert.That(ButtonExists(console.Items, _VIEW_ERRORS), Is.False);
        }

        [Test]
        public void ItShouldCreateSaveAsProfileButton()
        {
            var buttonState = new ButtonStateDTO
            {
                ViewErrorsLinkVisible = false,
                RetryErrorsButtonVisible = false,
                ViewErrorsLinkEnabled = false,
                RetryErrorsButtonEnabled = false,
                StopButtonEnabled = true,
                RunButtonEnabled = false,
                SaveAsProfileButtonVisible = true,
                DownloadErrorFileLinkVisible = false,
                DownloadErrorFileLinkEnabled = false
            };

            var console = _consoleBuilder.CreateConsole(buttonState, _onClickEventDTO);

            ConsoleButton button = FindButton(console.Items, _SAVE_AS_PROFILE);

            Assert.That(button.DisplayText, Is.EqualTo(_SAVE_AS_PROFILE));
            Assert.That(button.CssClass, Is.EqualTo("consoleButtonEnabled"));
            Assert.That(button.OnClickEvent, Is.EqualTo(_onClickEventDTO.SaveAsProfileOnClickEvent));
            Assert.That(button.Enabled, Is.True);
            Assert.That(button.RaisesPostBack, Is.False);
        }

        [Test]
        public void ItShouldCreateDownloadErrorLink()
        {
            var buttonState = new ButtonStateDTO
            {
                ViewErrorsLinkVisible = false,
                RetryErrorsButtonVisible = false,
                ViewErrorsLinkEnabled = false,
                RetryErrorsButtonEnabled = false,
                StopButtonEnabled = false,
                RunButtonEnabled = false,
                SaveAsProfileButtonVisible = false,
                DownloadErrorFileLinkVisible = true,
                DownloadErrorFileLinkEnabled = true
            };

            var console = _consoleBuilder.CreateConsole(buttonState, _onClickEventDTO);

            ConsoleButton button = FindButton(console.Items, _DOWNLOAD_ERROR_FILE);

            Assert.That(button.DisplayText, Is.EqualTo(_DOWNLOAD_ERROR_FILE));
            Assert.That(button.CssClass, Is.EqualTo("consoleLinkEnabled"));
            Assert.That(button.OnClickEvent, Is.EqualTo(_onClickEventDTO.DownloadErrorFileOnClickEvent));
            Assert.That(button.Enabled, Is.True);
            Assert.That(button.RaisesPostBack, Is.False);
        }
    }
}
