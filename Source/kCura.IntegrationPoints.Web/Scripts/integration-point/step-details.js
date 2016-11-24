var IP = IP || {};
IP.reverseMapFields = false;

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

IP.emailUtils = (function () {
	var _emailParse = function (emails) {
		return (emails || '').split(';');
	};

	var _validate = function (value) {
		return /^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))$/i.test(value.trim());
	};

	return {
		parse: _emailParse,
		validate: _validate
	};
})();

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
	validator: function (value, params) {
		if (!$.isNumeric(value) || (value.toString().indexOf(".") != -1)) { return false; }
		var num = parseInt(value, 10);
		return !isNaN(num) && num >= params.min && num <= params.max;
	},
	message: 'Please enter a value between 1 and 999.'
};

ko.validation.rules['emailList'] = {
	validator: function (value, param) {
		var emails = IP.emailUtils.parse(value);
		for (var i = 0; i < emails.length; i++) {
			if (!IP.utils.stringNullOrEmpty(emails[i]) && !IP.emailUtils.validate(emails[i])) {
				return false;
			}
		}
		return true;
	},
	message: 'Email(s) are improperly formatted.'
};

ko.validation.registerExtenders();
var IP = IP || {};
(function (root) {
	root.services = root.services || {};

	root.services.getChoice = function (fieldGuid) {
		return root.data.ajax({ type: 'Get', url: root.utils.generateWebAPIURL('Choice', fieldGuid) });
	};
})(IP);

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
	};

	root.messaging.subscribe('details-loaded', function () {
		initDatePicker($('#scheduleRulesStartDate, #scheduleRulesEndDate'));
	});

	var Choice = function (name, value, artifactID, object) {
		this.displayName = name;
		this.value = value;
		this.artifactID = artifactID;
		this.model = object;
	};

	var Source = function (s, parentModel) {
		this.settings = $.extend({}, s);
		this.templateID = 'ldapSourceConfig';
		var self = this;
		self.disable = parentModel.hasBeenRun();

		this.sourceTypes = ko.observableArray();
		this.selectedType = ko.observable().extend({ required: true });
		this.isSourceProviderDisabled = ko.observable(false);

		this.SourceProviderConfiguration = ko.observable();

		this.tmpRelativitySourceTypeObject = null;

		this.sourceProvider = self.settings.sourceProvider || 0;

		this.loadSettings = function (settings) {
			self.settings = settings;
			self.sourceProvider = settings.sourceProvider;
			self.updateSelectedType();
		};
		

		this.displayRelativityInSourceTypes = function (value) {
			if (self.tmpRelativitySourceTypeObject === null) return;

			if (value === true) {
				var containsRelativityObj = false;
				$.each(self.sourceTypes(),
					function () {
						if (this.displayName === "Relativity") {
							containsRelativityObj = true;
						}
					});
				if (containsRelativityObj === false) {
					self.sourceTypes.push(self.tmpRelativitySourceTypeObject);
				}
			} else {
				$.each(self.sourceTypes(),
					function () {
						if (this === self.tmpRelativitySourceTypeObject) {
							self.sourceTypes.remove(this);
						}
					});
			}
		};

		this.updateSelectedType = function () {

			$.each(self.sourceTypes(), function () {
				if (!!self.settings && this.value === self.settings.selectedType || this.artifactID === self.sourceProvider) {
					self.selectedType(this.value);
				}

				if (this.displayName == "Relativity") {
					self.tmpRelativitySourceTypeObject = this;
				}
			});
		};

		this.selectedType.subscribe(function (selectedValue) {
			$.each(self.sourceTypes(), function () {
				if (this.value === selectedValue) {
					self.sourceProvider = this.artifactID;
					if (typeof this.model.config.compatibleRdoTypes === 'undefined' || this.model.config.compatibleRdoTypes === null
					|| parentModel.destination.selectedDestinationTypeGuid() === "1D3AD995-32C5-48FE-BAA5-5D97089C8F18") {
						parentModel.destination.rdoTypes(parentModel.destination.allRdoTypes());
					} else {
						var compatibleRdos = this.model.config.compatibleRdoTypes;
						var rdosToDisplay = [];
						$.each(parentModel.destination.allRdoTypes(), function () {
							if (compatibleRdos.indexOf(this.value) > -1) {
								rdosToDisplay.push(this);
							}
						});
						parentModel.destination.rdoTypes(rdosToDisplay);
					}
					self.SourceProviderConfiguration = this.model.config;
					parentModel.destination.UpdateSelectedItem();
				}
			});
			self.selectedType.isModified(false);
		});
	};

	var Destination = function (d, parentModel) {
		try {
			d = JSON.parse(d);
		} catch (e) {
			d = d;
		}
		this.settings = $.extend({}, d);
		var self = this;
		self.disable = parentModel.hasBeenRun();

		this.loadSettings = function (settings) {
			self.settings = settings;
			self.updateDestinationProvider();
			self.UpdateSelectedItem();//TODO: refactor RDO update dependency on source
		};

		this.templateID = 'ldapDestinationConfig';
		this.allRdoTypes = ko.observableArray();
		this.rdoTypes = ko.observableArray();

		this.destinationTypes = ko.observableArray();
		this.selectedDestinationType = ko.observable().extend({ required: true });

		this.selectedDestinationType.subscribe(function (selectedValue) {
			var destType = self.selectedDestinationTypeGuid();
			if (destType === "1D3AD995-32C5-48FE-BAA5-5D97089C8F18"
			|| (typeof parentModel.source.SourceProviderConfiguration.compatibleRdoTypes === 'undefined' || parentModel.source.SourceProviderConfiguration.compatibleRdoTypes === null)
			) {
				self.rdoTypes(self.allRdoTypes());
			}
			else {
				if ($.isArray(parentModel.source.SourceProviderConfiguration.compatibleRdoTypes)) {
					var rdosToDisplay = [];
					$.each(self.allRdoTypes(), function () {
						if (parentModel.source.SourceProviderConfiguration.compatibleRdoTypes.indexOf(this.value) > -1) {
							rdosToDisplay.push(this);
						}
					});
					self.rdoTypes(rdosToDisplay);
				}
			}
		});

		this.destinationProviderVisible = ko.observable(false);
		this.isDestinationProviderDisabled = ko.observable(false);

		var withArtifactId = function (artifacId) {
			return function (element) {
				return element.artifactID === artifacId;
			}
		}
		this.selectedDestinationTypeGuid = function () {
			var results = self.destinationTypes().filter(withArtifactId(self.selectedDestinationType()));
			return results.length > 0 ? results[0].value : "";
		}

		this.setRelativityAsDestinationProvider = function () {
			var defaultRelativityProvider = self.destinationTypes().filter(function (obj) {
				return obj.value === "74A863B9-00EC-4BB7-9B3E-1E22323010C6";
			});
			if (defaultRelativityProvider.length === 1) {
				self.selectedDestinationType(defaultRelativityProvider[0].artifactID);
			}
		}

		this.updateDestinationProvider = function () {
			$.each(self.destinationTypes(), function () {
				if (!!self.settings && !!self.settings.destinationProviderType && this.value === self.settings.destinationProviderType && self.settings.destinationProviderType !== undefined) {
					self.selectedDestinationType(this.artifactID);
				}
			})
		};
		this.artifactTypeID = ko.observable().extend({ required: true });
		this.UpdateSelectedItem = function () {
			self.artifactTypeID(self.settings.artifactTypeID);
		}

		this.isDestinationObjectDisabled = ko.observable(false);
	};

	var Profile = function (p, parentModel) {
		try {
			p = JSON.parse(p);
		} catch (e) {
			p = p;
		}
		this.settings = $.extend({}, p);
		var self = this;
		self.disable = parentModel.hasBeenRun();

		this.templateID = 'profileConfig';

		this.selectedProfile = ko.observable();

		this.profileTypes = ko.observableArray();
		this.profiles = [];

		this.getSelectedProfile = function (value) {
			var selectedItem = ko.utils.arrayFirst(self.profiles, function (item) {
				if (item.artifactID === value) {
					return item;
				}
			});
			return selectedItem;
		};

		this.getProfiles = function (ipType) {
			var profilePromise = IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('IntegrationPointProfilesAPI/GetByType', ipType) });
			profilePromise.then(function (result) {
				var profileTypes = $.map(result, function (entry) {
					return new Choice(entry.name, entry.artifactID);
				});
				self.profileTypes(profileTypes);
				self.profiles = result;
			});
		};
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
				return 'For months containing fewer than ' + self.selectedDay() + ' days, Relativity will attempt to initiate the job on the last day of the month';
			});

			this.dayTypes = ko.observableArray([
				new Choice("first", 1),
				new Choice("second", 2),
				new Choice("third", 3),
				new Choice("fourth", 4),
				new Choice("last", 5)
			]);

			this.selectedType = ko.observable(currentState.selectedType);

			this.selectedType.subscribe(function (value) {
				var selected = self.selectedDayOfTheMonth();
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
		try {
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

		if (options.selectedFrequency === null) {
			options.selectedFrequency = undefined;
		}
		this.selectedFrequency(options.selectedFrequency);

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

		this.startDate = ko.observable(options.startDate).extend({
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

		this.endDate = ko.observable(options.endDate).extend({
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

		this.name = ko.observable().extend({ required: true });
	
		this.logErrors = ko.observable();
		this.showErrors = ko.observable(false);
		this.integrationPointTypes = ko.observableArray();
		this.type = ko.observable().extend({ required: true });
		this.isEdit = ko.observable();
		this.hasBeenRun = ko.observable();

		this.notificationEmails = ko.observable().extend({
			emailList: {
				onlyIf: function () {
					return self.showErrors();
				}
			}
		});

		this.loadSettings = function (settings) {
			if (settings !== undefined) {
				if (!!settings.name) {
					self.name(settings.name);
				}

				var destinationSettings = JSON.parse(settings.destination || "{}");
				self.SelectedOverwrite = settings.selectedOverwrite;
				self.FieldOverlayBehavior = destinationSettings.FieldOverlayBehavior;
				self.CustodianManagerFieldContainsLink = destinationSettings.CustodianManagerFieldContainsLink;
				self.UseFolderPathInformation = destinationSettings.UseFolderPathInformation;
				self.FolderPathSourceField = destinationSettings.FolderPathSourceField;
				self.ExtractedTextFieldContainsFilePath = destinationSettings.ExtractedTextFieldContainsFilePath;
				self.ExtractedTextFileEncoding = destinationSettings.ExtractedTextFileEncoding;
				self.importNativeFile = destinationSettings.importNativeFile;
				self.destinationProvider = settings.destinationProvider;

				self.type(settings.type);

				self.isEdit(parseInt(settings.artifactID) > 0);

				var hasBeenRun = false;
				if (settings.lastRun != null) {
					hasBeenRun = true;
				}
				else if (settings.hasBeenRun != null) {
					hasBeenRun = settings.hasBeenRun;
				}
				self.hasBeenRun(hasBeenRun);

				self.notificationEmails(settings.notificationEmails);
				if (typeof settings.logErrors === "undefined") {
					settings.logErrors = "true";
				}
				self.logErrors(settings.logErrors.toString());
			}
		}

		this.loadSettings(settings);

		this.destination = new Destination(settings.destination, self);
		this.source = new Source(settings.source, self);
		this.profile = new Profile(settings.profile, self);
		this.scheduler = new Scheduler(settings.scheduler);
		
		//Subscriptions
		this.type.subscribe(function (value) {
			self.setTypeVisibility(value);
			self.profile.getProfiles(value);
		});

		this.profile.selectedProfile.subscribe(function (profileId) {
			self.publishUpdateProfile(profileId);
		});

		this.publishUpdateProfile = function (profileId) {
			if (!!profileId) {
				var item = self.profile.getSelectedProfile(profileId);
				IP.messaging.publish('loadProfile', item);
			}
		};

		this.loadProfile = function (profile) {
			console.log('Subscribe');
			self.loadSettings(profile);
			self.destination.loadSettings(JSON.parse(profile.destination || "{}"));
			self.source.loadSettings(profile);
			console.log(profile);
		};


		var sourceTypePromise = root.data.ajax({ type: 'get', async: false, url: root.utils.generateWebAPIURL('SourceType') });
		var destinationTypePromise = root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('DestinationType') });
		var rdoFilterPromise = IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('RdoFilter/GetAll') });
		var defaultRdoTypeIdPromise = IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('RdoFilter/GetDefaultRdoTypeId') });
		var ipTypesPromise = IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('IntegrationPointTypes') });
		
		root.data.deferred().all([
				sourceTypePromise,
				destinationTypePromise,
				rdoFilterPromise,
				defaultRdoTypeIdPromise,
				ipTypesPromise
		]).then(function (result) {

			var sTypes = $.map(result[0], function (entry) {
				var c = new Choice(entry.name, entry.value, entry.id, entry);
				c.href = entry.url;
				return c;
			});

			var dTypes = $.map(result[1], function (entry) {
				var c = new Choice(entry.name, entry.id, entry.artifactID, entry);
				c.href = entry.url;
				return c;
			});

			var rdoTypes = $.map(result[2], function (entry) {
				return new Choice(entry.name, entry.value);
			});

			self.DefaultRdoTypeId = result[3];

			var ipTypes = $.map(result[4], function (entry) {
				return new Choice(entry.name, entry.identifier, entry.artifactId);
			});

			self.destination.destinationTypes(dTypes);
			self.destination.allRdoTypes(rdoTypes);


			self.destination.destinationProviderVisible(self.destination.destinationTypes().length > 1);

			self.destination.setRelativityAsDestinationProvider();
			self.destination.updateDestinationProvider();

			self.source.sourceTypes(sTypes);
			self.source.updateSelectedType();

			self.integrationPointTypes(ipTypes);
			self.setTypeVisibility(self.type());
			self.profile.getProfiles(self.type());
		});
		
		this.submit = function () {
			this.showErrors(true);
			this.scheduler.submit();
		};

		this.isTypeDisabled = ko.observable(false);

		this.getSelectedType = function (value, comparator) {
			var selectedPath = ko.utils.arrayFirst(self.integrationPointTypes(), function (item) {
				if (comparator(item, value)) {
					return item;
				}
			});
			return selectedPath;
		};

		this.setTypeVisibility = function (type) {
			var isExportType = true;
			var exportGuid = "dbb2860a-5691-449b-bc4a-e18d8519eb3a";
			if (type === undefined || type === 0) {
				type = self.getSelectedType(exportGuid, function (item, guid) { return item.value === guid }).artifactID;
				isExportType = true;
				self.type(type);
			}
			else {
				var guid = self.getSelectedType(type, function (item, artifactID) { return item.artifactID === artifactID }).value;
				isExportType = guid === exportGuid;
			}

			if (self.hasBeenRun() || self.isEdit()) {
				self.source.isSourceProviderDisabled(true);
				self.destination.isDestinationProviderDisabled(true);
				self.destination.isDestinationObjectDisabled(true);
				self.isTypeDisabled(true);
			}
			else {
				self.source.displayRelativityInSourceTypes(isExportType);
				self.source.isSourceProviderDisabled(isExportType);
				self.destination.isDestinationProviderDisabled(!isExportType);
				if (isExportType === false) {
					self.destination.setRelativityAsDestinationProvider();
				} else {
					var relativitySourceProviderGuid = "423b4d43-eae9-4e14-b767-17d629de4bb2";
					self.source.selectedType(relativitySourceProviderGuid);
				}
			}
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
			this.model.Map = ip.Map;
		};

		IP.messaging.subscribe('loadProfile', function (profile) {
			self.model.loadProfile(profile);
			self.model.sourceConfiguration = profile.sourceConfiguration;
			self.model.Map = profile.Map;
		});

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
				this.model.destinationProvider = this.model.destination.selectedDestinationType();
				var guid = this.model.destination.selectedDestinationTypeGuid();
				this.model.destinationProviderGuid = guid;
				this.model.artifactTypeID = this.model.destination.artifactTypeID(),
				this.model.destination = JSON.stringify({
					artifactTypeID: ko.toJS(this.model.destination).artifactTypeID,
					destinationProviderType: ko.toJS(guid),
					CaseArtifactId: IP.data.params['appID'],
					CustodianManagerFieldContainsLink: ko.toJS(this.model.CustodianManagerFieldContainsLink)
				});

				this.model.scheduler.sendOn = JSON.stringify(ko.toJS(this.model.scheduler.sendOn));
				this.model.sourceProvider = this.model.source.sourceProvider;
				this.model.SourceProviderConfiguration = this.model.source.SourceProviderConfiguration;
				d.resolve(ko.toJS(this.model));
			} else {
				this.model.errors.showAllMessages();
				root.message.error.raise("Resolve all errors before proceeding");
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
