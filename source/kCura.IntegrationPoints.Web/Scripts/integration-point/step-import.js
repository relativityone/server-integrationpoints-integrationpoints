var IP = IP || {};
(function (root, ko) {

	var _getAppPath = function () {
		var newPath = window.location.pathname.split('/')[1];
		var url = window.location.protocol + '//' + window.location.host +'/'+ newPath;
		return url;		
	};

	var Step = function (settings) {
		var self = this;
		var frameName = 'configurationFrame';
		self.settings = settings;
		this.template = ko.observable();
		this.hasTemplate = false;
		this.model = {};
		this.frameBus = {};
		this.stepKey = 'test'
		this.hasBeenLoaded = false;
		this.bus =IP.frameMessaging(); 
		this.loadModel = function (model) {//loads a readonly version of the ipmodel
			if(!this.hasBeenLoaded){
				this.model = model;
				stepCache[this.stepKey] = self.model;
				this.hasBeenLoaded = true;
			}
		};

		this.source = '%applicationPath%/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/IntegrationPoints/LDAPConfiguration/%appID%';
		
		var FRAME_KEY = 'syncType';
		var stepCache = {};

		this.getTemplate = function () {
			IP.data.ajax({ dataType: 'html', cache: true, type: 'get', url: self.settings.url }).then(function (result) {
				var applicationPath = _getAppPath();
				var appID = IP.utils.getParameterByName('AppID', window.top);
				var artifactID = 1037306;
				var obj = {
					applicationPath: applicationPath,
					appID: appID,
					artifactID: artifactID
				};
				var urlFormat = self.source;
				for (var key in obj) {
					if (obj.hasOwnProperty(key)) {
						urlFormat = urlFormat.replace('%' + key + '%', obj[key]);
					}
				}
				self.source = urlFormat;
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
				d.resolve(data);
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