var IP = IP || {};

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

	return {
		add: add,
		addAll: addAll,
		down: down,
		up: up
	};

	
})();

IP.affects = (function() {

	var hover = function () {
		
		
		$('#selected-workspace-fields').on('mouseenter', function (argument) {
			
			if ($(argument.currentTarget).eq(0).is("select")) {
				var index = $('#selected-workspace-fields')[0].length-1;
				var $soureField = $("#selected-source-fields option").eq(index);
				var $workspaceField = $('#selected-workspace-fields option').eq(index);
				$soureField.addClass("hover");
				$workspaceField.addClass("hover");
			}
		});
		$('#selected-workspace-fields').on('mouseleave', function (argument) {
			if ($(argument.currentTarget).eq(0).is("select")) {
				var index = $('#selected-workspace-fields')[0].length - 1;
				var $soureField = $("#selected-source-fields option").eq(index);
				var $workspaceField = $('#selected-workspace-fields option').eq(index);
				$soureField.removeClass("hover");
				$workspaceField.removeClass("hover");
			}
		});
		$('#selected-source-fields').on('mouseenter', function (argument) {
			if ($(argument.currentTarget).eq(0).is("select")) {
				var index = $('#selected-source-fields')[0].length - 1;
				var $soureField = $("#selected-source-fields option").eq(index);
				var $workspaceField = $('#selected-workspace-fields option').eq(index);
				$soureField.addClass("hover");
				$workspaceField.addClass("hover");
			}
		});
		$('#selected-source-fields').on('mouseleave', function (argument) {
			if ($(argument.currentTarget).eq(0).is("select")) {
				var index = $('#selected-source-fields')[0].length - 1;
				var $soureField = $("#selected-source-fields option").eq(index);
				var $workspaceField = $('#selected-workspace-fields option').eq(index);
				$soureField.removeClass("hover");
				$workspaceField.removeClass("hover");
			}
		});

		$('#selected-workspace-fields').on('mouseenter', 'option', function () {
			var index = $(this).index();
			var soureField = $("#selected-source-fields option")[index];
			$(soureField).addClass("hover");
			$(this).addClass("hover");

		});
		$('#selected-source-fields ').on('mouseenter', 'option', function () {
			var index = $(this).index();
			var soureField = $("#selected-workspace-fields option")[index];
			$(soureField).addClass("hover");
			$(this).addClass("hover");

		});
		$('#selected-workspace-fields').on('mouseleave', 'option', function () {
			var index = $(this).index();
			var soureField = $("#selected-source-fields option")[index];
			$(soureField).removeClass("hover");
			$(this).removeClass("hover");
		});
		
		$('#selected-source-fields').on('mouseleave', 'option', function () {
			var index = $(this).index();
			var soureField = $("#selected-workspace-fields option")[index];
			$(soureField).removeClass("hover");
			$(this).removeClass("hover");
		});

		//$('#selected-source-fields').on('mouseenter', function () {
		//	var index = $("#selected-workspace-fields option")["last-child"];
		//	var soureField = $("#selected-workspace-fields option")[index];
		//	$(soureField).removeClass("hover");
		//	$(this).removeClass("hover");
		//});
	};
	return {
		hover : hover
	};
})();