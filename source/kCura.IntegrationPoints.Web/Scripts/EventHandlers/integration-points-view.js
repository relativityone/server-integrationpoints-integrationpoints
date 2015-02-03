var IP = IP || {};
(function (root) {
	root.importNow = function (artifactId, appid) {
		IP.data.ajax({
			type: 'post',
			url: root.utils.generateWebAPIURL('ImportNow'),
			data:  JSON.stringify({
				"appId": appid,
					"artifactId": artifactId
			})
});
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