var IP = IP || {};

IP.workspaceFieldsControls = (function () {
	function udpateMappingType() {		
		IP.mappingType = mappingType.Manual;		
    }

	function add(from, fields, destination) {
		var to = destination;
		move(fields(), to);
		from.removeAll(fields());
		fields.splice(0, fields().length);
		udpateMappingType();
	};

	function addAll(from, fields, destination) {
		var to = destination;
		move(from(), to);
		from.removeAll();
		fields.splice(0, fields().length);
		udpateMappingType();
	};

	var move = function(from, to) {
		$.each(from, function() {
			to.push(this);
		});
		udpateMappingType();
	};
	var down = function(field, selected) {
		for (var j = selected().length - 1; j >= 0; j--) {
			var i = field().indexOf(selected()[j]);
			var length = field().length - 1;
			if ((i + 1) <= length) {
				var array = field();
				field.splice(i, 2, array[i + 1], array[i]);
			} else {
				break;
			}
		}
		udpateMappingType();
	};

	var up = function(field, selected) {
		for (var j = 0; j < selected().length; j++) {
			var i = field.indexOf(selected()[j]);
			if (i >= 1) {
				var array = field();
				field.splice(i - 1, 2, array[i], array[i - 1]);
			} else {
				break;
			}
		}
		udpateMappingType();
	};
	function moveField(source, oldIndex, newIndex) {
		if (newIndex >= source.length) {
			var k = newIndex - source.length;
			while ((k--) + 1) {
				source.push(undefined);
			}
		}
		source.splice(newIndex, 0, source.splice(oldIndex, 1)[0]);
		udpateMappingType();
	};


	var _moveBottom = function (source, selected) {
		if (selected.length > 0) {
			var evaled = source();
			//only move the top most one
			selected = selected[0];
			var idx = evaled.indexOf(selected);
			moveField(evaled, idx, evaled.length - 1);
			source.valueHasMutated();
		}
	};

	var _moveTop = function (source, selected) {
		if (selected.length > 0) {
			var evaled = source();
			//only move the top most one
			selected = selected[0];
			var idx = evaled.indexOf(selected);
			moveField(evaled, idx, 0);
			source.valueHasMutated();
		}
	};
	return {
		add: add,
		addAll: addAll,
		down: down,
		up: up,
		moveBottom: _moveBottom,
		moveTop : _moveTop
	};

	
})();

IP.affects = (function() {

	var hover = function () {
		var SOURCE_FIELD = '#selected-source-fields';
		var WORKSPACE_FIELD = '#selected-workspace-fields'
		var HOVER_CLASS = 'hover';


		var _forceIERedraw = function () {
			$('#forceRedraw').text(1); //force IE to redraw
		};

		// This fixes the field mapping pane alignment in IE 10 for screen widths greater than ~2300px
		var isIe10 = navigator.userAgent.toLowerCase().indexOf('msie 10') > -1;
		if (isIe10) {
			var mapFields = $('body').find("#mapFields");
			var fieldMappings = $('body').find("#fieldMappings");

			if (fieldMappings.length > 0 && mapFields.length > 0) {
				mapFields.css("position", "absolute");
				mapFields.css("top", "85px");
				fieldMappings.css("min-width", "1250px");
				fieldMappings.css("min-height", "490px");
			}
		}

		var _init = function (source, destination) {
			$(source).on('mousemove', function (event) {
				var $this = $(this);
				var $opt = $this.find('option:hover');
				var idx = $opt.index();
				$opt.siblings().removeClass(HOVER_CLASS);
				if (idx < 0) {
					idx = $this.find('option').length - 1;
				}
				$this.find('option').eq(idx).addClass(HOVER_CLASS);
				$(destination).find('option').removeClass(HOVER_CLASS).eq(idx).addClass(HOVER_CLASS);
				_forceIERedraw();
			});
		};

		var removeHoverClass = function (source, destination) {
			$(source).on('mouseleave', function (event) {
				$(this).find('option').removeClass(HOVER_CLASS);
				$(destination).find('option').removeClass(HOVER_CLASS);
				_forceIERedraw();
			});
		};

		
		removeHoverClass(WORKSPACE_FIELD, SOURCE_FIELD);
		removeHoverClass(SOURCE_FIELD, WORKSPACE_FIELD);
		_init(WORKSPACE_FIELD, SOURCE_FIELD);
		_init(SOURCE_FIELD, WORKSPACE_FIELD);
	};
	return {
		hover : hover
	};
})();