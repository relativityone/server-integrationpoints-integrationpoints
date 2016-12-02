var IP = IP || {};
IP.timeUtil = (function () {

	function isValidDate(d) {
		if (Object.prototype.toString.call(d) !== "[object Date]")
			return false;
		return !isNaN(d.getTime());
	}

	function utcToLocal(dateText, dateFormat) {
		var temp = new Date();
		var inDateMod = new Date(temp.getFullYear(), temp.getMonth(), temp.getDate(), dateText[0], dateText[1]);
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
	function utcToTime(time) {
		if (time === '') {
			return '';
		}
		var timeSplit = time.split(':');
		
		if (timeSplit[0] < 12) {
			if (parseInt(timeSplit[0], 10) === 0) {
				timeSplit[0] = 12;
			}
			timeSplit[0] = timeSplit[0] < 10 ? "0" + timeSplit[0] : timeSplit[0];
			timeSplit[1] = timeSplit[1] < 10 ? "0" + timeSplit[1]  : timeSplit[1];
			return timeSplit.join(':') + " AM";
		}
		timeSplit[0] = parseInt(timeSplit[0]) - 12;
		if (parseInt(timeSplit[0], 10) === 0) {
			timeSplit[0] = 12;
		}
		timeSplit[0] = timeSplit[0] < 10 ? "0" + timeSplit[0] : timeSplit[0];
		timeSplit[1] = timeSplit[1] < 10 ? "0" + timeSplit[1]  : timeSplit[1];
		return timeSplit.join(':') + " PM";
	}
	function utcToLocalAmPm(time) {
		
		var scheduleTime = "";
		var timeSplit = utcToLocal(time.split(':'), "HH:mm").split(':');
		if ((timeSplit[0] - 12) >= 1) {
			scheduleTime = timeSplit[0] - 12 + ":" + timeSplit[1] + " PM";
		} else {
			scheduleTime = (timeSplit[0].length==2)  + ":" + timeSplit[1] + ' AM';
		}
		return scheduleTime;
	}

	function timeFormatMinutes(time) {
		if (!time) {
			return time;
		}

		var lengthCheck = time.split(':');
		if (lengthCheck[1].length < 2) {
			lengthCheck[1] = "0" + lengthCheck[1];
			time = lengthCheck[0] + ":" + lengthCheck[1];
		}
		return time;
	}

	function timeUtcToLocal(time) {

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
		utcToLocal: utcToLocal,
		timeLocalToUtc: timeLocalToUtc,
		utcDateToLocal: _noOp,
		timeToAmPm: utcToTime,
		utcToLocalAmPm: utcToLocalAmPm,
		formatTimeMinutes: timeFormatMinutes
	};

}());