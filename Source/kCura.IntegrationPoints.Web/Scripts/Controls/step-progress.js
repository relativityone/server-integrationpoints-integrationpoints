(function ($) {
	var settings = {
		pendingClass: 'pending',
		completedClass: 'complete',
		activeClass: 'in-progress'
	};

	var _buildStep = function (step) {
		var last = step.step === this.options.steps.length ? "last" : "";
		var container = '<li class="' + last + '">';
		container += '<div id="step' + step.step + 'text">' + step.options.text + '</div>';
		var div = "<div class='step'>";
		div += "<i class='" + settings.pendingClass + "'></i>";
		div += "<span class='" + settings.pendingClass + "'>" + step.step + "</span>";
		div += "</div>";
		container += div;
		container += "<div class='bar'></div>";
		container += '</li>';
		return container;
	};

	var _next = function () {
		var nextStep = this.options.currentStep + 1;
		if (this.options.steps.length <= nextStep) {
			nextStep = this.options.steps.length;
		}
		$.stepProgress.goToStep.call(this, nextStep);
	};

	var _back = function () {
		var nextStep = this.options.currentStep - 1;
		if (nextStep <= 0) {
			nextStep = 1;
		}
		$.stepProgress.goToStep.call(this, nextStep);
	};

	var _goToStep = function (stepIdx) {
		this.$this.find('li').each(function (idx) {
			idx = idx + 1;
			var $this = $(this),
					classesToRemove = '',
					classesToAdd = '',
					iconClass = ''
			if (idx < stepIdx) {
				iconClass = 'icon-step-complete';
				classesToRemove = settings.pendingClass + ' ' + settings.activeClass;
				classesToAdd = settings.completedClass;
			} else if (idx == stepIdx) {
				iconClass = 'icon-circle';
				classesToRemove = settings.pendingClass + ' ' + settings.completedClass;
				classesToAdd = settings.activeClass;
			} else {
				iconClass = 'icon-circle';
				classesToRemove = settings.activeClass + ' ' + settings.completedClass;
				classesToAdd = settings.pendingClass;
			}
			$this.removeClass(classesToRemove).addClass(classesToAdd);
			$this.find('i').removeClass().addClass(iconClass);
		});
		this.options.currentStep = stepIdx;
		this.$this.trigger('spChangeStep');
		$("#back").show();
		if (this.options.currentStep == 1) {
			$("#back").prop('disabled', true);
		} else {
			$("#back").prop('disabled', false);
		}
	};

	var _isLast = function () {
		return this.options.currentStep >= this.$this.find('li').length;
	};

	var _allowSaveProfile = function (flag) {
		if (flag) {
			$("#back").hide();
			$("#save").show();
		} else {
			$("#back").show();
			$("#save").hide();
		}
		
	};

	var _showButtons = function (showBack, showNext, showSave) {
		$('#back').prop('disabled', !showBack);

		if (showNext) {
			$('#next').show();
		}
		else {
			$('#next').hide();
		}

		if (showSave) {
			$('#save').show();
		}
		else {
			$('#save').hide();
		}
	}

	$.stepProgress = $.stepProgress || {};

	$.extend($.stepProgress, {
		buildStep: _buildStep,
		next: _next,
		back: _back,
		goToStep: _goToStep,
		last: _isLast,
		allowSaveProfile: _allowSaveProfile,
		showButtons: _showButtons
	});

	$.fn.stepProgress = function (options, step) {
		var callArgs = arguments;
		if (typeof options === "string") {
			var fn = $.stepProgress[options];
			if (!fn) {
				throw 'stepProgress - No such method' + options;
			}
			var args = $.makeArray(callArgs).slice(1);
			return fn.apply(this[0], args); //if you want more than just ID you will have to fix this
		}
		return this.each(function () {
			$.fn.stepProgress.defaults = {
				currentStep: step || 1,
				steps: []
			};

			var local = $.extend({}, $.fn.stepProgress.defaults, options);
			this.options = local;
			var self = this;
			self.$this = $(this);
			self.$this.empty();
			var ul = '<ul class="step-progress-bar" data-steps="' + local.steps.length + '">';
			$.each(local.steps, function (idx) {
				ul += $.stepProgress.buildStep.call(self, {
					step: idx + 1,
					options: this
				});
			});
			ul += '</ul>';
			self.$this.append(ul);
			$.stepProgress.goToStep.call(this, this.options.currentStep);
		});
	};
})(jQuery);