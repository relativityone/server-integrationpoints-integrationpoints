var IP = IP || {};
IP.timeUtil = (function () {

	var defaultDateFormat = "MM/DD/YYYY";
	var dateTimeFormat = "MM/DD/YYYY h:mm A";
	var format24H = "H:m";
	var militaryFormat = "h:mm A";
	var discoveredCurrentUserLocale = null;

	function isValidMilitaryTime(militaryTime) {
		if (!militaryTime) {
			return false;
		}

		return moment(militaryTime, "h:mm", true).isValid();
	}

	function isValidDate(date, format) {
		if (!date) {
			return false;
		}

		if (!format) {
			format = defaultDateFormat;
		}

		return moment(date, format, true).isValid();
	}

	function isTodayOrInTheFuture(date, format) {
		if (!date) {
			return false;
		}

		if (!format) {
			format = defaultDateFormat;
		}
		
		var today = moment();
		var future = moment(date, format);

		return !future.isBefore(today, 'day');
	}

	function format24HourToMilitaryTime(time, formatMilitary) {
		if (!time) {
			return "";
		}
		if (!formatMilitary) {
			formatMilitary = "h:mm";
		}
		
		return moment(time, format24H).isValid() ? moment(time, format24H).format(formatMilitary) : "";
	}

	function formatMilitaryTimeTo24HourTime(time, anteOrPostMeridiem) {
		if (!time || !anteOrPostMeridiem) {
			return "";
		}
		
		time = time + " " + anteOrPostMeridiem;

		return moment(time, militaryFormat).isValid() ? moment(time, militaryFormat).format(format24H) : "";
	}

	function formatDateTime(dateTime, format) {
		if (!dateTime) {
			return "";
		}

		if (!format) {
			format = dateTimeFormat;
		}

		return moment(dateTime).isValid() ? moment(dateTime).format(format) : "";
	}

	function formatDate(date, fromFormat, toFormat) {
		if (!date || !fromFormat || !toFormat) {
			return "";
		}

		return moment(date, fromFormat).format(toFormat);
	}

	function getLocalIanaTimeZoneId() {
		return moment.tz.guess();
	}

	function getCurrentUserLocale() {
		var currentLocale;

		if (!!discoveredCurrentUserLocale) {
			currentLocale = discoveredCurrentUserLocale;
		} else {
			if (!!window.navigator.languages) {
				currentLocale = window.navigator.languages[0];
			} else if (!!window.navigator.userLanguage) {
				currentLocale = window.navigator.userLanguage;
			} else if (!!window.navigator.language) {
				currentLocale = window.navigator.language;
			} else {
				currentLocale = "en-us";
			}

			discoveredCurrentUserLocale = currentLocale;
		}

		return currentLocale;
	}

	function getCurrentUserDateFormat() {
		var currentUserLocale = getCurrentUserLocale();
		var localDateFormat = moment().locale(currentUserLocale).localeData()._longDateFormat.L;
		return localDateFormat;
	}

	return {
		anteMeridiem: "AM",
		postMeridiem: "PM",
		defaultDateFormat: defaultDateFormat,
		dateTimeFormat: dateTimeFormat,
		getLocalIanaTimeZoneId: getLocalIanaTimeZoneId,
		format24HourToMilitaryTime: format24HourToMilitaryTime,
		formatMilitaryTimeTo24HourTime: formatMilitaryTimeTo24HourTime,
		formatDateTime: formatDateTime,
		formatDate: formatDate,
		isValidMilitaryTime: isValidMilitaryTime,
		isValidDate: isValidDate,
		isTodayOrInTheFuture: isTodayOrInTheFuture,
		getCurrentUserDateFormat: getCurrentUserDateFormat
	};

}());