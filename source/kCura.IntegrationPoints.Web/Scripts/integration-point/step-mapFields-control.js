var IP = IP || {};

IP.workspaceFieldsControls = (function () {
	function add(from, fields, destination) {
		var to = destination;
		move(fields(), to);
		from.removeAll(fields());
		fields.splice(0, fields().length);
	}

	function addAll(from, fields, destination) {
		var to = destination;
		move(from(), to);
		from.removeAll();
		fields.splice(0, fields().length);
	}
	var move = function (from, to) {
		$.each(from, function () {
			to.push(this);
		});
	}
	var down = function (field, selected) {
		for (var j = selected().length - 1; j >= 0 ; j--) {
			var i = field().indexOf(selected()[j]);
			var length = field().length - 1;
			if ((i + 1) <= length) {
				var array = field();
				field.splice(i, 2, array[i + 1], array[i]);
			} else {
				break;
			}
		}
	}
	var up = function (field, selected) {
		for (var j = 0; j < selected().length ; j++) {
			var i = field.indexOf(selected()[j]);
			if (i >= 1) {
				var array = field();
				field.splice(i - 1, 2, array[i], array[i - 1]);
			} else {
				break;
			}
		}
	}

	return {
		add: add,
		addAll: addAll,
		down: down,
		up: up
	}
})();