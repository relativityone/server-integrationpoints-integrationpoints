﻿var Scheduler = function (model) {
	var self = this;
	//TODO: move to seperate js file - quick fix
	var Choice = function (name, value, artifactID, object) {
		this.displayName = name;
		this.value = value;
		this.artifactID = artifactID;
		this.model = object;
	};

	this.options = $.extend({}, {
		enabled: "false",
		reoccur: 1,
		sendOn: {}
	}, model);

	var stateManager = {};

	var SendOnWeekly = function (state) {
		var defaults = {
			selectedDays: []
		};
		var self = this;
		this.currentState = $.extend({}, defaults, state);
		this.days = ko.observableArray([
			"Monday",
			"Tuesday",
			"Wednesday",
			"Thursday",
			"Friday",
			"Saturday",
			"Sunday"
		]);

		this.selectedDays = ko.observableArray();
		this.loadCount = 0;
		this.showErrors = ko.observable(false);
		this.submit = function () {
			self.showErrors(true);
		};
		this.selectedDays.extend({
			minArray: {
				params: 0,
				onlyIf: function () {
					return self.showErrors();
				}
			}
		});
		this.selectedDays(this.currentState.selectedDays);
		this.templateID = "weeklySendOn";

		this.loadSettings = function (settings) {
			self.currentState = $.extend({}, defaults, settings);
			self.selectedDays(self.currentState.selectedDays);
		}

	};

	var SendOnMonthly = function (state) {
		var defaults = {
			monthChoice: '2'
		};

		var self = this;

		this.currentState = $.extend({}, defaults, state);
		this.templateID = 'monthlySendOn';
		var days = [];
		for (var i = 1; i <= 31; i++) {
			days.push(new Choice(i.toString(), i));
		}

		this.days = ko.observableArray(days);
		this.selectedDay = ko.observable(this.currentState.selectedDay);
		this.selectedType = ko.observable(this.currentState.selectedType);
		this.selectedDayOfTheMonth = ko.observable(this.currentState.selectedDayOfTheMonth);
		this.monthChoice = ko.observable(this.currentState.monthChoice);

		this.overflowMessage = ko.computed(function () {
			return 'For months containing fewer than ' + self.selectedDay() + ' days, Relativity will attempt to initiate the job on the last day of the month';
		});

		this.dayTypes = ko.observableArray([
			new Choice("first", 1),
			new Choice("second", 2),
			new Choice("third", 3),
			new Choice("fourth", 4),
			new Choice("last", 5)
		]);



		this.selectedType.subscribe(function (value) {
			var selected = self.selectedDayOfTheMonth();
			if (value === 1 || value === 5) {
				self.selectedDayOfTheMonth(selected);
			} else {
				self.selectedDayOfTheMonth(2);
			}
		});

		this.daysOfMonth = ko.observableArray([
			new Choice("day", 128),
			new Choice("Monday", 1),
			new Choice("Tuesday", 2),
			new Choice("Wednesday", 4),
			new Choice("Thursday", 8),
			new Choice("Friday", 16),
			new Choice("Saturday", 32),
			new Choice("Sunday", 64)
		]);

		this.daysOfMonthComputed = ko.computed(function () {
			var type = self.selectedType();
			var hideSpecial = type !== 1 && type !== 5;
			return ko.utils.arrayFilter(self.daysOfMonth(), function (entry) {
				if (hideSpecial) {
					return entry.value >= 1 && entry.value <= 64;
				}
				return true;
			});
		});

		this.loadSettings = function (settings) {
			self.currentState = $.extend({}, defaults, settings);;
			self.selectedDay(currentState.selectedDay);
			self.selectedType(currentState.selectedType);
			self.selectedDayOfTheMonth(currentState.selectedDayOfTheMonth);
			self.monthChoice(currentState.monthChoice);
		}
	};

	this.enableScheduler = ko.observable((this.options.enableScheduler === "true" || this.options.enableScheduler === true).toString());

	this.templateID = "schedulingConfig";
	this.getSendOn = function () {
		var sendOn = {};
		try {
			sendOn = JSON.parse(self.options.sendOn);
		} catch (e) {
			sendOn = {};
		}
		if (typeof (sendOn) === "undefined" || sendOn === null) {
			sendOn = {};
		}
		return sendOn;
	}
	var sendOn = this.getSendOn();
	this.sendOn = ko.observable(sendOn);
	if (sendOn.templateID === "weeklySendOn") {
		this.sendOn(new SendOnWeekly(sendOn));
	} else if (sendOn.templateID === "monthlySendOn") {
		this.sendOn(new SendOnMonthly(sendOn));
	}

	this.submit = function () {
		IP.reverseMapFields = false;
		if (ko.utils.unwrapObservable(this.sendOn).submit) {
			this.sendOn().submit();
		}
	};

	this.frequency = ko.observableArray(["Daily", "Weekly", "Monthly"]);
	this.isEnabled = ko.computed(function () {
		return self.enableScheduler() === "true";
	});

	this.selectedFrequency = ko.observable().extend({
		required: {
			onlyIf: function () {
				return self.isEnabled();
			}
		}
	});
	this.showSendOn = ko.computed(function () {
		var f = self.selectedFrequency();
		return f === 'Weekly' || f === 'Monthly';
	});

	this.reoccur = ko.observable(this.options.reoccur).extend({
		required: {
			onlyIf: function () {
				return self.selectedFrequency() !== 'Daily' && self.isEnabled();
			}
		}
	}).extend({
		arrayRange: {
			onlyIf: function () {
				return self.selectedFrequency() !== 'Daily' && self.isEnabled();
			},
			params: { min: 1, max: 999 },
		}
	});

	this.reoccurEvery = ko.computed(function () {
		var state = self.selectedFrequency();
		var states = {
			"Daily": "",
			"Weekly": "week(s)",
			"Monthly": 'month(s)'
		}
		return states[state] || '';
	});

	this.selectedFrequency.subscribe(function (previousValue) {
		stateManager[previousValue] = ko.toJS(self.sendOn());
	}, this, "beforeChange");

	if (this.options.selectedFrequency === null) {
		this.options.selectedFrequency = undefined;
	}
	this.selectedFrequency(this.options.selectedFrequency);

	this.selectedFrequency.subscribe(function (value) {
		var oldState = stateManager[value];
		if (value === 'Daily') {
			self.sendOn({});
		} else if (value === 'Weekly') {
			self.sendOn(new SendOnWeekly(oldState));
		} else if (value === 'Monthly') {
			self.sendOn(new SendOnMonthly(oldState));
		}
	});

	this.startDate = ko.observable(this.options.startDate).extend({
		date: {
			message: 'The field Start Date must be a date.'
		}
	}).extend({
		validation: {
			validator: function (value) {
				if (!self.isEnabled()) {
					return true;
				}
				if (value) {
					var date = Date.parseExact(value, "M/dd/yyyy");
					if (date == null) {
						return false;
					}
					var currentDate = new Date();
					currentDate.setHours(0, 0, 0, 0);
					if (date.compareTo(currentDate) >= 0) {
						return true;
					}
				}
				return false;
			},
			message: 'Please enter a valid date.'
		}
	}).extend({
		required: {
			onlyIf: function () {
				return self.isEnabled();
			}
		}
	});

	this.timeZoneOffsetInMinute = ko.computed(function () {
		var date = new Date();
		return date.getTimezoneOffset();
	}, this);

	this.endDate = ko.observable(this.options.endDate).extend({
		date: {
			message: 'The field End Date must be a date.'
		}
	}).extend({
		validation: [{
			validator: function (value) {
				if (value) {
					var date = Date.parseExact(value, "M/dd/yyyy");
					if (date == null) {
						return false;
					}
				}

				return true;
			},
			message: 'Please enter a valid date.'
		}, {
			validator: function (value) {
				if (!value) {
					return true;
				}
				var date = new Date(value);
				if (self.startDate() && self.startDate.isValid() && date.compareTo(new Date(self.startDate())) < 0) {
					return false;
				}
				return true;
			},
			message: 'The start date must come before the end date.'
		}]
	});

	this.scheduledTime = ko.observable(this.options.scheduledTime).extend({
		required: {
			onlyIf: function () {
				return self.isEnabled();
			}
		}
	}).extend({
		time: {
			onlyIf: function () {
				return self.isEnabled();
			}
		}
	});
	this.timeFormat = ko.observableArray(['AM', 'PM']);//
	this.selectedTimeFormat = ko.observable(this.options.selectedTimeFormat);

	this.loadSettings = function (settings) {
		self.options = $.extend({}, {
			enabled: "false",
			reoccur: 1,
			sendOn: {}
		}, settings);
		self.enableScheduler((self.options.enableScheduler === "true" || self.options.enableScheduler === true).toString());
		self.reoccur(self.options.reoccur);
		if (self.options.selectedFrequency === null) {
			self.options.selectedFrequency = undefined;
		}
		self.selectedFrequency(self.options.selectedFrequency);
		var state = self.getSendOn();
		if (state.templateID === "weeklySendOn") {
			console.log(state);
			var sendOnWeekly = new SendOnWeekly(state);
			self.sendOn(sendOnWeekly);
		} else if (state.templateID === "monthlySendOn") {
			var sendOnMonthly = new SendOnMonthly(state);
			self.sendOn(sendOnMonthly);
		}
		self.startDate(self.options.startDate);
		self.endDate(self.options.endDate);
		self.scheduledTime(self.options.scheduledTime);
		self.selectedTimeFormat(self.options.selectedTimeFormat);
	};

};
