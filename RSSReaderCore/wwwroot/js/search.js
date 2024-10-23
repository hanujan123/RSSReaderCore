		//search funncctionality
		$("#strSearchInput").on("change input", function () {
				var searchStr = $("#strSearchInput")[0].value;
				if (searchStr.length > 2) {
						$("#SearchContainer .searchElement .str:not(:contains('" + searchStr + "'))").parent().hide();
						$("#SearchContainer .searchElement .str:contains('" + searchStr + "')").parent().show();
				}
				else {
						$("#SearchContainer .searchElement").show();
				}
		});