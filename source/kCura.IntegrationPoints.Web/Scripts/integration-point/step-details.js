var IP = IP || {};
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

		this.selectedType = ko.observable();

	};

	var Destination = function () {
		var self = this;

		IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('Test') }).then(function (result) {
			var types = $.map(result, function (entry) {
				return new Choice(entry.m_Item1, entry.m_Item2);
			});
			self.rdoTypes(types);
		}, function () {

		})

		this.templateID = 'ldapDestinationConfig';
		this.rdoTypes = ko.observableArray();
		this.selectedRdo = ko.observable();
	};

	var Scheduler = function () {
		var stateManager = {};

		var SendOnWeekly = function (state) {
			var defaults = {
				selectedDays: []
			};
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
			this.selectedDays = ko.observableArray(currentState.selectedDays);
			this.templateID = 'weeklySendOn';
		};

		var SendOnMonthly = function (state) {
			var defaults = {
				monthChoice:  'days'
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
						return entry.value >1 && entry.value < 9;
					}
					return true;
				});
			});

			this.selectedDayOfTheMonth = ko.observable(currentState.selectedDayOfTheMonth);
			
			this.monthChoice = ko.observable(currentState.monthChoice);

		};
		
		var self = this;
		this.frequency = ko.observableArray([
			new Choice("Daily", 1),
			new Choice("Weekly", 2),
			new Choice("Monthly", 3)
		]);
		
		this.selectedFrequency = ko.observable();
		this.reoccur = ko.observable('asdf');

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



		this.startDate = ko.observable();
		this.endDate = ko.observable();
		this.scheduledTime = ko.observable();
		this.enabled = ko.observable('true');
		this.templateID = 'schedulingConfig';
		this.sendOn = ko.observable({});
	};

	var Model = function () {

		var self = this;
		this.name = {
			label: 'Integration Name:',
			value: ko.observable('')
		};

		this.source = new Source();

		this.destination = new Destination();
		this.overwrite = ko.observableArray([
			new Choice('Append', 1234),
			new Choice('Append Overlay', 5678)
		]);
		this.selectedOverwrite = ko.observable();
		this.scheduler = new Scheduler();
	};

	var Step = function (settings) {
		var self = this;
		self.settings = settings;
		this.template = ko.observable();
		this.hasTemplate = false;
		this.model = new Model();

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
			d.resolve();
			return d.promise;
		};
	};

	var step = new Step({
		url: IP.utils.generateWebURL('IntegrationPoints', 'StepDetails'),
		templateID: 'step1'
	});

	root.points.steps.push(step);

})(IP, ko);

(function () {
	var toggleScheduler = function (func) {
		D('.dragon-panel')[func]();
	}
	

	IP.messaging.subscribe('details-loaded', function () {
		//this comes before initing the drowndown filter so we will populate the state
		//before going forward
		
	});

	IP.messaging.subscribe('details-loaded', function () {
		D('.dragon-panel').scheduleControl();

		$('.required.value').each(function () {
			var $this = $(this);
			$this.siblings('.title').addClass('required');
		});

		var $el = $('#scheduleRulesFrequency, #scheduleRulesSendOnControl select');

		$el.select2({
			dropdownAutoWidth: false,
			dropdownCssClass: "filter-select",
			containerCssClass: "filter-container",
		});
		$el.parent().find('.filter-container span.select2-arrow').removeClass("select2-arrow").addClass("icon icon-chevron-down");

		$el.on('change', function () {
			D.updatePlaceholders();
		});
		var enabled = $('#scheduleRulesEnabled input:checked').val() === "true";

		if (!enabled) {
			toggleScheduler('hide');
		} else {
			toggleScheduler('show');
		}

		$('#scheduleRulesEnabled input').on('change', function () {
			var enabled = $(this).val() === "true";
			var func = enabled ? "show" : "hide";
			toggleScheduler(func);

			D.updatePlaceholders();
		});

	});
})();