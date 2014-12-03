﻿var IP = IP || {};

ko.validation.rules.pattern.message = 'Invalid.';

ko.validation.configure({
	registerExtenders: true,
	messagesOnModified: true,
	insertMessages: true,
	parseInputAttributes: true,
	messageTemplate: null
});

ko.validation.insertValidationMessage = function (element) {
	var errorContainer = document.createElement('div');
	var iconSpan = document.createElement('span');
	iconSpan.className = 'icon-error legal-hold field-validation-error';

	errorContainer.appendChild(iconSpan);

	$(element).parents('.field-value').eq(0).append(errorContainer);

	return iconSpan;
};

ko.validation.rules["time"] = {
	validator: function (value) {
		var date = Date.parseExact(value, "HH:mm") || Date.parseExact(value, "H:mm");
		return !!date;
	},
	message: 'Please enter a valid time (24-hour format).'
};

ko.validation.rules["arrayMin"] = {
	validator: function (value, params) {
		return value.length > params;
	},
	message: 'Bad :('
};

ko.validation.registerExtenders();

(function (root, ko) {
	var initDatePicker = function ($els) {
		$els.datepicker({
			beforeShow: function (el, inst) {
				if ($(el).attr('readonly')) {
					return false;
				}
				inst.dpDiv.css({ marginTop: -el.offsetHeight + 'px', marginLeft: el.offsetWidth + 5 + 'px' });
				return true;
			},
			onSelect: function () {
				//get the shim to work properly
				$(this).blur();
			}
		});
	}

	root.messaging.subscribe('details-loaded', function () {
		initDatePicker($('#scheduleRulesStartDate, #scheduleRulesEndDate'))
	});


	var Choice = function (name, value) {
		this.displayName = name;
		this.value = value;
	};

	var Source = function () {
		this.templateID = 'ldapSourceConfig';
		var self = this;

		this.sourceTypes = ko.observableArray();

		root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('SourceType') }).then(function (result) {
			var types = $.map(result, function (entry) {
				return new Choice(entry.name, entry.value);
			});
			self.sourceTypes(types);
		});

		this.selectedType = ko.observable().extend({ required: true });

	};

	var Destination = function () {
		var self = this;

		IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('RdoFilter') }).then(function (result) {
			var types = $.map(result, function (entry) {
				return new Choice(entry.name, entry.value);
			});
			self.rdoTypes(types);
		}, function () {

		})

		this.templateID = 'ldapDestinationConfig';
		this.rdoTypes = ko.observableArray();
		this.selectedRdo = ko.observable().extend({ required: true });
	};

	var Scheduler = function (model) {
		var options = $.extend({}, {
			enabled: 'false',
			reoccur: 1
		}, model);
		var stateManager = {};
		var SendOnWeekly = function (state) {
			var defaults = {
				selectedDays: []
			};
			var self = this;
			var currentState = $.extend({}, defaults, state);
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
				this.showErrors(true);
			};

			this.selectedDays.extend({
				arrayMin: {
					params: 0,
					message: 'This field is required.',
					onlyIf: function () {
						return self.showErrors();
					}
				}
			})
			this.selectedDays(currentState.selectedDays);
			this.templateID = 'weeklySendOn';
		};

		var SendOnMonthly = function (state) {
			var defaults = {
				monthChoice: 'days'
			};

			var self = this;

			var currentState = $.extend({}, defaults, state);
			this.templateID = 'monthlySendOn';
			var days = [];
			for (var i = 1; i <= 31; i++) {
				days.push(new Choice(i.toString(), i));
			}

			this.days = ko.observableArray(days);
			this.selectedDay = ko.observable(currentState.selectedDay);

			this.overflowMessage = ko.computed(function () {
				return 'Months with fewer than ' + self.selectedDay() + ' days will send on the last day of the month.';
			});

			this.dayTypes = ko.observableArray([
				new Choice("first", 1),
				new Choice("second", 2),
				new Choice("third", 3),
				new Choice("fourth", 4),
				new Choice("last", 5)
			]);

			this.selectedType = ko.observable(currentState.selectedType);

			this.selectedType.subscribe(function () {
				var selected = self.selectedDayOfTheMonth();
				var value = this.target();
				if (value === 1 || value == 5) {
					self.selectedDayOfTheMonth(selected);
				} else {
					self.selectedDayOfTheMonth(2);
				}
			});

			this.daysOfMonth = ko.observableArray([
				new Choice("day", 1),
				new Choice("Monday", 2),
				new Choice("Tuesday", 3),
				new Choice("Wednesday", 4),
				new Choice("Thursday", 5),
				new Choice("Friday", 6),
				new Choice("Saturday", 7),
				new Choice("Sunday", 8),
				new Choice("last", 9),
			]);

			this.daysOfMonthComputed = ko.computed(function () {
				var type = self.selectedType();
				var hideSpecial = type !== 1 && type !== 5;
				return ko.utils.arrayFilter(self.daysOfMonth(), function (entry) {
					if (hideSpecial) {
						return entry.value > 1 && entry.value < 9;
					}
					return true;
				});
			});

			this.selectedDayOfTheMonth = ko.observable(currentState.selectedDayOfTheMonth);

			this.monthChoice = ko.observable(currentState.monthChoice);

		};

		var self = this;
		this.enabled = ko.observable(options.enabled);
		this.templateID = 'schedulingConfig';
		this.sendOn = ko.observable({});

		this.submit = function () {
			if (this.sendOn().submit) {
				this.sendOn().submit();
			}
		};

		this.frequency = ko.observableArray([
			new Choice("Daily", 1),
			new Choice("Weekly", 2),
			new Choice("Monthly", 3)
		]);

		this.isEnabled = ko.computed(function () {
			return self.enabled() === "true";
		});

		this.selectedFrequency = ko.observable().extend({
			required: {
				onlyIf: function () {
					return self.isEnabled();
				}
			}
		});

		this.reoccur = ko.observable(options.reoccur).extend({
			required: {
				onlyIf: function () {
					return self.selectedFrequency() !== 1 && self.isEnabled();
				}
			}
		}).extend({
			validation: {
				validator: function (value, params) {
					var num = parseInt(value, 10);
					return !isNaN(num) && num >= params.min && num <= params.max
				},
				onlyIf: function () {
					return self.selectedFrequency() !== 1 && self.isEnabled();
				},
				params: { min: 1, max: 999 },
				message: 'Please enter a value between 1 and 999.'
			}
		});

		this.reoccurEvery = ko.computed(function () {
			var state = self.selectedFrequency();
			var states = {
				"1": "",
				"2": "week(s)",
				"3": 'month(s)'
			}
			return states[state] || '';
		});

		this.selectedFrequency.subscribe(function (previousValue) {
			stateManager[previousValue] = ko.toJS(self.sendOn());
		}, this, "beforeChange");

		this.selectedFrequency.subscribe(function () {
			var value = this.target();
			var oldState = stateManager[value];
			if (value === 1) {
				self.sendOn({});
			} else if (value === 2) {
				self.sendOn(new SendOnWeekly(oldState));
			} else if (value === 3) {
				self.sendOn(new SendOnMonthly(oldState));
			}
		});

		this.startDate = ko.observable(options.startDate).extend({
			date: {
				message: 'Invalid Date'
			}
		}).extend({
			required: {
				onlyIf: function () {
					return self.isEnabled();
				}
			}
		});

		this.endDate = ko.observable(options.endDate).extend({
			date: {
				message: 'Invalid Date'
			}
		}).extend({
			validation: {
				validator: function (value) {
					if (value && self.startDate() && new Date(value).compareTo(new Date(self.startDate())) < 0) {
						return false;
					}
					return true;
				},
				message: 'The start date must come before the end date.'
			}
		});

		this.scheduledTime = ko.observable().extend({
			required: {
				onlyIf: function () {
					return self.isEnabled();
				}
			}
		}).extend({ time: true });

	};

	var Model = function () {

		var self = this;
		this.name = ko.observable().extend({ required: true });

		this.source = new Source();
		this.destination = new Destination();
		this.overwrite = ko.observableArray([
			new Choice('Append/Overlay', 1234),
			new Choice('Append', 5678)
		]);
		this.selectedOverwrite = ko.observable();
		this.scheduler = new Scheduler();
		this.submit = function () {
			this.scheduler.submit();
		};
	};

	var Step = function (settings) {
		var self = this;
		self.settings = settings;
		this.template = ko.observable();
		this.hasTemplate = false;
		this.model = new Model();
		this.model.errors = ko.validation.group(this.model, { deep: true });

		this.getTemplate = function () {
			IP.data.ajax({ dataType: 'html', cache: true, type: 'get', url: self.settings.url }).then(function (result) {
				$('body').append(result);
				self.hasTemplate = true;
				self.template(self.settings.templateID);
				root.messaging.publish('details-loaded');
			});
		};

		this.submit = function () {
			var d = root.data.deferred().defer();
			this.model.submit();
			if (this.model.errors().length === 0) {
				d.resolve();
			} else {
				this.model.errors.showAllMessages();
				d.reject();
			}
			return d.promise;
		};
	};

	var step = new Step({
		url: IP.utils.generateWebURL('IntegrationPoints', 'StepDetails'),
		templateID: 'step1'
	});

	$("#back").prop('disabled', true);
	root.points.steps.push(step);

})(IP, ko);
