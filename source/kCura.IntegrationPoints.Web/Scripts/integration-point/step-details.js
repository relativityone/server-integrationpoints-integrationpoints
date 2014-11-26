var IP = IP || {};
(function (root, ko) {
	var initDatePicker = function ($els) {
		$els.datepicker({
			beforeShow: function (el, inst) {
				if ($(el).attr('readonly')) {
					return false;
				}
				inst.dpDiv.css({ marginTop: -el.offsetHeight + 'px', marginLeft: el.offsetWidth + 5 + 'px' });
				return true;
			},
			onSelect: function () {
				//get the shim to work properly
				$(this).blur();
			}
		});
	}

	root.messaging.subscribe('details-loaded', function () {
		initDatePicker($('#scheduleRulesStartDate, #scheduleRulesEndDate'))
	});


	var Choice = function (name, value) {
		this.displayName = name;
		this.value = value;
	};

	var Source = function () {
		this.templateID = 'ldapSourceConfig';
		var self = this;

		this.sourceTypes = ko.observableArray();

		root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('SourceType') }).then(function (result) {
			var types = $.map(result, function (entry) {
				return new Choice(entry.name, entry.value);
			});
			self.sourceTypes(types);
		});

		this.selectedType = ko.observable();

	};

	var Destination = function () {
		var self = this;

		IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('Test') }).then(function (result) {
			var types = $.map(result, function (entry) {
				return new Choice(entry.m_Item1, entry.m_Item2);
			});
			self.rdoTypes(types);
		}, function () {

		})

		this.templateID = 'ldapDestinationConfig';
		this.rdoTypes = ko.observableArray();
		this.selectedRdo = ko.observable();
	};

	var Scheduler = function () {
		this.templateID = 'schedulingConfig';
	};

	var Model = function () {

		var self = this;
		this.name = {
			label: 'Integration Name:',
			value: ko.observable('')
		};

		this.source = new Source();

		this.destination = new Destination();
		this.overwrite = ko.observableArray([
			new Choice('Append', 1234),
			new Choice('Append Overlay', 5678)
		]);
		this.selectedOverwrite = ko.observable();
		this.scheduler = new Scheduler();
	};

	var Step = function (settings) {
		var self = this;
		self.settings = settings;
		this.template = ko.observable();
		this.hasTemplate = false;
		this.model = new Model();

		this.getTemplate = function () {
			IP.data.ajax({ dataType: 'html', cache: true, type: 'get', url: self.settings.url }).then(function (result) {
				$('body').append(result);
				self.hasTemplate = true;
				self.template(self.settings.templateID);
				root.messaging.publish('details-loaded');
			});
		};

		this.submit = function () {
			var d = root.data.deferred().defer();
			d.resolve();
			return d.promise;
		};
	};

	var step = new Step({
		url: IP.utils.generateWebURL('IntegrationPoints', 'StepDetails'),
		templateID: 'step1'
	});

	root.points.steps.push(step);

})(IP, ko);

(function () {
	var toggleScheduler = function (func) {
		D('.dragon-panel')[func]();
	}
	
	IP.messaging.subscribe('details-loaded', function () {
		D('.dragon-panel').scheduleControl();

		$('.required.value').each(function () {
			var $this = $(this);
			$this.siblings('.title').addClass('required');
		});

		var $el = $('#scheduleRulesFrequency, #scheduleRulesSendOnControl select');

		$el.select2({
			dropdownAutoWidth: false,
			dropdownCssClass: "filter-select",
			containerCssClass: "filter-container",
		});
		$el.parent().find('.filter-container span.select2-arrow').removeClass("select2-arrow").addClass("icon icon-chevron-down");

		$el.on('change', function () {
			D.updatePlaceholders();
		});
		var enabled = $('#scheduleRulesEnabled input:checked').val() === "true";

		if (!enabled) {
			toggleScheduler('hide');
		} else {
			toggleScheduler('show');
		}

		$('#scheduleRulesEnabled input').on('change', function () {
			var enabled = $(this).val() === "true";
			var func = enabled ? "show" : "hide";
			toggleScheduler(func);

			D.updatePlaceholders();
		});

	});



})();