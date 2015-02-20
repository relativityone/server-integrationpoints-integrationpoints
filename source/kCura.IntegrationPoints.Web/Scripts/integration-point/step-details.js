var IP = IP || {};

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
		if (value !== undefined) {
			var date = Date.parseExact(value, "HH:mm") || Date.parseExact(value, "H:mm");
			if (value != null && value.split(':')[0] > 12) {
				return false;
			}
			return !!date;
		}
		return true;
	},
	message: 'Please enter a valid time (12-hour format).'
};
ko.validation.rules["minArray"] = {
		validator: function (value, params) {
			if (value.length > params) {
					return true;
			}
		return false;
		},
	message: 'This field is required.'
};
ko.validation.rules['arrayRange'] = {
    validator: function(value, params) {
        var num = parseInt(value, 10);
        return !isNaN(num) && num >= params.min && num <= params.max
    },
    message: 'Please enter a value between 1 and 999.'
};

ko.validation.registerExtenders();
var IP = IP || {};
(function (root) {
	root.services = root.services || {};

	root.services.getChoice = function (fieldGuid) {
		return root.data.ajax({type: 'Get', url: root.utils.generateWebAPIURL('Choice', fieldGuid)});
	};

})(IP);

(function (root, ko) {
    var initDatePicker = function($els) {
        $els.datepicker({
            beforeShow: function(el, inst) {
                if ($(el).attr('readonly')) {
                    return false;
                }
                inst.dpDiv.css({ marginTop: -el.offsetHeight + 'px', marginLeft: el.offsetWidth + 5 + 'px' });
                return true;
            },
            onSelect: function() {
                //get the shim to work properly
                $(this).blur();
            }
        });
    };

	root.messaging.subscribe('details-loaded', function () {
		initDatePicker($('#scheduleRulesStartDate, #scheduleRulesEndDate'))
	});

	var Choice = function (name, value, artifactID) {
		this.displayName = name;
		this.value = value;
		this.artifactID = artifactID;
	};

	var Source = function (s) {
		var settings = $.extend({}, s);
		this.templateID = 'ldapSourceConfig';
		var self = this;

		this.sourceTypes = ko.observableArray();
		
		this.selectedType = ko.observable().extend({ required: true });
		this.sourceProvider = settings.sourceProvider || 0;
		root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('SourceType') }).then(function (result) {
			var types = $.map(result, function (entry) {
				var c = new Choice(entry.name, entry.value, entry.id);
				c.href = entry.url;
				return c;
			});
			self.sourceTypes(types);
			$.each(self.sourceTypes(), function () {

				if (this.value === settings.selectedType || this.artifactID === self.sourceProvider) {
					self.selectedType(this.value);
				}
			});
		});
		
		this.selectedType.subscribe(function (selectedValue) {
			$.each(self.sourceTypes(), function () {
				if (this.value === selectedValue) {
					self.sourceProvider = this.artifactID;
				}
			});
		});
	};

	var Destination = function (d) {
		try {
			d = JSON.parse(d);
		} catch (e) {
			d = d;
		}
		var settings = $.extend({}, d);
		var self = this;

		IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('RdoFilter') }).then(function (result) {
			var types = $.map(result, function (entry) {
				return new Choice(entry.name, entry.value);
			});
			self.rdoTypes(types);
			self.artifactTypeID(settings.artifactTypeID); //this can only be populated after all the types are loaded.
		}, function () {

		});

		this.templateID = 'ldapDestinationConfig';
		this.rdoTypes = ko.observableArray();

		self.artifactTypeID = ko.observable().extend({ required: true });
	    //CaseArtifactId
		//ParentObjectIdSourceFieldName
	};

	var Scheduler = function (model) {
		var options = $.extend({}, {
			enabled: 'false',
			reoccur: 1,
			sendOn: {}
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
				minArray: {
					params: 0,
					onlyIf: function () {
						return self.showErrors();
					}
				}
			});
			this.selectedDays(currentState.selectedDays);
			this.templateID = 'weeklySendOn';
		};

		var SendOnMonthly = function (state) {
			var defaults = {
				monthChoice: '2'
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

			this.selectedDayOfTheMonth = ko.observable(currentState.selectedDayOfTheMonth);

			this.monthChoice = ko.observable(currentState.monthChoice);
		
		};

		var self = this;

		this.enableScheduler = ko.observable((options.enableScheduler == 'true' || options.enableScheduler == true).toString());
		this.templateID = 'schedulingConfig';
		var sendOn = {};
		try{
			sendOn = JSON.parse(options.sendOn);
		} catch (e) {
			sendOn = {};
		}
		if (typeof (sendOn) === "undefined" || sendOn === null) {
			sendOn = {};
		}
		this.sendOn = ko.observable(sendOn);
		if (sendOn.templateID === "weeklySendOn") {
			this.sendOn(new SendOnWeekly(sendOn));
		} else if (sendOn.templateID === "monthlySendOn") {
			this.sendOn(new SendOnMonthly(sendOn));
		}

		this.submit = function () {
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

		this.reoccur = ko.observable(options.reoccur).extend({
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

		this.selectedFrequency(options.selectedFrequency);

		this.selectedFrequency.subscribe(function () {
			var value = this.target();
			var oldState = stateManager[value];
			if (value === 'Daily') {
				self.sendOn({});
			} else if (value === 'Weekly') {
				self.sendOn(new SendOnWeekly(oldState));
			} else if (value === 'Monthly') {
				self.sendOn(new SendOnMonthly(oldState));
			}
		});

		this.startDate = ko.observable(options.startDate).extend({
			date: {
				message: 'The field Start Date must be a date.'
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
				message: 'The field End Date must be a date.'
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

		this.scheduledTime = ko.observable(options.scheduledTime).extend({
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
		this.selectedTimeFormat = ko.observable(options.selectedTimeFormat);
	};

	var Model = function (m) {
		var settings = $.extend({}, m);
		var self = this;
		this.name = ko.observable(settings.name).extend({ required: true });
		this.source = new Source(settings.source);
		this.destination = new Destination(settings.destination);
		this.destinationProvider = settings.destinationProvider;
		this.overwrite = ko.observableArray([
			'Append/Overlay', 'Append', 'Overlay Only']);
		this.CustodianManagerFieldContainsLink = JSON.parse(settings.destination ||"{}").CustodianManagerFieldContainsLink; 
		this.selectedOverwrite = ko.observable(settings.selectedOverwrite);
		this.scheduler = new Scheduler(settings.scheduler);
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
		this.loadModel = function (ip) {
			ip.source = $.extend({}, { sourceProvider: ip.sourceProvider }, ip.source);
			this.model = new Model(ip);
			
			
			this.model.sourceConfiguration = ip.sourceConfiguration;
		};

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
			this.model.errors = ko.validation.group(this.model, { deep: true });
			this.model.submit();
			if (this.model.errors().length === 0) {
				this.model.destination = JSON.stringify({
					artifactTypeID: ko.toJS(this.model.destination).artifactTypeID,
					ImportOverwriteMode: ko.toJS(this.model.selectedOverwrite).replace('/', '').replace(' ', ''),
					CaseArtifactId: IP.data.params['appID'],
					CustodianManagerFieldContainsLink: ko.toJS(this.model.CustodianManagerFieldContainsLink)
			});
				
				this.model.scheduler.sendOn = JSON.stringify(ko.toJS(this.model.scheduler.sendOn));
				this.model.sourceProvider = this.model.source.sourceProvider;
				d.resolve(ko.toJS(this.model));
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


	root.points.steps.push(step);

})(IP, ko);
