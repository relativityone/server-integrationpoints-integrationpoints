﻿var IP;
(function (root, $) {
	"use strict";
	(function (message) {
		var $main = $('#bodyContainer');

		function getElement($el, $default) {
			if (root.utils.isDefined($el) || !($el instanceof $)) {
				return $default;
			}
			return $el;
		}

		function _isJson(obj) {
			try {
				JSON.parse(obj);
				return true;
			} catch (e) {
				return false;
			}
		}

		function getMessage(error) {
			var DEFAULT_ERROR = 'There was an error processing your request. Please see the errors tab for details.';
			var result;
			if (typeof error === "object") {
				if (error.hasOwnProperty('responseText')) {
					//webAPI
					try {
						if (_isJson(error.responseText)) {
							result = JSON.parse(error.responseText);
							var props = ['ExceptionMessage', 'exceptionMessage', "Message", "message"];
							$.each(props, function () {
								if (result.hasOwnProperty(this)) {
									result = result[this];
									return false;
								}
							});
						} else {
							result = error.responseText;
						}
					} catch (e) {
						if (error.getResponseHeader('Content-Type').indexOf('html') > -1) {
							result = $(error.responseText).eq(2).text();
							if ($.trim(result) == '') {
								result = $(error.responseText).eq(1).text();
							}

						}

					}
				} else if ($.isArray(error.ErrorMessages)) {
					result = '';
					for (var i = 0; i < error.ErrorMessages.length; i++) {
						result = result + '\n' + error.ErrorMessages[i];
					}
				} else if (error.toString) {
					result = error.toString();
				} else {
					//umm... what the heck is this thing?
					result = DEFAULT_ERROR;
				}
			} else if (typeof error === "string") {
				result = error;
			}
			if (root.utils.stringNullOrEmpty(result)) {
				result = DEFAULT_ERROR;
			}
			return result;
		}

		//change to constants
		var settings = {
			SHOW_ANIMATION: 'fade'
		};

		message.error = (function () {
			function raiseError(messageBody, $container) {
				messageBody = getMessage(messageBody);
				var $el = getElement($container, $main),
						$error = $('<div class="page-message page-error"/>').append('<span class="legal-hold icon-error"></span>').append($('<div/>').append(messageBody)).hide();

				clearError($el);

				if ($.isFunction($container)) {
					$error.show();
					$container.call($error, $error);
				} else {
					$error.prependTo($el).show({
						effect: settings.SHOW_ANIMATION
					});
				}
			};
			function clearError($container) {
				var $el = getElement($container, $main);
				$el.find('div.page-error').remove();
			};
			return {
				raise: raiseError,
				clear: clearError
			};
		})();

		message.info = (function () {
			function raiseInfo(messageBody, $container) {
				var $el = getElement($container, $main),
						$error = $('<div class="page-message page-info"/>').append('<span class="legal-hold icon-step-complete"></span>').append($('<div/>').append(messageBody)).hide();

				clearInfo($el);

				$error.prependTo($el).show({
					effect: settings.SHOW_ANIMATION
				});
			};
			function clearInfo($container) {
				var $el = getElement($container, $main);
				$el.find('div.page-info').remove();
			};

			return {
				raise: raiseInfo,
				clear: clearInfo
			};
		})();

		message.displayUnresolvedError = function (e, $container) {
			//needs to be able to handle $container, webAPI fail, Web controller fail and maybe just a fail message as well!
			var message = getMessage(e);
			root.message.error.raise(message, $container);
		}

	})(root.message || (root.message = {}));
})(IP || (IP = {}), jQuery);