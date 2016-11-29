var IP = IP || {};

(function (root, message) {
	
	var channel = postal.channel();

	root.messaging = {
		publish: function (name, func) {
			channel.publish(name, func);
		},
		subscribe: function (name, func) {
			return channel.subscribe(name, func);
		}
	};

})(IP, postal);