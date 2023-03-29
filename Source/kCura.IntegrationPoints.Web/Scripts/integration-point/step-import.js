var IP = IP || {};
(function (root, ko) {

	var _getAppPath = function () {
		var newPath = window.location.pathname.split('/')[1];
		var url = window.location.protocol + '//' + window.location.host +'/'+ newPath;
		return url;
	};

	var parseURL = function (url, obj) {
		return root.utils.format(url, obj);
	};

	var Step = function (settings) {
		var self = this;
		var frameName = 'configurationFrame';
		self.settings = settings;
		this.template = ko.observable();
		this.hasTemplate = false;
		this.model = {};
		this.frameBus = {};
		this.hasBeenLoaded = false;
		this.bus = IP.frameMessaging({ destination: window.frameElement.contentWindow });
		this.loadModel = function (model) {//loads a readonly version of the ipmodel
			this.stepKey = model.source.selectedType;
			this.model = model;

			if(typeof(stepCache[model.source.selectedType]) === "undefined"){
				stepCache[model.source.selectedType] = self.model.sourceConfiguration || '';
			}

			this.source = $.grep(model.source.sourceTypes, function(item){
				return item.value === model.source.selectedType;
			})[0].href;
		};

        var stepCache = {};

		this.getTemplate = function () {
			IP.data.ajax({ dataType: 'html', cache: true, type: 'get', url: self.settings.url }).then(function (result) {
				var appID = IP.utils.getParameterByName('AppID', window.top);
				var artifactID = self.model.artifactID;
				var obj = {
					applicationPath: _getAppPath(),
					appID: appID,
					artifactID: artifactID
				};
				self.source = parseURL(self.source, obj);

				$('body').append(result);
				self.template(self.settings.templateID);
				self.hasTemplate = true;
				var $frame = $('#' + frameName).attr('src', self.source);
				$frame.iFrameResize({ heightCalculationMethod: 'max' }).on('load', function () {
					self.frameBus = IP.frameMessaging({ destination: window[frameName].contentWindow || window[frameName].frameElement.contentWindow });

					//for ImportProvider, pass along full model to our second step
					if (self.model.source.selectedType === "548f0873-8e5e-4da6-9f27-5f9cda764636") {
						if (stepCache[self.stepKey] !== "") {
							self.model.sourceConfiguration = stepCache[self.stepKey];
						}
						self.frameBus.publish('loadFullState', self.model);
					}
					else {
						var state = stepCache[self.stepKey];

						//Provider is initialized based only on that state, so we need to pass SecuredConfiguration in addition to the rest of the data
						if (state) {
							var stateObject = JSON.parse(state);
							stateObject.SecuredConfiguration = self.model.SecuredConfiguration;
							stateObject.CreateSavedSearchForTagging = JSON.parse(self.model.destination).CreateSavedSearchForTagging;
							state = JSON.stringify(stateObject);
						}

						self.frameBus.publish('load', state);
					}

				});
			});
		};
		this.bus.subscribe("saveState", function (state) {
			//get key from IFrame
			//save sate in local cache
			stepCache[self.stepKey] = state;
		});

		this.submit = function () {
			var d = root.data.deferred().defer();
			this.frameBus.publish('submit');
			//this is sketchy at best
			this.bus.subscribe('saveComplete', function (data) {
				self.model.sourceConfiguration = data;

				stepCache[self.model.source.selectedType] = self.model.sourceConfiguration;
				d.resolve(self.model);
			});

			this.bus.subscribe('saveCompleteImage', function (data) {
				var fullModel = JSON.parse(data);
				self.model.SelectedOverwrite = fullModel.SelectedOverwrite;
				self.model.Map = '[]';
				self.model.sourceConfiguration = fullModel.sourceConfiguration;
				self.model.destination = fullModel.destination;

				stepCache[self.model.source.selectedType] = self.model.sourceConfiguration;
				d.resolve(self.model);
			});

			this.bus.subscribe('saveError', function (error) {
				d.reject(error);
			});
			return d.promise;
		};

		this.back = function () {
			var d = root.data.deferred().defer();
			this.frameBus.publish("back");
			d.resolve();
			return d.promise;
		};
	};

	var step = new Step({
		url: IP.utils.generateWebURL('IntegrationPoints', 'ConfigurationDetail'),
		templateID: 'configuration'
	});



	root.points.steps.push(step);

})(IP, ko);
