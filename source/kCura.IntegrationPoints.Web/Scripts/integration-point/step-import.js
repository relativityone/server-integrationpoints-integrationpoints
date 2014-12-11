﻿var IP = IP || {};
(function (root, ko) {

	var _getAppPath = function () {
		var newPath = window.location.pathname.split('/')[1];
		var url = window.location.protocol + '//' + window.location.host +'/'+ newPath;
		return url;		
	};

	var parseURL = function (url, obj) {
		var urlFormat = url;
		if (urlFormat[0] === '/') {
			urlFormat = urlFormat.slice(1, urlFormat.length);
		}
		for (var key in obj) {
			if (obj.hasOwnProperty(key)) {
				var regEx = new RegExp('%' + key + '%', "ig")
				urlFormat = urlFormat.replace(regEx, obj[key]);
			}
		}
		return urlFormat;
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
		this.bus =IP.frameMessaging(); 
		this.loadModel = function (model) {//loads a readonly version of the ipmodel
			this.stepKey = model.source.selectedType;
			if (!this.hasBeenLoaded) {
				this.model = model;
				stepCache[model.source.selectedType] = self.model;
				this.hasBeenLoaded = true;
				this.source = $.grep(model.source.sourceTypes, function(item){
					return item.value === model.source.selectedType;
				})[0].href;
			}
		};

		
		var FRAME_KEY = 'syncType';
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
				$frame.iFrameResize({ heightCalculationMethod: 'max' }).load(function () {
					self.frameBus = IP.frameMessaging({ source: window[frameName].contentWindow });
					var state = stepCache[self.stepKey];
					self.frameBus.publish('load', state);
				});
			});
		};

		this.bus.subscribe("saveState", function (state) {
			var key = $('#' + frameName).data(FRAME_KEY);
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