var IP = IP || {};
(function (root, $) {
	
	var helper = (function (frame) {
		var tracker = {};
		return {
			get: function () {
				return tracker;
			},
			set: function (data) {
				tracker = data;
			}
		}
	})();
	var _setupMessaging = function () {
		var self = this;
		$.receiveMessage(function (e) {
			var _events = self.destination.get() || {};
			var data = {}
			try {
				data = JSON.parse(e.data);
			} catch (e) {
				///
			}
			if (typeof (data) === "undefined") {
				return;
			}
			var subs = _events[data.name] || [];
			for (var i = 0; i < subs.length; i++) {
				var f = subs[i];
				f.call(self, data.data);
			}
		});
	};

	root.frameMessaging = (function () {
		var model = function (options) {
			this.dFrame = options.destination;
			this.destination = helper;
			_setupMessaging.call(this);
		};

		model.prototype.subscribe = function (name, func) {
			var currentEvents = this.destination.get() || {};
			if (typeof (currentEvents[name]) === "undefined") {
				currentEvents[name] = [];
			}
			currentEvents[name].push(func);
			this.destination.set(currentEvents);
		};

		model.prototype.publish = function (name, data) {
			var message = JSON.stringify({ 'name': name, 'data': data });
			$.postMessage(message, this.dFrame.location.href, this.dFrame);
		}

		return function (obj) {
			var settings = $.extend({}, {
				destination: window.parent.contentWindow || (window.parent.frameElement || {}).contentWindow
			}, obj);
			return new model(settings);
		};

	})();

})(IP, jQuery);