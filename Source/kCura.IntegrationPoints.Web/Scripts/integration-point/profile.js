var Profile = function (profileName, parentModel) {
	this.settings = $.extend({}, { name: profileName });
	var self = this;
	self.disable = parentModel.hasBeenRun();

	this.templateID = 'profileConfig';

	this.selectedProfile = ko.observable();

	this.profileTypes = ko.observableArray();
	this.profiles = [];

	const PROFILE_ERR_PREFIX = 'Issue(s) occurred while loading the profile.';

	this.getSelectedProfilePromise = function (artifactId) {
		var validatedProfileModelPromise = IP.data.ajax({
			url: IP.utils.generateWebAPIURL('IntegrationPointProfilesAPI/GetValidatedProfileModel', artifactId),
			type: 'get'
		}).fail(function (error) {
			console.log(error);
			IP.message.error.raise("Profile not loaded. Please check Error tab for details");
		});
		return validatedProfileModelPromise;
	};
	
	this.sourceProvider = ko.observable();
	this.destinationProvider = ko.observable();

	this.subscriptionSourceProvider = IP.messaging.subscribe('SourceProviderTypeChanged', function (providerType) {
		self.setSaveButton(false);
		self.sourceProvider(providerType);
	});

	this.subscriptionDestinationProviderType = IP.messaging.subscribe('DestinationProviderTypeChanged', function (providerType) {
		self.setSaveButton(false);;
		self.destinationProvider(providerType);
	});
	this.subscriptionTransferedObject = IP.messaging.subscribe("TransferedObjectChanged", function (value) {
		self.setSaveButton(false);
	});

	this.getProfiles = function (ipType) {
		self.setSaveButton(false);
		var profilePromise = IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('IntegrationPointProfilesAPI/GetByType', ipType) });
		profilePromise.then(function (result) {
			var profileTypes = $.map(result, function (entry) {
				return new Choice(entry.name, entry.artifactID, "", { source: entry.sourceProvider, destination: entry.destinationProvider });
			});
			self.profileTypes(profileTypes);
			self.profiles = result;
		});
	};

	this.setSaveButton = function(showFlag){
		$.stepProgress.allowSaveProfile(showFlag);
	}

	this.currentFilter = ko.computed(function() {
		return { source: self.sourceProvider(), destination: self.destinationProvider() };
	});

	this.filterProfiles = ko.computed(function () {
		if (!self.currentFilter() || !self.currentFilter().source || !self.currentFilter().destination) {
			return self.profileTypes();
		} else {
			return ko.utils.arrayFilter(self.profileTypes(), function (profile) {
				return profile.model.source === self.currentFilter().source && profile.model.destination === self.currentFilter().destination;
			});
		}
	});

	this.publishUpdateProfile = function (profileId) {
		if (!!profileId) {
			var promise = self.getSelectedProfilePromise(profileId);
			promise.then(function (result) {
				IP.messaging.publish("loadProfile", result);
			});
		}
	};

	this.notifyUser = function (result) {
		var isValid = result.validationResult.isValid;
		self.setSaveButton(isValid);
		if (isValid) {
			IP.message.notify("Profile has been successfully loaded. Click Next to modify or Save to complete the set up.");
		} else {
			IP.message.errorFormatted.raise(result.validationResult.errors, null, PROFILE_ERR_PREFIX);
		}
	};

	this.selectedProfile.subscribe(function (profileId) {
		self.publishUpdateProfile(profileId);
	});

};
