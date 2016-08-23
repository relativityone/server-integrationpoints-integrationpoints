(function (root, $) {
	function StepDefinitionProvider($) {
		self = this;
		var ProviderState = {
			Init: 0,
			InProgress: 1,
			IsSet: 2
		};
		self._state = ProviderState.Init;
		self._stepsOptions = {
			initsteps: [
				{
					text: 'Setup'
				},
				{
					text: 'Complete the Setup'
				}
			],
			inProgress: [
				{
					text: 'Setup'
				},
				{
					text: 'Loading...'
				},
				{
					text: 'Loading...'
				}
			],
			defaultsteps: [
				{
					text: 'Setup'
				},
				{
					text: 'Connect to Source'
				},
				{
					text: 'Map Fields'
				}
			]
		};

		self._updateDOM = function (options) {
			$('#stepProgress').stepProgress({
				steps: options
			});
		};

		self._init = function () {
			self._state = ProviderState.Init;
			self._updateDOM(self._stepsOptions['initsteps']);
		};

		self._loadDefaults = function () {
			self._state = ProviderState.IsSet;
			self._updateDOM(self._stepsOptions['defaultsteps']);
			
		};

		self._setStateInProgress = function () {
			self._state = ProviderState.InProgress;
			self._updateDOM(self._stepsOptions['inProgress']);
		};

		self._loadOverride = function (optionsOverride) {
			self._state = ProviderState.IsSet;
			self._updateDOM(optionsOverride);
		};

		return {
			init: self._init,
			loadDefaults: self._loadDefaults,
			setStateInProgress: self._setStateInProgress,
			loadOverride: self._loadOverride
		};

	};

	root.stepDefinitionProvider = new StepDefinitionProvider($);

})(IP, jQuery);
