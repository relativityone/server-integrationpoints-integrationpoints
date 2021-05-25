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

		data.cacheBustUrl = function(urlToBust) {
            var url = urlToBust;
            if (url.includes("?") && url.includes("=")) { // already has some parameters
                url += "&";
            } else {
                url += "?";
            }

            return url + "v=" + IP.assemblyVersion;
        }

		var requestCounter = 0;
		data.ajax = function (options, showOverlayWidget) {
		    if (showOverlayWidget === undefined) {
		        showOverlayWidget = true;
		    }

			var ajaxDefaults = {
				loading: {
					timeout: 200,
					container: 'body'
				},
				cache: false,
				dataType: 'json',
				contentType: 'application/json; charset=utf-8',
				data: {}
			};

			var settings = $.extend({}, ajaxDefaults, options);

			if (!!settings.url) {
				settings.url = data.cacheBustUrl(settings.url);
            }


            var container = settings.loading.container;

			var beforeSend = settings.beforeSend;
			settings.beforeSend = function (request) {
				var args = $.makeArray(arguments);
				if(data.params){
				    request.setRequestHeader('X-IP-USERID', data.params['userID']);
				    request.setRequestHeader('X-IP-CASEUSERID', data.params['caseUserID']);
				}
				if($.isFunction(beforeSend)){
					return beforeSend.apply(this, args);
				}
				return true;
			};

			if (settings.loading.timeout > 0 && showOverlayWidget) {
				root.modal.open(settings.loading.timeout, (container instanceof jQuery) ? container : $(container));

				settings.complete = function () {
					if (requestCounter <= 1) {
						requestCounter = 0;
						if (root.modal && root.modal.close) {
							root.modal.close();
						}
					} else {
						requestCounter--;
					}
				};
				
				requestCounter++;
			}

			return data.deferred($.ajax(settings));
		};

		data.get = function (url) {
			return data.ajax({ url: url, type: 'get' });
		};
		//move to RLH.async
		data.deferred = (function (q) {
		    return function(obj) {
		        if (typeof obj === "object") {
		            return q(obj);
		        } else {
		            return q;
		        }
		    };
		})(Q);

	})(root.data || (root.data = {}));
})(IP || (IP = {}), jQuery, Q, window);