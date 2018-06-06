var Dragon;
(function (d, deferred) {
	(function (dialogs) {

		function getDialog() {
			var $dialog = $('<div/>').attr('id', 'msgDiv').appendTo('body');
			return $dialog;
		}

		function handleSuccess(callback) {
			var $dialog = this,
			close = function () { $dialog.dialog('close') },
			enable = function () { $dialog.parent().find('.button.primary').attr('disabled', false) },
			result;

			$dialog.parent().find('.button.primary').attr('disabled', true);
			result = (typeof callback !== "function") || callback.call(this, { close: close, enable: enable });
			if (result) {
				close();
			}
		};

		function handleCancel(callback) {
			var $dialog = this,
			close = function () { $dialog.dialog('close') },
			enable = function () { $dialog.parent().find('.button.secondary').attr('disabled', false) },
			result;

			$dialog.parent().find('.button.secondary').attr('disabled', true);
			result = (typeof callback !== "function") || callback.call(this, { close: close, enable: enable });
			if (result) {
				close();
			}
		};

		dialogs.showConfirm = (function () {
			var confirmDefaults = {
				message: '',
				title: 'Confirmation',
				okText: 'OK',
				cancelText: 'Cancel',
				width: 'auto',
				height: 'auto',
				showOk: true,
				showCancel: true,
				messageAsHtml: false
			};

			function confirm(options) {
				var p = deferred.defer(),
				settings,
				$dialog = getDialog(),
				enable,
				buttons = [];

				settings = $.extend(true, {}, confirmDefaults, options);
				$dialog.html($('<label/>'));

				if (settings.messageAsHtml === true) {
					$dialog.html(settings.message);
				} else {
					$dialog.text(settings.message);
				}

				close = function () { $dialog.dialog('close'); };

				if (settings.showOk) {
					buttons.push({
						text: settings.okText,
						click: function () {
							handleSuccess.call($dialog, settings.success);
						},
						'class': 'button primary'
					});
				}

				if (settings.showCancel) {
					buttons.push({
						text: settings.cancelText,
						click: function () {
							$(this).dialog('close');
						},
						'class': 'button secondary'
					});
				}

				$dialog.dialog({
					autoOpen: true,
					resizable: false,
					draggable: false,
					title: settings.title,
					modal: true,
					height: settings.height,
					width: settings.width,
					dialogClass: 'msg',
					buttons: buttons,
					beforeClose: function () {
						$(this).remove();
					},
					dialogClass: 'prompt'
				});
				return p.promise;
			};

			return confirm;
		})();

		dialogs.showConfirmWithCancelHandler = (function () {
			var confirmDefaults = {
				message: '',
				title: 'Confirmation',
				okText: 'OK',
				cancelText: 'Cancel',
				width: 'auto',
				height: 'auto',
				showOk: true,
				showCancel: true,
				hideTitlebarBorder: true
			};

			function confirm(options) {
				var p = deferred.defer(),
					settings,
					$dialog = getDialog(),
					enable,
					buttons = [];

				settings = $.extend(true, {}, confirmDefaults, options);
				$dialog.html($('<label/>').append(settings.message));

				close = function () { $dialog.dialog('close'); };

				if (settings.showOk) {
					buttons.push({
						text: settings.okText,
						click: function () {
							handleSuccess.call($dialog, settings.success);
						},
						'class': 'button primary'
					});
				}

				if (settings.showCancel) {
					buttons.push({
						text: settings.cancelText,
						click: function () {
							handleCancel.call($dialog, settings.cancel);
						},
						'class': 'button primary'
					});
				}

				$dialog.dialog({
					autoOpen: true,
					resizable: false,
					draggable: false,
					title: settings.title,
					modal: true,
					height: settings.height,
					width: settings.width,
					dialogClass: 'msg',
					buttons: buttons,
					beforeClose: function () {
						$(this).remove();
					},
					dialogClass: 'prompt'
				});
				return p.promise;
			};

			return confirm;
		})();

		dialogs.showTextEntry = (function () {
			var entryDefaults = {
				okText: 'Send',
				okWidth: '100px',
				title: '',
				height: 250,
				width: 625
			};
			function entry(options) {
				var defaults = entryDefaults;
				defaults.height = 215;
				var $dialog = getDialog(),
		$emailBox = $('<textarea/>').attr('id', 'emailAddresses').css('width', '100%').attr('rows', '5').attr('col', '1'),
		$errorDiv = $('<div/>').attr('id', 'errorMessage').css('color', '#FF0000'),
		$outerdiv = $('<div/>').attr('id', 'emailAddressesArea').css('height', '100%').append($errorDiv),
		settings = $.extend(true, {}, defaults, options),
		$saveBtn;

				$outerdiv.append($emailBox);
				$dialog.append($outerdiv);
				$emailBox.on('input propertychange', function () {
					$errorDiv.empty();
				});
				$dialog.dialog({
					autoOpen: true,
					resizable: false,
					draggable: false,
					height: settings.height,
					width: settings.width,
					modal: true,
					title: settings.title,
					buttons: [{
						text: settings.okText,
						width: settings.okWidth,
						'class': 'button primary',
						click: function () {
							handleSuccess.call($dialog, function (calls) {
								return options.success.call($dialog, $emailBox.val().trim(), $errorDiv, calls);
							});
						}
					},
{
	text: 'Cancel',
	click: function () {
		$(this).dialog('close');
	},
	'class': 'button secondary'
}

					],
					beforeClose: function () {
						$(this).remove();
					},
					dialogClass: 'prompt'
				});
			};

			return entry;
		})();

		dialogs.showTestEmailDialog = function (success) {
			var splitRE = /\n?\r|\r?\n|,|;/;
			Dragon.dialogs.showTextEntry({
				title: 'Test Email: <span style="font-size:12px">Separate emails with a comma, semicolon, or new line.</span>',
				success: function (text, $error, calls) {
					var $dialog = this,
					    emails = text.split(splitRE),
					    result = emails.length > 0;

					if (!result) {
						$error.text('At least one email is required');
					} else {
						success(emails, $error, calls);
					}
				}
			});
		};
	})(d.dialogs || (d.dialogs = {}));

})(window.Dragon || (window.Dragon = {}), Q);