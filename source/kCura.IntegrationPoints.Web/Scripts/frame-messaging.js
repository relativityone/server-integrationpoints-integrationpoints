var IP = IP || {};
(function (root) {
	var objectInstances = (function () {
		var cache = {};

		return {
			add: function () { },
			get: function () { }
		};

	})();

	//root.frameMessaging = (function (frame) {
	//	var _reference = $.extend({}, { source: window, origin: '*' }, frame);
	//	var _subscriptions = _reference.source['ip-frame-message'];
	//	if (typeof _subscriptions === "undefined") {
	//		debugger;
	//		_reference.source.addEventListener('message', function (e) {
	//			var _events = e.currentTarget['ip-frame-message'];
	//			var data = {}
	//			try {
	//				data = JSON.parse(e.data);
	//			} catch (e) {
	//				///
	//			}
	//			if (typeof (data) === "undefined") {
	//				return;
	//			}
	//			var subs = _events[data.name] || [];
	//			var newBus = root.frameMessaging(e);
	//			for (var i = 0; i < subs.length; i++) {
	//				var f = subs[i];
	//				f.call(newBus, data.data);
	//			}
	//		});
	//		_reference.source['ip-frame-message'] = _subscriptions || {};
	//	}

	//	var obj =
	//			{
	//				source: _reference.source,
	//				publish: function (name, data) {
	//					var message = JSON.stringify({ 'name': name, 'data': data });
	//					this.source.postMessage(message, _reference.origin);
	//				},
	//				subscribe: function (name, func) {
	//					var _events = _reference.source['ip-frame-message'];
	//					if (!_events[name]) {
	//						_events[name] = [];
	//					}
	//					_events[name].push(func);
	//				}
	//			};


	//	return obj;
	//});

	root.frameMessaging = (function () {
		var model = function (frame) {
			var _subscriptions = frame.source['ip-frame-message'];
			if (typeof _subscriptions === "undefined") {
				frame.source.addEventListener('message', function (e) {
					var _events = e.currentTarget['ip-frame-message'];
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
					var newBus = root.frameMessaging(e);
					for (var i = 0; i < subs.length; i++) {
						var f = subs[i];
						f.call(newBus, data.data);
					}
				});
				frame.source['ip-frame-message'] = _subscriptions || {};
			}
			this.frame = frame;
		};

		model.prototype.subscribe = function (name, func) {
			var _events = this.frame.source['ip-frame-message'];
			if (!_events[name]) {
				_events[name] = [];
			}
			_events[name].push(func);
			this.frame.source['ip-frame-message'] = _events;
		};

		model.prototype.publish = function (name, data) {
			var message = JSON.stringify({ 'name': name, 'data': data });
			this.frame.source.postMessage(message, this.frame.origin);
		};


		return function (frame) {
			var _reference = $.extend({}, { source: window, origin: '*' }, frame);
			return new model(_reference);
		};

	})();

})(IP);