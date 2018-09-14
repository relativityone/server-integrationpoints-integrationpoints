var IP;
(function (root, $) {
	"use strict";
	root.modal = (function () {
		var shouldRun = false;
		var $dialog;
		var settings = {
			MODAL_ID: 'progressIndicatorContainer'
		};

		function openModal(waitTime) {
			shouldRun = true;
			setTimeout(function () {
				if (typeof $dialog === 'undefined') {
					$dialog = $('<div id="' + settings.MODAL_ID + '"></div>').appendTo('body');
					$dialog.append('<div class="loading"></div>');					
					$dialog.css({ 'overflow': 'hidden' });
					$dialog.dialog({
						modal: true,
						autoOpen: false,
						height: 'auto',
						width: 'auto',
						resizable: false,
						draggable: false,
						dialogClass: 'transparent no-shadow no-border'
					});
				}

				if (shouldRun) {
					shouldRun = false;
					$("#" + settings.MODAL_ID).prev(".ui-dialog-titlebar").hide();
					$dialog.dialog('open');
				}
			}, waitTime);
		}

		function closeModal(waitTime) {
			if ($dialog) {
				$dialog.dialog('close');
			}
			shouldRun = false;
		}

		return {
			open: openModal,
			close: closeModal
		};
	})();
})(IP || (IP = {}), jQuery);