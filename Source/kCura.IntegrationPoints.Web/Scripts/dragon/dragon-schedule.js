(function (D, $) {
	var SCHEDULE_STATE = 'dragon-schedule-state';
	var DAY_VALUE = '128';
		
	var _selectors = {
		sendOn: function ($this) { return $this.find('.SendOn-edit-input') },
		frequency: function ($this) { return $this.find('.Select-edit-input select') },
		reoccur: function ($this) { return $this.find('[name="Reoccur"]'); },
		selectedWeeklyDays: function ($this) { return $this.find('#dayOfMonth') },
		selectedWeekOfMonth: function ($this) { return $this.find('#weekOfMonth') },
		occuranceSelect: function ($this) { return $this.find('#occuranceOfMonth'); },
		daySelect: function ($this) { return $this.find('#daySelect'); },
		dayLabel: function ($this) { return $this.find('#monthlyDayLabel') }

	}
	var _parsers = (function () {
		var _weeklyParser = function ($checked) {
			return {
				SelectedWeeklyDays: $.map($checked, function (el) {
					return $(el).parent().data('value');
				})
			};
		};

		var _monthlyParser = function ($checked) {
			var retValue = {};
			var internalOptions = options(this);
			switch ($checked.val()) {
				case 'day':
					retValue.DayOfTheMonth = D.parseInt($checked.parent().find('select').val());
					break;
				case 'monthWeek':
					retValue.SelectedWeeklyDays = [internalOptions.$daySelect.find('option:selected').text()];
					retValue.SelectedWeekOfMonth = D.parseInt(internalOptions.$occuranceSelect.val());
					break;
				default:
					throw new Error($checked.val() + " not supported");
			}
			return retValue;
		};
		//these are the only type of parsers that are exposed,
		//but can be extended/changed by outside code
		return {
			'monthly': _monthlyParser,
			'weekly': _weeklyParser
		};

	})();

	function findSendOnHidden(options) {
		//this may not be the best selector, but for now
		return options.$sendOn.find('input:hidden[data-parse]');
	}

	var _manageState = function (scheduleOptions) {
		var suffixText = {
			"monthly": 'month(s)',
			"weekly": 'week(s)'
		};

		//hard coded for text for now but value would be way better
		var state = scheduleOptions.$frequency.find(':selected').text().toLowerCase();
		scheduleOptions.$sendOn.find('[data-type]').hide();
		var $type = scheduleOptions.$sendOn.find('[data-type="' + state + '"]').show();
		if (state === 'daily' || state === 'select ...') {
			scheduleOptions.$reoccur.parents('.input-control').hide();
		} else {
			scheduleOptions.$reoccur.parents('.input-control').show();
		}
		scheduleOptions.$reoccur.parent().find('[data-suffix]').text(suffixText[state]);
		scheduleOptions.$sendOn.toggle($type.length !== 0);
		scheduleOptions.$this.data(SCHEDULE_STATE, state);
		findSendOnHidden(scheduleOptions).val(state); //Do not touch this it's important for validation
	}

	var _manageDayState = function (scheduleOptions, val) {
		var hiddenClass = 'hidden';
		var $el = scheduleOptions.$daySelect.find('option[value="' + DAY_VALUE + '"]');
		if (val === '1' || val === '5') {
			$el.removeClass(hiddenClass);
		} else {
			$el.addClass(hiddenClass);
			var currentOptions = scheduleOptions.$daySelect.select2("val");
			if (currentOptions === DAY_VALUE) {
				scheduleOptions.$daySelect.select2("val", "1");
			}
		}

	};

	function options(el, data) {
		var $this = $(el);
		if (arguments.length == 1) {
			//get
			return $this.data('scheduleOptions');
		} else {
			//set
			return $this.data('scheduleOptions', data);
		}
	};

	var _parse = function () {
		var ops = options(this),
				text = ops.$this.data(SCHEDULE_STATE),
				parseFunc = D.scheduleControl.parsers[text],
				retValue = {},
				$checked;

		if ($.isFunction(parseFunc)) {
			$checked = ops.$sendOn.find('[data-type="' + text + '"]').find("input:checked");
			retValue = parseFunc.call(this, $checked);
		}

		return retValue;
	};

	function manageDayMessage(scheduleOptions, day) {
		if (day > 28) {
			//show message
			var message = "Months with fewer than {0} days will send on the last day of the month."
			scheduleOptions.$dayLabel.show().text(D.format(message, day));
		} else {
			scheduleOptions.$dayLabel.hide();
		}
	}

	D.scheduleControl = function () {
		return this.each(function () {
			var self = this;
			var $this = $(this);

			var selectors = D.scheduleControl.selectors;
			var scheduleOptions = {};
			scheduleOptions.$this = $this;
			for (var key in selectors) {
				if (selectors.hasOwnProperty(key)) {
					var str = '$' + key;
					scheduleOptions[str] = selectors[key]($this);
				}
			}
			scheduleOptions.$sendOn.find('[data-type]').hide();

			options(this, scheduleOptions);
			manageDayMessage(scheduleOptions, D.parseInt(scheduleOptions.$selectedWeeklyDays.val()));

			scheduleOptions.$selectedWeeklyDays.on('change', function () {
				var day = D.parseInt(this.value);
				manageDayMessage(scheduleOptions, day);
			});

			D.scheduleControl.manageState(scheduleOptions);
			scheduleOptions.$frequency.on('change', function () {
				D.scheduleControl.manageState(options(self));
			});
			D.scheduleControl.manageDayState(options(self), scheduleOptions.$occuranceSelect.val());
			scheduleOptions.$occuranceSelect.on('change', function (e) {
				D.scheduleControl.manageDayState(options(self), e.val);
			});

			//set default state to monday
			var state = scheduleOptions.$frequency.find(':selected').text().toLowerCase();
			if (state != 'monthly') {
				scheduleOptions.$daySelect.select2("val", "1");
			}
			//used by dragon form grid
			var parser = {
				parse: function () {
					return _parse.call(self);
				}
			};
			findSendOnHidden(scheduleOptions).data("control", parser);
		});
	};

	//expose public static methods
	$.extend(D.scheduleControl, {
		parsers: _parsers,
		selectors: _selectors,
		manageState: _manageState,
		manageDayState: _manageDayState
	});

	//make it available to the dragon Prototype
	$.extend(D.prototype, {
		scheduleControl: D.scheduleControl
	});

})(Dragon, jQuery);