var IP = IP || {};
IP.timeUtil = (function () {

	function isValidDate(d) {
		if (Object.prototype.toString.call(d) !== "[object Date]")
			return false;
		return !isNaN(d.getTime());
	}

	function utcToLocal(dateText, dateFormat) {
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
	}

	function timeUtcToLocal(time, timeFormat) {

		timeFormat = timeFormat || "hh:mm";
		var lengthCheck = time.split(':');
		if (lengthCheck[0].length < 2) {
			lengthCheck[0] = "0" + lengthCheck[0];
			time = lengthCheck[0] + ":" + lengthCheck[1];
		}
		if (lengthCheck[1].length < 2) {
			lengthCheck[1] = "0" + lengthCheck[1];
			time = lengthCheck[0] + ":" + lengthCheck[1];
		}
		var currentTime = Date.parseExact(time, "HH:mm") || Date.parseExact(time, "H:mm");

		return time;
	}


	function timeLocalToUtc(time) {

		var localTime = Date.parseExact(time, "HH:mm") || Date.parseExact(time, "H:mm");
		if (localTime) {
			var temp = new Date();
			var tempDateTime = new Date(temp.getFullYear(), temp.getMonth(), temp.getDate(), localTime.getHours(), localTime.getMinutes(), localTime.getSeconds());
			return tempDateTime.getUTCHours() + ":" + tempDateTime.getUTCMinutes();
		}
		return '';
	}

	var _noOp = function (time) {
		return time;
	};

	return {
		utcToLocal: _noOp,
		timeLocalToUtc: _noOp,
		utcDateToLocal: _noOp
	};

}());