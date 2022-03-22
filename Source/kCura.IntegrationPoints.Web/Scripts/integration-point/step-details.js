var IP = IP || {};
IP.reverseMapFields = false;

var relativitySourceProviderGuid = "423B4D43-EAE9-4E14-B767-17D629DE4BB2";
var relativityDestinationProviderGuid = "74A863B9-00EC-4BB7-9B3E-1E22323010C6";

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
			return IP.timeUtil.isValidMilitaryTime(value);
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

	var Model = function (m) {
		var settings = $.extend({}, m);
		var self = this;

		this.name = ko.observable().extend({
			required: true,
			textFieldWithoutSpecialCharacters: {}
		});
		if (!!settings.name) {
			self.name(settings.name);
		}

		this.logErrors = ko.observable();
		this.showErrors = ko.observable(false);
		this.isTypeDisabled = ko.observable(false);
		this.isExportType = ko.observable(true);
		this.integrationPointTypes = ko.observableArray();
		this.type = ko.observable().extend({ required: true });
		this.isEdit = ko.observable(parseInt(settings.artifactID) > 0);
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

				var destinationSettings = JSON.parse(settings.destination || "{}");
				self.SelectedOverwrite = settings.selectedOverwrite;
				self.FieldOverlayBehavior = destinationSettings.FieldOverlayBehavior;
				self.EntityManagerFieldContainsLink = destinationSettings.EntityManagerFieldContainsLink;
				self.UseFolderPathInformation = destinationSettings.UseFolderPathInformation;
				self.UseDynamicFolderPath = destinationSettings.UseDynamicFolderPath;
				self.FolderPathSourceField = destinationSettings.FolderPathSourceField;
				self.ImageImport = destinationSettings.ImageImport;
				self.MoveExistingDocuments = destinationSettings.MoveExistingDocuments;
				self.LongTextColumnThatContainsPathToFullText = destinationSettings.LongTextColumnThatContainsPathToFullText;
				self.ExtractedTextFieldContainsFilePath = destinationSettings.ExtractedTextFieldContainsFilePath;
				self.ExtractedTextFileEncoding = destinationSettings.ExtractedTextFileEncoding;
				self.importNativeFile = destinationSettings.importNativeFile;
			    self.importNativeFileCopyMode = destinationSettings.importNativeFileCopyMode;
				self.CreateSavedSearchForTagging = destinationSettings.CreateSavedSearchForTagging;
				self.IPDestinationSettings = destinationSettings;
				self.destinationProvider = settings.destinationProvider;
				self.SecuredConfiguration = settings.securedConfiguration;

				self.type(settings.type);

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
		this.profile = new Profile(settings.profileName, self);
		this.scheduler = new Scheduler(settings.scheduler);

		//Subscriptions
		this.type.subscribe(function (value) {
			self.setTypeVisibility(value);
			self.profile.getProfiles(value);
		});

		this.profile.selectedProfile.subscribe(function (profileId) {
			self.isProfileLoaded = self.isProfileLoaded || profileId != undefined;
		});

		this.loadProfile = function (result) {
			self.loadSettings(result.model);
			self.destination.loadSettings(JSON.parse(result.model.destination || "{}"));
			self.source.loadSettings(result.model);
			self.scheduler.loadSettings(result.model.scheduler);
			self.profile.notifyUser(result);
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
				return new Choice(entry.name, entry.value, null, null, entry.belongsToApplication);
			});

			self.DefaultRdoTypeId = result[3];

			var ipTypes = $.map(result[4], function (entry) {
				return new Choice(entry.name, entry.identifier, entry.artifactId);
			});

			self.destination.destinationTypes(dTypes);
			self.destination.allRdoTypes(rdoTypes);
			self.source.sourceTypes(sTypes);
			self.integrationPointTypes(ipTypes);

			self.destination.destinationProviderVisible(self.destination.destinationTypes().length > 1);
			self.destination.setRelativityAsDestinationProvider();
			self.destination.updateDestinationProvider();
			self.source.updateSelectedType();

			self.setTypeVisibility(self.type());
			self.profile.getProfiles(self.type());
		});

		this.submit = function () {
			this.showErrors(true);
			this.scheduler.submit();
		};

		this.getSelectedType = function (value, comparator) {
			var selectedPath = ko.utils.arrayFirst(self.integrationPointTypes(), function (item) {
				if (comparator(item, value)) {
					return item;
				}
			});
			return selectedPath;
		};

		this.setTypeVisibility = function (type) {

			var enableSyncNonDocumentFlow = IP.data.params['EnableSyncNonDocumentFlowToggleValue'];

			var exportGuid = "dbb2860a-5691-449b-bc4a-e18d8519eb3a";
			if (type === undefined || type === 0) {
				type = self.getSelectedType(exportGuid, function (item, guid) { return item.value === guid }).artifactID;
				self.isExportType(true);
				self.type(type);
			}
			else {
				var guid = self.getSelectedType(type, function (item, artifactID) { return item.artifactID === artifactID }).value;
				self.isExportType(guid === exportGuid);
			}

			if (self.hasBeenRun() || self.isEdit()) {
				self.source.isSourceProviderDisabled(true);
				self.destination.isDestinationProviderDisabled(true);
				self.destination.isDestinationObjectDisabled(true);
				self.isTypeDisabled(true);
			}
			else {
				var isExportType = self.isExportType();
				self.source.displayRelativityInSourceTypes(isExportType);
				self.source.isSourceProviderDisabled(isExportType);
				self.destination.isDestinationProviderDisabled(!isExportType);
				if (isExportType === false) {
					self.destination.setRelativityAsDestinationProvider();
				} else {
					var relativitySourceProviderGuid = "423b4d43-eae9-4e14-b767-17d629de4bb2";
					self.source.selectedType(relativitySourceProviderGuid);
				}
				var disableRdoSelection = isExportType && !enableSyncNonDocumentFlow;
				self.destination.isDestinationObjectDisabled(disableRdoSelection);
			}
		};

		this.isSyncFlow = function () {
			var sourceProvider = (this.source.selectedType() || '').toUpperCase();
			var destinationProvider = (this.destination.selectedDestinationTypeGuid() || '').toUpperCase();
	
			return sourceProvider === relativitySourceProviderGuid && destinationProvider === relativityDestinationProviderGuid;
		};
	};

	var Step = function (settings) {
		var self = this;
		self.settings = settings;
		this.template = ko.observable();
		this.hasTemplate = false;
		this.loadModel = function (ip) {
			ip.source = $.extend({}, { sourceProvider: ip.sourceProvider }, ip.source);
			this.model = new Model(ip);
			this.model.sourceConfiguration = ip.sourceConfiguration;
			this.model.map = ip.map;
		};

		IP.messaging.subscribe("loadProfile", function (result) {
			self.model.loadProfile(result);
			self.model.sourceConfiguration = result.model.sourceConfiguration;
			self.model.map = result.model.map;
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
				this.model.artifactTypeID = this.model.destination.artifactTypeID();
				var destination = {
					artifactTypeID: ko.toJS(this.model.destination).artifactTypeID,
					destinationProviderType: ko.toJS(guid),
					EntityManagerFieldContainsLink: ko.toJS(this.model.EntityManagerFieldContainsLink),
					CreateSavedSearchForTagging: ko.toJS(this.model.destination).settings.CreateSavedSearchForTagging
				};
				if (this.model.destination.profile) {
					destination = $.extend(this.model.destination.profile, destination);
				} else {
					destination.CaseArtifactId = IP.data.params['appID'];
				}
				this.model.destination = JSON.stringify(destination);
				this.model.scheduler.sendOn = JSON.stringify(ko.toJS(this.model.scheduler.sendOn));
				this.model.sourceProvider = this.model.source.sourceProvider;
				this.model.SourceProviderConfiguration = this.model.source.SourceProviderConfiguration;
				this.model.IntegrationPointTypeIdentifier = this.model.getSelectedType(this.model.type(), function (item, artifactID) { return item.artifactID === artifactID }).value;
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
