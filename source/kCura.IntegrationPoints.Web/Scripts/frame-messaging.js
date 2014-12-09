var IP = IP || {};
(function (root) {
	//I split this up so there is a clear sepration from the actual messaging implementation
	//in case this has to change due to my ignorance

	root.frameMessaging = (function (frame) {
		var _reference = $.extend({}, { source: window, origin: '*' }, frame);
		var _subscriptions = {};

		return {
			publish: function (name, data) {
				var message = JSON.stringify({ 'name': name, 'data': data });
				_reference.source.postMessage(message, _reference.origin);
			},
			subscribe: function (name, func) {
				if (!_subscriptions[name]) {
					_subscriptions[name] = [];
				}
				_subscriptions[name].push(func);
				_reference.source.addEventListener('message', function (e) {
					var data = {}
					try {
						data = JSON.parse(e.data);
					} catch (e) {
						///
					}
					if (typeof (data) === "undefined") {
						return;
					}
					var subs = _subscriptions[data.name] || [];
					var newBus = root.frameMessaging(e);
					for (var i = 0; i < subs.length; i++) {
						var f = subs[i];
						f.call(newBus, data.data);
					}
				});
			}
		};
	});

})(IP);