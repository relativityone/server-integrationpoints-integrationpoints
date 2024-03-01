var IP;
(function (root, undefined) {
	"use strict";
	(function (utils) {
		var settings = {
			WEB_GUID: 'DCF6E9D1-22B6-4DA3-98F6-41381E93C30C',
			APP_NAME: 'Relativity',
			CS_NAME: 'CustomPages',
			APP_IDENTIFIER: 'AppID',
			API_PREFIX: 'api'
		};

		utils.parseInt = function (num) {
			if (!utils.isInt(num)) {
				return undefined;
			}
			return parseInt(num, 10);
		};

		utils.isInt = function (value) {
			return ((parseFloat(value) == parseInt(value)) && !isNaN(value));
		};

		utils.timeUtcToLocal = function (date) {
			//obsolete - use dragon-utils.js
			//var today = new Date();
			var inDateMod = new Date(date);
			var offSet = inDateMod.getTimezoneOffset();
			if (offSet < 0) {
				inDateMod.setMinutes(inDateMod.getMinutes() + offSet);
			} else {
				inDateMod.setMinutes(inDateMod.getMinutes() - offSet);
			}
			return D.UTCtoLocal('');
		};

		utils.localtoUTC = function (date) {
			var data = (Date.parseExact(date, "HH:mm") || Date.parseExact(date, "H:mm"));
			return data.getUTCHours() + ":" + data.getUTCMinutes();
		};

		utils.addPadding = function (num) {
			if (utils.parseInt(num) < 10) { return "0" + num; }
			return num;
		};

		utils.decodeHtml = function (html) {
			var y = document.createElement('textarea');
			y.innerHTML = html;
			return y.value;
		};

		utils.split = function (str, separator) {
			if (!utils.stringNullOrEmpty(str)) {
				return str.split(separator);
			}
			return [];
		};

		utils.stringNullOrEmpty = function (str) {
			if (str === null || str === '' || typeof (str) === 'undefined') {
				return true;
			};
			return false;
		};

		utils.isDefined = function (item) {
			return typeof item === "undefiend";
		};

		utils.getBaseURL = function () {
			var array = [];
			array.push(window.location.protocol);
			array.push(''); //for the extra / in http(s):/
			array.push(window.location.host);
			array.push(settings.APP_NAME);
			array.push(settings.CS_NAME);
			array.push(settings.WEB_GUID);
			return array.join('/');
		};

		utils.generateWebURL = function () {
			var array = [];
			array.push(utils.getBaseURL());
			Array.prototype.push.apply(array, arguments);
			return array.join('/');
		};

		utils.generateWebAPIURL = function () {
			var array = [];
			array.push(utils.getBaseURL());
			array.push(utils.getParameterByName('AppID', window.top));
			array.push(settings.API_PREFIX);
			for (var i = 0; i < arguments.length; i++) {
				array.push(arguments[i]);
			}
			return array.join('/');
		};

		utils.getParameterByName = function (name, w) {
			w = w || window;
			name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
			var regexS = "[\\?&]" + name + "=([^&#]*)";
			var regex = new RegExp(regexS);
			var results = regex.exec(w.location.search);
			if (results == null) {
				return "";
			} else {
				return decodeURIComponent(results[1].replace(/\+/g, " "));
			}
		};
	})(root.utils || (root.utils = {}));
})(IP || (IP = {}));