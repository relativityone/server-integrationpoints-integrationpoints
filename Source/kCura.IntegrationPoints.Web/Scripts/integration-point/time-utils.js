var IP = IP || {};
IP.timeUtil = (function () {

	var dateFormat = "MM/DD/YYYY";
	var dateTimeFormat = "MM/DD/YYYY h:mm A";
	var format24H = "H:m";
	var militaryFormat = "h:mm A";

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
			format = dateFormat;
		}

		return moment(date, format, true).isValid();
	}

	function isTodayOrInTheFuture(date, format) {
		if (!date) {
			return false;
		}

		if (!format) {
			format = dateFormat;
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

	function getLocalIanaTimeZoneId() {
		return moment.tz.guess();
	}

	return {
		anteMeridiem: "AM",
		postMeridiem: "PM",
		dateFormat: dateFormat,
		dateTimeFormat: dateTimeFormat,
		getLocalIanaTimeZoneId: getLocalIanaTimeZoneId,
		format24HourToMilitaryTime: format24HourToMilitaryTime,
		formatMilitaryTimeTo24HourTime: formatMilitaryTimeTo24HourTime,
		formatDateTime: formatDateTime,
		isValidMilitaryTime: isValidMilitaryTime,
		isValidDate: isValidDate,
		isTodayOrInTheFuture: isTodayOrInTheFuture
	};

}());