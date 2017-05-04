var IP = IP || {};

var loadData = function (ko, dataContainer) {

	var Model = function (dataContainer) {
		var self = this;

		var dayLookUp = {
			'1': "first",
			'2': "second",
			'3': "third",
			'4': "fourth",
			'5': "last"
		};

		var dayOfMonthLookup = {
			'1': "Monday",
			'2': "Tuesday",
			'4': "Wednesday",
			'8': "Thursday",
			'16': "Friday",
			'32': "Saturday",
			'64': "Sunday",
			'128': "day"
		};

		var getLastRun = function (dataContainer) {
			if (!dataContainer.lastRun)
				return "";
			return moment(dataContainer.lastRun).format('M/D/YYYY h:mm A');
		}

		var getNextRun = function (dataContainer) {
			if (!dataContainer.nextRun) {
				return "";
			}

			return moment(dataContainer.nextRun).format('M/D/YYYY h:mm A');
		}

		var getFrequency = function (scheduler) {
			return scheduler.selectedFrequency || "";
		};

		var getReoccur = function (frequecy, scheduler) {
			if (frequecy === "Weekly") {
				return "Every " + scheduler.reoccur + " week(s).";
			} else if (frequecy === "Monthly") {
				return "Every " + scheduler.reoccur + " month(s).";
			} else
				return "";
		};

		var getStartDate = function (scheduler) {
			return scheduler.startDate || "";
		};

		var getEndDate = function (scheduler) {
			return scheduler.endDate || "";
		};

		var buildDaysList = function (selectedDays) {
			var list = '<ul style="list-style-type: none; padding: 0; margin: 0;">';
			$.each(selectedDays,
				function () {
					list += "<li>" + this + "</li>";
				});
			list += "</ul>";
			return list;
		};

		var getSendOn = function (frequency, schedulerSendOn) {
			if (!schedulerSendOn) {
				return "";
			}

			var sendOnJson = JSON.parse(schedulerSendOn);

			if (frequency === "Weekly") {
				return buildDaysList(sendOnJson.selectedDays || []);

			} else if (frequency === "Monthly") {
				if (sendOnJson.monthChoice === 1) {
					return "The " +
						dayLookUp[sendOnJson.selectedType] +
						" " +
						dayOfMonthLookup[sendOnJson.selectedDayOfTheMonth] +
						" of the Month.";
				} else if (sendOnJson.monthChoice === 2) {
					return "Day " + sendOnJson.selectedDay + " of the Month.";
				}
			}

			return "";
		};

		var getTimeZone = function (timeZoneId, windowsTimeZones) {
			var timeZone = null;
			if (timeZoneId) {
				timeZone = ko.utils.arrayFirst(windowsTimeZones,
					function (item) {
						if (item.Id === timeZoneId) {
							return item;
						}
						return null;
					});
			}

			return timeZone ? "; " + timeZone.DisplayName : "";
		};

		var getScheduledTime = function (scheduler, windowsTimeZones) {
			var timeZone = getTimeZone(scheduler.timeZoneId, windowsTimeZones);

			var scheduledTime = IP.timeUtil.format24HourToMilitaryTime(scheduler.scheduledTime || "", "h:mm A") + timeZone;
			return scheduledTime;
		};

		this.enableScheduler = dataContainer.scheduler.enableScheduler;
		this.nextRun = getNextRun(dataContainer);
		this.lastRun = getLastRun(dataContainer);
		this.frequency = getFrequency(dataContainer.scheduler);
		this.reoccur = getReoccur(self.frequency, dataContainer.scheduler);
		this.sendOn = getSendOn(self.frequency, dataContainer.scheduler.sendOn);
		this.startDate = getStartDate(dataContainer.scheduler);
		this.endDate = getEndDate(dataContainer.scheduler);
		this.scheduledTime = getScheduledTime(dataContainer.scheduler, dataContainer.windowsTimeZones);
	};

	var viewModel = new Model(dataContainer);
	ko.applyBindings(viewModel, document.getElementById("schedulerSummaryPage"));
};