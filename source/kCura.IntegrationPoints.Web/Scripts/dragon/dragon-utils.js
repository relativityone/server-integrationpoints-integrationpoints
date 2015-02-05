(function (D, $) {
	function isValidDate(d) {
		if (Object.prototype.toString.call(d) !== "[object Date]")
			return false;
		return !isNaN(d.getTime());
	}

	$.extend(D, {
		findObj: function (string, root) {
			var elms = (string || '').split("."),
			curr = typeof root === "undefined" ? window : root,
			nxt;

			while (nxt = elms.shift()) {
				curr = curr[nxt];
			};

			return curr;
		},
		formatString: function (urlFormat, data) {
			for (var key in data) {
				if (data.hasOwnProperty(key)) {
					urlFormat = urlFormat.replace('{' + key + '}', data[key] || '');
				}
			}
			return urlFormat;
		},
		htmlEscape: function (str) {
			return String(str)
							.replace(/&/g, '&amp;')
							.replace(/"/g, '&quot;')
							.replace(/'/g, '&#39;')
							.replace(/</g, '&lt;')
							.replace(/>/g, '&gt;');
		},
		decodeHtml: function (html) {
			var y = document.createElement('textarea');
			y.innerHTML = html;
			return y.value;
		},
		parseInt: function (number) {
			return parseInt(number, 10);
		},
		UTCtoLocal: function (dateText, dateFormat) {
			var inDateMod = new Date(dateText);
			if (!isValidDate(inDateMod)) {
				return dateText;
			}
			var offSet = inDateMod.getTimezoneOffset();
			if (offSet < 0) {
				inDateMod.setMinutes(inDateMod.getMinutes() + offSet);
			} else {
				inDateMod.setMinutes(inDateMod.getMinutes() - offSet);
			}
			return inDateMod.toString(dateFormat);
		},
		timeLocalToUtc: function (time) {
			var localTime = Date.parseExact(time, "HH:mm") || Date.parseExact(time, "H:mm");
			if (localTime) {
				var temp = new Date();
				var tempDateTime = new Date(temp.getFullYear(), temp.getMonth(), temp.getDate(), localTime.getHours(), localTime.getMinutes(), localTime.getSeconds());
				
				return tempDateTime.getUTCHours() + ":" + tempDateTime.getUTCMinutes();
			}
			return '';
		},
		isDisabled: function (element) {
			return $(element).hasClass('disabled');
		},
		isPromise: function (obj) {
			return typeof obj === "object" && typeof obj.then === "function";
		},
		toggleDisabled: function (el, state) {
			var func = state ? 'addClass' : 'removeClass';
			$(el)[func]('disabled');
			//if (makeDisabled) {
			//	$el.attr('disabled', 'disabled');
			//} else {
			//	$el.attr('disabled', '');
			//}
		},
		format: function (format) {
			var args = $.makeArray(arguments).slice(1);
			if (format == null) { format = ""; }
			return format.replace(/\{(\d+)\}/g, function (m, i) {
				return args[i];
			});
		}
	});

	//keycode module??
	D.keyCode = {
		RETURN: 13,
		BACK_SPACE: 8
	};

	//config module?
	D.config = {
		time: {
			longDate: Date.CultureInfo.formatPatterns.shortDate + ' ' + Date.CultureInfo.formatPatterns.shortTime
		}
	};

})(window.Dragon, jQuery);