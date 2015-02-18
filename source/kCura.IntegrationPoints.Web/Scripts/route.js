var IP = IP || {};

IP.redirect = (function (undefined) {

	var _getURL = function() {
		var url = undefined;
		if (typeof (localStorage.URL) !== "undefined") {
			url = localStorage.URL;
		}
		localStorage.setItem("URL", undefined);
		return url;
	};

	var _setURL = function (_url) {
			localStorage.setItem("URL", _url);
	};
	var _setEditFlag = function(bool) {
		localStorage.setItem("edit", bool);
	};
	var edit = function() {
		var routedFromEdit = false;
		if (localStorage.edit !== "undefined" && localStorage.edit !== "false") {
			routedFromEdit = localStorage.edit;
		}
		localStorage.setItem("edit", undefined);
		return routedFromEdit;
	};
	return {
		get: _getURL,
		set: _setURL,
		reset: _setEditFlag,
		isEdit : edit,
	}
})();
