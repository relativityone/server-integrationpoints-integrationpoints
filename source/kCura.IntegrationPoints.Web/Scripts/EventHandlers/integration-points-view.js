var IP = IP || {};
(function (root) {
	root.importNow = function (id) {

	};


	var _convertUTCToLocal = function () {

	};

	var config = {
		time: {
			longDate: Date.CultureInfo.formatPatterns.shortDate + ' ' + Date.CultureInfo.formatPatterns.shortTime
		}
	};

	$(function () {
		$.each(IP.nextTimeid || [], function () {
			var $el = $('input[faartifactid="' + this + '"]').siblings('.dynamicViewFieldValue');
			var text = $el.text();
			var result = IP.timeUtil.utcDateToLocal(text, config.time.longDate);
			$el.text(result);
		});
		
	});
	

})(IP);

//function helloworld() {
//	alert("HELLO ");
//}