﻿var IP = IP || {};

IP.workspaceFieldsControls = (function () {
	function add(from, fields, destination) {
		var to = destination;
		move(fields(), to);
		from.removeAll(fields());
		fields.splice(0, fields().length);
	};

	function addAll(from, fields, destination) {
		var to = destination;
		move(from(), to);
		from.removeAll();
		fields.splice(0, fields().length);
	};

	var move = function(from, to) {
		$.each(from, function() {
			to.push(this);
		});
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
	};
	function moveField(source, oldIndex, newIndex) {
		if (newIndex >= source.length) {
			var k = newIndex - source.length;
			while ((k--) + 1) {
				source.push(undefined);
			}
		}
		source.splice(newIndex, 0, source.splice(oldIndex, 1)[0]);
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
		var _init = function (source, destination) {
			$(source).on('mousemove', function (event) {
				var $this = $(this);
				var $opt = $this.find('option:hover');
				var idx = $opt.index();
				$opt.siblings().removeClass('hover');
				if (idx < 0) {
					idx = $this.find('option').length - 1;
				}
				$this.find('option').eq(idx).addClass('hover');
				$(destination).find('option').removeClass('hover').eq(idx).addClass('hover');
				$('#forceRedraw').text(idx); //force IE to redraw
			});
		}

		var removeHoverClass = function (source, destination) {
			$(source).on('mouseleave', function (event) {
				$(this).find('option').removeClass('hover');
				$(destination).find('option').removeClass('hover');
				$('#forceRedraw').text(1); //force IE to redraw
			});
		}
		removeHoverClass('#selected-workspace-fields', '#selected-source-fields');
		removeHoverClass('#selected-source-fields', '#selected-workspace-fields');
		_init('#selected-workspace-fields', '#selected-source-fields');
		_init('#selected-source-fields', '#selected-workspace-fields');
	};
	return {
		hover : hover
	};
})();