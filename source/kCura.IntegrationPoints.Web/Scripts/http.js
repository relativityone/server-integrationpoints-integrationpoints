var IP;
(function (root, $, Q, window) {
	"use strict";
	var sessionStorage = window.sessionStorage;
	var localStorage = window.localStorage;

	(function (data) {
		var storage = (function (storageLocation) {
			return {
				getItem: function (key) {
					var value = storageLocation.getItem(key);
					if (typeof value === "string") {
						try {
							value = JSON.parse(value);
						} catch (exp) {
							//this is actually a string!!
							value = value;
						}
					}
					return value;
				},
				setItem: function (key, value) {
					var vs = JSON.stringify(value);
					storageLocation.setItem(key, vs);
				}
			};
		});
		data.session = storage(sessionStorage);
		data.local = storage(localStorage);

		data.ajax = function (ajaxData, options) {
			var ajaxDefaults = {
				loading: {
					timeout: 200,
					container: 'body'
				},
				cache: false,
				dataType: 'json',
				contentType: 'application/json; charset=utf-8'
			};

			var settings = $.extend({}, ajaxDefaults, options);
			var container = settings.loading.container;

			if (settings.loading.timeout > 0) {
				root.modal.open(settings.loading.timeout, (container instanceof jQuery) ? container : $(container));
			}
			settings.complete = function () {
				if (root.modal && root.modal.close) {
					root.modal.close();
				}
			};
			return root.data.deferred($.ajax(settings));
		};

		//move to RLH.async
		data.deferred = (function (q) {
			return function (obj) {
				if (typeof obj === "object") {
					return q(obj);
				} else {
					return q;
				}
			}
		})(Q);

	})(root.data || (root.data = {}));
})(IP || (IP = {}), jQuery, Q, window);