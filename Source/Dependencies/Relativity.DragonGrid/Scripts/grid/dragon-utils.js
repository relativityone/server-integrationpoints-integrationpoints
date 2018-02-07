var Dragon;
(function (D) {
	D.keyCode = {
		RETURN: 13,
		BACK_SPACE: 8
	};

	D.findObj = function (string, root) {
		var elms = (string || '').split("."),
		curr = typeof root === "undefined" ? window : root,
		nxt;

		while (nxt = elms.shift()) {
			curr = curr[nxt];
		};

		return curr;
	};

	D.formatString = function (urlFormat, data) {
		for (var key in data) {
			if (data.hasOwnProperty(key)) {
				urlFormat = urlFormat.replace('{' + key + '}', data[key] || '');
			}
		}
		return urlFormat;
	};

	D.htmlEscape = function (str) {
		return String(str)
            .replace(/&/g, '&amp;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
	};

	D.decodeHtml = function (html) {
		var y = document.createElement('textarea');
		y.innerHTML = html;
		return y.value;
	};

	D.updatePlaceholders = function () {
		if (jQuery.placeholder) {
			jQuery.placeholder.shim();
		}
	};

	D.isPromise = function(value) {
		if (typeof value.then !== "function") {
			return false;
		}
		var promiseThenSrc = String($.Deferred().then);
		var valueThenSrc = String(value.then);
		return promiseThenSrc === valueThenSrc;
		
	};


})(Dragon || (Dragon = {}));