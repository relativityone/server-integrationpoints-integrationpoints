(function (window, $) {
	var Dragon = function (selector, context) {
		return new Dragon.fn.init(selector, context);
	};

	Dragon.fn = Dragon.prototype = {
		constructor: Dragon,
		init: function (selector, context) {
			var self = this;
			var $els = $(selector, context);
			$els.each(function (idx) {
				self[idx] = this;
			});
			self.length = $els.length;
			return this;
		},
		each: function (callback, args) {
			return jQuery.each(this, callback, args);
		},
		UTCtoLocal: function (dateFormat) {
			return this.each(function () {
				this.innerText = D.UTCtoLocal(this.innerText, dateFormat);
			});
		},
		timeUtcToLocal: function (timeFormat) {
			timeFormat = timeFormat || "hh:mm";
			return this.each(function () {
				var time = this.value || this.innerText;
				var currentTime = Date.parseExact(time, "HH:mm") || Date.parseExact(time, "H:mm");
				if (currentTime) {
					var temp = new Date();
					var now = new Date(temp.getFullYear(), temp.getMonth(), temp.getDate(), currentTime.getHours(), currentTime.getMinutes(), currentTime.getSeconds());
					this.innerText = D.UTCtoLocal(now, timeFormat);
				}
			});
		},
		hide: function () {
			return this.each(function () {
				$(this).hide().find(':input').addClass(D.steps.FormControl.ignoreValidationClass);
			});
		},
		show: function () {
			return this.each(function () {
				$(this).show().find(':input').removeClass(D.steps.FormControl.ignoreValidationClass);
			});
		}
	};
	//give this instance the correct prototypes
	Dragon.fn.init.prototype = Dragon.fn;

	$.extend(Dragon, {
		version: '1.0.0'
	});

	window.Dragon = window.D = Dragon;

})(window, jQuery);