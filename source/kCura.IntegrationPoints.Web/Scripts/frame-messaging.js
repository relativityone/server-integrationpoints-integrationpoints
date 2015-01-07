//var IP = IP || {};
//(function (root, $) {

//	//root.frameMessaging = (function () {
//	//	var model = function (frame) {
//	//		var _subscriptions = $(frame.source['ip-frame-message']).data('frame-message');
//	//		if (typeof _subscriptions === "undefined") {
//	//			frame.source.addEventListener('message', function (e) {
//	//				var _events = $(e.currentTarget['ip-frame-message']).data('frame-message') || {};
//	//				var data = {}
//	//				try {
//	//					data = JSON.parse(e.data);
//	//				} catch (e) {
//	//					///
//	//				}
//	//				if (typeof (data) === "undefined") {
//	//					return;
//	//				}
//	//				var subs = _events[data.name] || [];
//	//				var newBus = root.frameMessaging(e);
//	//				for (var i = 0; i < subs.length; i++) {
//	//					var f = subs[i];
//	//					f.call(newBus, data.data);
//	//				}
//	//			});
//	//			$(frame.source['ip-frame-message']).data('frame-message', _subscriptions || {});
//	//		}
//	//		this.frame = frame;
//	//	};

//	//	model.prototype.subscribe = function (name, func) {
//	//		var _events = $(this.frame.source['ip-frame-message']).data('frame-message') || {};
//	//		if (!_events[name]) {
//	//			_events[name] = [];
//	//		}
//	//		_events[name].push(func);
//	//		$(this.frame.source['ip-frame-message']).data('frame-message', _events);
//	//	};

//	//	model.prototype.publish = function (name, data) {
//	//		var message = JSON.stringify({ 'name': name, 'data': data });
//	//		this.frame.source.postMessage(message, this.frame.origin);
//	//	};


//	//	return function (frame) {
//	//		var _reference = $.extend({}, { source: window, origin: '*' }, frame);
//	//		return new model(_reference);
//	//	};

//	//})();

//	root.frameMessaging = (function () {
//		var model = function (frame) {
//			this.frame = frame;
//			this.destinationFrame =frame
//		};

//		model.prototype.subscribe = function () {

//		};

//		model.prototype.publish = function () {

//		}

//		return function (obj) {
//			return new model(obj.source);
//		};

//	})();

//})(IP, jQuery);


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