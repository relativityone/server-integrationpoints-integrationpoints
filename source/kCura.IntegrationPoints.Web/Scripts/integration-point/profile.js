var Profile = function (profileName, parentModel) {
	this.settings = $.extend({}, { name: profileName });
	var self = this;
	self.disable = parentModel.hasBeenRun();

	this.templateID = 'profileConfig';

	this.selectedProfile = ko.observable();

	this.profileTypes = ko.observableArray();
	this.profiles = [];

	this.getSelectedProfilePromise = function (artifactId) {
		var validatedProfileModelPromise = IP.data.ajax({
			url: IP.utils.generateWebAPIURL('IntegrationPointProfilesAPI/GetValidatedProfileModel', artifactId),
			type: 'get'
		}).fail(function (error) {
			IP.message.error.raise("No exportable fields were returned from the source provider.");
		});
		return validatedProfileModelPromise;
	};

	this.currentFilter = ko.observable();

	this.subscription = IP.messaging.subscribe('ProviderTypeChanged', function (providerType) {
		self.setSaveButton(false);
		if (!providerType) {
			self.currentFilter(undefined);//reset if providerType empty
		}
		else if (!!parentModel.source.sourceProvider && !!parentModel.destination.selectedDestinationType) {
			self.currentFilter({ source: parentModel.source.sourceProvider, destination: parentModel.destination.selectedDestinationType() });
		}
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
			if (self.settings.name) {
				self.selectedProfile(self.settings.name);
			}
		});
	};

	this.setSaveButton = function(showFlag)
	{
		$.stepProgress.allowSaveProfile(showFlag);
	}

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
			promise.then(function (profile) {
				IP.messaging.publish("loadProfile", profile);
				self.setSaveButton(true);
			});
		}
	};

	this.selectedProfile.subscribe(function(profileId) {
		self.publishUpdateProfile(profileId);
	});

};
